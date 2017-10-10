using System.Windows;
using PetitsPains.Model;
using PetitsPains.ViewModel;

namespace PetitsPains.View
{
    /// <summary>
    /// Interaction logic for UpdatePersonWindow.xaml
    /// </summary>
    public partial class UpdatePersonWindow : Window, IClosable, IDialog
    {
        /// <summary>View model for the view.</summary>
        public UpdatePersonViewModel ViewModel { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lineToUpdate">Line to update.</param>
        public UpdatePersonWindow(Line lineToUpdate)
        {
            // Instanciate the view model and set it as the data context.
            ViewModel = new UpdatePersonViewModel(lineToUpdate);
            this.DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
