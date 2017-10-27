using System;
using PetitsPains.Model;
using PetitsPains.Utils;

namespace PetitsPains.Resources
{
    /// <summary>
    /// Custom implementation of the EmailTemplate class.
    /// </summary>
    partial class EmailTemplate
    {
        /// <summary>Date at which the report was created.</summary>
        private DateTime date;

        /// <summary>Collection of lines to send in the email.</summary>
        private ItemsChangeObservableCollection<Line> lines;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="date">Date at which the report was created.</param>
        /// <param name="lines">Collection of lines.</param>
        public EmailTemplate(DateTime date, ItemsChangeObservableCollection<Line> lines)
        {
            this.date = date;
            this.lines = lines;
        }
    }
}
