using UnityEngine;

namespace AstroDX.UI.RecyclingScrollRect.Interfaces
{
    public abstract class ScrollViewItem<T> : MonoBehaviour
    {
        public T   Data  { get; private set; }
        public int Index { get; private set; }

        public void SetData(T data, int index)
        {
            Data  = data;
            Index = index;

            OnDataUpdated();
        }

        /// <summary>
        /// This is called whenever this instance is requested to render a new item with a different index.
        /// </summary>
        protected abstract void OnDataUpdated();

        /// <summary>
        /// When this object instance scrolls out of view, this is called to disable relevant components;
        /// Should be as lightweight as possible since disabled items might get reused instantly.
        /// </summary>
        internal abstract void DisableItem();

        /// <summary>
        /// Called when an item is retrieved from the pool.
        /// </summary>
        internal abstract void EnableItem();
    }
}