using System;
using PetitsPains.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using PetitsPains.Model;
using PetitsPains.Enums;

namespace PetitsPainsTest
{
    /// <summary>
    /// Class to test the extension methods.
    /// </summary>
    [TestClass]
    public class TestExtensions
    {
        // Create a collection and items to add to it to test the sorting methods.
        ObservableCollection<Croissant> unorderedCollection;
        Croissant croissant1;
        Croissant croissant2;
        Croissant croissant3;
        Croissant croissant4;
        Croissant croissant5;
        Croissant croissant6;
        Croissant croissant7;

        /// <summary>
        /// Initialises the tests.
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            unorderedCollection = new ObservableCollection<Croissant>();

            croissant1 = new Croissant(new DateTime(2017, 09, 05));
            croissant2 = new Croissant();
            croissant3 = new Croissant(new DateTime(2017, 09, 03));
            croissant4 = new Croissant(new DateTime(2017, 09, 10));
            croissant5 = new Croissant();
            croissant6 = new Croissant(new DateTime(2017, 09, 02));
            croissant7 = new Croissant(new DateTime(2017, 09, 01));

            unorderedCollection.Add(croissant1);
            unorderedCollection.Add(croissant2);
            unorderedCollection.Add(croissant3);
            unorderedCollection.Add(croissant4);
            unorderedCollection.Add(croissant5);
            unorderedCollection.Add(croissant6);
            unorderedCollection.Add(croissant7);
        }

        /// <summary>
        /// Tests the default sorting method.
        /// </summary>
        [TestMethod]
        public void TestBubbleSortDefault()
        {
            bool result = false;

            var sortedCollection = new ObservableCollection<Croissant>(unorderedCollection);

            sortedCollection.BubbleSort();

            // Default behavior: ascending sort
            result = (sortedCollection[0].Date == croissant7.Date &&
                sortedCollection[1].Date == croissant6.Date &&
                sortedCollection[2].Date == croissant3.Date &&
                sortedCollection[3].Date == croissant1.Date &&
                sortedCollection[4].Date == croissant4.Date &&
                sortedCollection[5].Date == null &&
                sortedCollection[6].Date == null);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests the explicit descending sort method.
        /// </summary>
        [TestMethod]
        public void TestBubbleSortExplicitAscending()
        {
            bool result = false;

            var sortedCollection = new ObservableCollection<Croissant>(unorderedCollection);

            sortedCollection.BubbleSort(SortDirection.Ascending);

            result = (sortedCollection[0].Date == croissant7.Date &&
                sortedCollection[1].Date == croissant6.Date &&
                sortedCollection[2].Date == croissant3.Date &&
                sortedCollection[3].Date == croissant1.Date &&
                sortedCollection[4].Date == croissant4.Date &&
                sortedCollection[5].Date == null &&
                sortedCollection[6].Date == null);

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests the explicit descending sort method.
        /// </summary>
        [TestMethod]
        public void TestBubbleSortExplicitDescending()
        {
            bool result = false;

            var sortedCollection = new ObservableCollection<Croissant>(unorderedCollection);

            sortedCollection.BubbleSort(SortDirection.Descending);

            result = (sortedCollection[0].Date == null &&
                sortedCollection[1].Date == null &&
                sortedCollection[2].Date == croissant4.Date &&
                sortedCollection[3].Date == croissant1.Date &&
                sortedCollection[4].Date == croissant3.Date &&
                sortedCollection[5].Date == croissant6.Date &&
                sortedCollection[6].Date == croissant7.Date);

            Assert.IsTrue(result);
        }
    }
}
