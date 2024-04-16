using System.Collections.Generic;
using AstroDX.UI.RecyclingScrollRect.Interfaces;
using AstroDX.Utilities.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AstroDX.UI.RecyclingScrollRect.ScrollRects
{
    public class RecyclingVerticalListView<TData> : ScrollRectRecyclingBase<TData>
    {
        [BeginGroup("Scrolling")]
        [SerializeField]
        private float gap;

        [SerializeField]
        private float marginTop;

        [SerializeField]
        [EndGroup]
        private float marginBottom;

        [Disable, SerializeField]
        private float itemHeight;

        [Disable, SerializeField]
        private int maxVisibleCount;

        [Disable, SerializeField]
        private float contentHeight;

        private void OnValidate()
        {
            itemHeight = itemPrefab?.GetRectTransform().sizeDelta.y ?? 0;

            scrollRect.content.anchorMin  = new Vector2(0,    1);
            scrollRect.content.anchorMax  = new Vector2(1,    1);
            scrollRect.content.pivot      = new Vector2(0.5f, 1);
            scrollRect.content.localScale = Vector3.one;

            var dt = new DrivenRectTransformTracker();
            dt.Clear();

            //Object to drive the transform
            dt.Add(this, scrollRect.content, DrivenTransformProperties.All);
        }

        private void Update()
        {
            if (scrollRect.velocity.y == 0)
                return;

            UpdateVisibleRange(false);
        }

        protected override void ReloadItems()
        {
            maxVisibleCount = Mathf.CeilToInt((scrollRect.viewport.rect.size.y + gap) / (itemHeight + gap));

            contentHeight                = marginTop + (itemHeight + gap) + marginBottom;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, contentHeight);

            UpdateVisibleRange(true);
        }

        private readonly HashSet<int> _itemsToAdd = new();

        private int _previousStartIndexInclusive;
        private int _previousEndIndexExclusive;

        protected override void UpdateVisibleRange(bool resetActiveItems)
        {
            var startIndexInclusive = scrollRect.content.anchoredPosition.y <= marginTop
                                          ? 0
                                          : Mathf.FloorToInt((scrollRect.content.anchoredPosition.y - marginTop) /
                                                             (itemHeight                            + gap));
            var endIndexExclusive = Mathf.Min(startIndexInclusive + maxVisibleCount, DataSource.Count);

            // If the visible range hasn't changed, just update the positions of all items in range
            if (startIndexInclusive == _previousStartIndexInclusive &&
                endIndexExclusive   == _previousEndIndexExclusive)
            {
                foreach (var item in ShownItemsUnordered)
                {
                    var position = item.GetRectTransform().anchoredPosition;
                    position.y                               = AbsolutePositionAtIndex(item.Index);
                    item.GetRectTransform().anchoredPosition = position;

                    if (resetActiveItems)
                        item.SetData(DataSource[item.Index], item.Index);
                }

                return;
            }

            _previousStartIndexInclusive = startIndexInclusive;
            _previousEndIndexExclusive   = endIndexExclusive;

            // Populate the list of items that should be shown by the end of this procedure
            for (var i = startIndexInclusive; i < endIndexExclusive; i++)
                _itemsToAdd.Add(i);

            // Find and skip items that are already shown
            // Since the shown items are unordered (for example, we may request to show 1 after 2 and 3 are shown, resulting in [2, 3, 1]),
            // We can't make any assumptions about items not appearing later in the list (e.g. [1, 2, 4, 5, 6, 3])
            for (var i = 0; i < ShownItemsUnordered.Count; i++)
            {
                var item = ShownItemsUnordered[i];

                if (_itemsToAdd.Contains(item.Index))
                {
                    var position = item.GetRectTransform().anchoredPosition;
                    position.y                               = AbsolutePositionAtIndex(item.Index);
                    item.GetRectTransform().anchoredPosition = position;

                    if (resetActiveItems)
                        item.SetData(DataSource[item.Index], item.Index);

                    _itemsToAdd.Remove(item.Index);
                }
                else
                {
                    // We don't need to render this item in the new visible range, so we can safely release it
                    itemsPool.Release(item);

                    // Modifying a list while enumerating it isn't recommended
                    // But we're doing it here for performance
                    ShownItemsUnordered.RemoveAt(i);
                    i--;
                }
            }

            // Lastly, generate the actual missing items
            foreach (var index in _itemsToAdd)
            {
                var itemInstance = itemsPool.Get();
                ShownItemsUnordered.Add(itemInstance);
                itemInstance.SetData(DataSource[index], index);

                var position = itemInstance.GetRectTransform().anchoredPosition;
                position.y                                       = AbsolutePositionAtIndex(index);
                itemInstance.GetRectTransform().anchoredPosition = position;
            }

            _itemsToAdd.Clear();
        }

        public override void JumpToIndex(int index)
        {
            scrollRect.velocity                   = new Vector3(0, 0);
            scrollRect.verticalNormalizedPosition = GetScrollPosition(index);
            UpdateVisibleRange(false);
        }

        private Tweener _positionTween;

        public override void ScrollToIndex(int index, float duration, Ease easing = default)
        {
            scrollRect.velocity = Vector2.zero;
            _positionTween?.Kill();
            _positionTween = DOTween.To(() => scrollRect.verticalNormalizedPosition, p =>
            {
                scrollRect.verticalNormalizedPosition = p;
                UpdateVisibleRange(false);
            }, GetScrollPosition(index), duration).SetEase(easing);
        }

        private float GetScrollPosition(int index)
        {
            // absolute position at index starts 0 at the top, and decreases when descending
            var normalizedPosition = 1 + AbsolutePositionAtIndex(index) / contentHeight;
            normalizedPosition -= ((itemHeight + marginBottom) * (1 - normalizedPosition) - marginTop * normalizedPosition) /
                                  contentHeight;

            return normalizedPosition;
        }

        public override void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _positionTween?.Kill();
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            UpdateVisibleRange(false);
        }

        private float AbsolutePositionAtIndex(int index)
        {
            return -index * (itemHeight + gap) - marginTop;
        }
    }
}
