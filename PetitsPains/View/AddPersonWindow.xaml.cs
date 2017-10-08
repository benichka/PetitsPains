using System.Collections.ObjectModel;
using System.Windows;
using PetitsPains.Model;
using PetitsPains.ViewModel;

namespace PetitsPains.View
{
    /// <summary>
    /// Interaction logic for AddPersonWindow.xaml
    /// </summary>
    public partial class AddPersonWindow : Window, IClosable, IDialog
    {
        /// <summary>View model for the view.</summary>
        public AddPersonViewModel ViewModel { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="lines">List of lines to add a person.</param>
        public AddPersonWindow(ObservableCollection<Line> lines)
        {
            // Instanciate the view model and set it as the data context.
            ViewModel = new AddPersonViewModel(lines);
            this.DataContext = ViewModel;

            InitializeComponent();
        }
    }
}
