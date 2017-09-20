using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using PetitsPains.ViewModel;

namespace PetitsPains.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>Page view model</summary>
        public MainWindowViewModel ViewModel { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public MainWindow()
        {
            this.DataContext = this;

            // By default, the language of the UI is set to en-us. We force the culture to
            // be the one that is running on the user's desktop.
            this.Language = XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.Name);

            InitializeComponent();
        }
    }
}
