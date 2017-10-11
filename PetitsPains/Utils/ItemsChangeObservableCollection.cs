using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace PetitsPains.Utils
{
    // Taken from https://www.codeproject.com/tips/694370/how-to-listen-to-property-chang

    /// <summary>
    /// This class adds the ability to refresh the list when any property of
    /// the objects changes in the list which implements the INotifyPropertyChanged.
    /// </summary>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    public class ItemsChangeObservableCollection<T> :
           ObservableCollection<T> where T : INotifyPropertyChanged
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public ItemsChangeObservableCollection()
        { }

        /// <summary>
        /// Constructor that takes an IEnumerable&lt;T&gt; as parameter.
        /// </summary>
        /// <param name="enumerable">Object that implements IEnumerable&lt;T&gt;</param>
        public ItemsChangeObservableCollection(IEnumerable<T> enumerable) : base(enumerable)
        { }

        /// <summary>
        /// Constructor that takes a List&lt;T&gt; as parameter.
        /// </summary>
        /// <param name="enumerable">Object that implements IEnumerable&lt;T&gt;</param>
        public ItemsChangeObservableCollection(List<T> list) : base(list)
        { }

        /// <summary>
        /// Action to take when the collection changed.
        /// </summary>
        /// <param name="e">Action operated on the collection.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                RegisterPropertyChanged(e.NewItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                UnRegisterPropertyChanged(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                UnRegisterPropertyChanged(e.OldItems);
                RegisterPropertyChanged(e.NewItems);
            }

            base.OnCollectionChanged(e);
        }

        /// <summary>
        /// Unsubscribes all elements in the collection and delete them.
        /// </summary>
        protected override void ClearItems()
        {
            UnRegisterPropertyChanged(this);
            base.ClearItems();
        }

        /// <summary>
        /// Subscribe to the PropertyChanged event of each item in the collection.
        /// </summary>
        /// <param name="items">Items in the collection.</param>
        private void RegisterPropertyChanged(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged += new PropertyChangedEventHandler(Item_PropertyChanged);
                }
            }
        }

        /// <summary>
        /// Unsubscribe to the PropertyChanged event of each item in the collection.
        /// </summary>
        /// <param name="items">Items in the collection.</param>
        private void UnRegisterPropertyChanged(IList items)
        {
            foreach (INotifyPropertyChanged item in items)
            {
                if (item != null)
                {
                    item.PropertyChanged -= new PropertyChangedEventHandler(Item_PropertyChanged);
                }
            }
        }

        /// <summary>
        /// Action taken when an item in the collection changed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Arguments.</param>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
