using System;
using System.Collections.ObjectModel;

namespace PetitsPains.Utils
{
    public static class Extensions
    {
        public enum SortDirection
        {
            Ascending,
            Descending
        }

        /// <summary>
        /// Do an in-place bubble sort in an ObservableCollection.
        /// </summary>
        /// <typeparam name="T">Type of the collection.</typeparam>
        /// <param name="collection">Collection to sort.</param>
        /// <param name="sortDirection">Is the sort ascending or descending?</param>
        public static void BubbleSort<T>(this ObservableCollection<T> collection, SortDirection? sortDirection) where T : IComparable<T>
        {
            if (sortDirection == null)
            {
                sortDirection = SortDirection.Ascending;
            }

            for (int i = (collection.Count - 1); i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    if (sortDirection == SortDirection.Ascending)
                    {
                        if (collection[j - 1].CompareTo(collection[j]) > 0)
                        {
                            collection.Move(j - 1, j);
                        }
                    }
                    else
                    {
                        if (collection[j - 1].CompareTo(collection[j]) < 0)
                        {
                            collection.Move(j - 1, j);
                        }
                    }
                }
            }
        }
    }
}
