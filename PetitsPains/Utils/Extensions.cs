using System;
using System.Collections.ObjectModel;
using PetitsPains.Enums;

namespace PetitsPains.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// Performs an in-place bubble sort in an ObservableCollection.
        /// </summary>
        /// <typeparam name="T">Type of the collection.</typeparam>
        /// <param name="collection">Collection to sort.</param>
        public static void BubbleSort<T>(this ObservableCollection<T> collection) where T : IComparable<T>
        {
            BubbleSort(collection, SortDirection.Ascending);
        }

        /// <summary>
        /// Performs an in-place bubble sort in an ObservableCollection.
        /// </summary>
        /// <typeparam name="T">Type of the collection.</typeparam>
        /// <param name="collection">Collection to sort.</param>
        /// <param name="sortDirection">Is it an ascending sort or a descending sort?</param>
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
                    if (sortDirection != SortDirection.Ascending)
                    {
                        if (collection[j - 1].CompareTo(collection[j]) < 0)
                        {
                            collection.Move(j - 1, j);
                        }
                    }
                    else
                    {
                        // No error is raised if the sort direction is not in the enum: an ascending sort is made.
                        if (collection[j - 1].CompareTo(collection[j]) > 0)
                        {
                            collection.Move(j - 1, j);
                        }
                    }
                }
            }
        }
    }
}
