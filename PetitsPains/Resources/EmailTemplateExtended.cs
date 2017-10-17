using PetitsPains.Model;
using PetitsPains.Utils;

namespace PetitsPains.Resources
{
    /// <summary>
    /// Custom implementation of the EmailTemplate class.
    /// </summary>
    partial class EmailTemplate
    {
        /// <summary>Collection of lines to send in the email.</summary>
        private ItemsChangeObservableCollection<Line> lines;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lines">Collection of lines.</param>
        public EmailTemplate(ItemsChangeObservableCollection<Line> lines)
        {
            this.lines = lines;
        }
    }
}
