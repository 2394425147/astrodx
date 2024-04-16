using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace AstroDX.UI.RecyclingScrollRect.Interfaces
{
    [RequireComponent(typeof(ScrollRect))]
    public abstract class ScrollRectRecyclingBase<TData> : MonoBehaviour,
                                                           IDragHandler,
                                                           IBeginDragHandler,
                                                           IInitializePotentialDragHandler,
                                                           IEndDragHandler
    {
        protected IList<TData> DataSource { get; private set; } = new List<TData>();

        protected ObjectPool<ScrollViewItem<TData>> itemsPool;

        [field: Disable, SerializeField]
        protected List<ScrollViewItem<TData>> ShownItemsUnordered { get; private set; } = new();

        [BeginGroup("List View")]
        [NotNull, SerializeField, Tooltip("Where the items will be created.")]
        protected ScrollRect scrollRect;

        [NotNull, SerializeField, Tooltip("The visual representation of each item.")]
        [EndGroup]
        protected ScrollViewItem<TData> itemPrefab;

        private bool _initialized;

        protected virtual void OnEnable()
        {
            if (_initialized)
                return;

            itemsPool = new ObjectPool<ScrollViewItem<TData>>(() =>
                                                              {
                                                                  var item = Instantiate(itemPrefab, scrollRect.content);
                                                                  item.DisableItem();
                                                                  return item;
                                                              },
                                                              item => item.EnableItem(),
                                                              item => item.DisableItem());
        }

        public void SetDataSource(IList<TData> dataSource)
        {
            DataSource = dataSource;
            ReloadItems();
        }

        /// <summary>
        /// Reloads the scroll rect to match up with the data source.
        /// </summary>
        protected abstract void ReloadItems();

        /// <summary>
        /// Sets the current visible range, load and discards items depending on the previous range.
        /// <param name="resetActiveItems">Set the item data again to items that can be recycled. Triggered when data source changes.</param>
        /// </summary>
        protected abstract void UpdateVisibleRange(bool resetActiveItems);

        /// <summary>
        /// Jumps to the item in the scroll rect.
        /// </summary>
        public void JumpToItem(TData item)
        {
            // We don't do any error handling here so that the error is thrown and developers using this can debug what's happening.
            JumpToIndex(DataSource.IndexOf(item));
        }

        /// <summary>
        /// Scrolls to the item in the scroll rect.
        /// </summary>
        public void ScrollToItem(TData item, float duration, Ease easing = default)
        {
            // We don't do any error handling here so that the error is thrown and developers using this can debug what's happening.
            ScrollToIndex(DataSource.IndexOf(item), duration, easing);
        }

        /// <summary>
        /// Jumps to the index provided by the data source in the scroll rect.
        /// </summary>
        public abstract void JumpToIndex(int index);

        public abstract void ScrollToIndex(int index, float duration, Ease easing = default);

        protected virtual void OnDestroy() => itemsPool.Dispose();

        public abstract void OnDrag(PointerEventData eventData);

        public abstract void OnBeginDrag(PointerEventData eventData);

        public abstract void OnInitializePotentialDrag(PointerEventData eventData);

        public abstract void OnEndDrag(PointerEventData eventData);
    }
}
