using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using PetitsPains.Command;
using PetitsPains.Data;
using PetitsPains.Model;

namespace PetitsPains.ViewModel
{
    /// <summary>
    /// Technical object used as a parameter for removing a penalty.
    /// </summary>
    public class RemovePenaltyParameter
    {
        /// <summary>Line where the croissant to remove is located.</summary>
        public Line Line { get; set; }

        /// <summary>Croissant to remove penalty from.</summary>
        public Croissant Croissant { get; set; }
    }

    /// <summary>
    /// View model for the MainWindow view
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region properties
        private ObservableCollection<Line> _Lines;
        /// <summary>List of lines.</summary>
        public ObservableCollection<Line> Lines
        {
            get { return this._Lines; }
            private set
            {
                SetProperty(CheckCommands, ref this._Lines, value);

                // Subscribe to each PenaltyAlreadyExistsAtThisDate.
                foreach (var line in this._Lines)
                {
                    line.PenaltyAlreadyExistsAtThisDate += HandlePenaltyAlreadyExistsAtThisDate;
                }
            }
        }

        private Line _SelectedLine;
        /// <summary>Selected line.</summary>
        public Line SelectedLine
        {
            get { return this._SelectedLine; }
            set
            {
                SetProperty(CheckCommands, ref this._SelectedLine, value);
            }
        }

        private readonly string _RootPathOriginal;
        /// <summary>Original root path.</summary>
        public string RootPathOriginal
        {
            get
            {
                return this._RootPathOriginal;
            }
        }

        private string _RoothPath;
        /// <summary>Root path for the files of the application.</summary>
        public string RootPath
        {
            get { return this._RoothPath; }
            private set
            {
                SetProperty(CheckCommands, ref this._RoothPath, value);
            }
        }

        private string _InformationMessage;
        /// <summary>Information message.</summary>
        public string InformationMessage
        {
            get { return this._InformationMessage; }
            set
            {
                // Clear other messages if this message has a content
                if (!String.IsNullOrWhiteSpace(value))
                {
                    ErrorPathInvalid = string.Empty;
                }
                SetProperty(CheckCommands, ref this._InformationMessage, value);
            }
        }

        private string _ErrorPathInvalid;
        /// <summary>Error message to display if the selected folder is not a valid one.</summary>
        public string ErrorPathInvalid
        {
            get { return this._ErrorPathInvalid; }
            private set
            {
                // Clear other messages if this message has a content
                if (!String.IsNullOrWhiteSpace(value))
                {
                    InformationMessage = string.Empty;
                }
                SetProperty(CheckCommands, ref this._ErrorPathInvalid, value);
            }
        }

        private DateTime _ProcessedDate;
        /// <summary>Date to set a penalty on.</summary>
        public DateTime ProcessedDate
        {
            get { return this._ProcessedDate; }
            set
            {
                SetProperty(CheckCommands, ref this._ProcessedDate, value);
            }
        }
        #endregion properties

        #region commands
        /// <summary>Command associated with the button to choose the root folder.</summary>
        public CommandHandler SelectRootPathFolderCommand { get; private set; }

        /// <summary>Command associated with the button to load lines from file.</summary>
        public CommandHandler LoadFileCommand { get; private set; }

        /// <summary>Command associated with the button to add a penalty to a line.</summary>
        public CommandHandler<Line> AddPenaltyCommand { get; private set; }

        /// <summary>Command associated with the button to remove a penalty to a line.</summary>
        public CommandHandler<RemovePenaltyParameter> RemovePenaltyCommand { get; private set; }

        /// <summary>Command to select a line.</summary>
        public CommandHandler<Line> SelectLineCommand { get; private set; }

        /// <summary>Command associated with the validate button.</summary>
        public CommandHandler SaveCommand { get; private set; }
        #endregion commands

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindowViewModel()
        {
            // The commands must be initialised before the fields, because commands are updated
            // as soon as a property changes in our view model.
            SelectRootPathFolderCommand = new CommandHandler(SelectRootPathFolder, () => true);

            // The Load button can not be clicked if the path is empty.
            LoadFileCommand = new CommandHandler(LoadAfterCheck, () => !String.IsNullOrWhiteSpace(RootPath));

            AddPenaltyCommand = new CommandHandler<Line>(AddPenalty, () => true);

            RemovePenaltyCommand = new CommandHandler<RemovePenaltyParameter>(RemovePenalty, CanRemovePenalty);

            SelectLineCommand = new CommandHandler<Line>((line) => SelectedLine = line, () => true);

            // The Save button can not be clicked if the path is empty.
            SaveCommand = new CommandHandler(Save, () => !String.IsNullOrWhiteSpace(RootPath));

            // By default, the proccesed date is the current day.
            ProcessedDate = DateTime.Now;

            // Load the configuration.
            PersonsStore.ReadConfig();

            // Set the original root path.
            this._RootPathOriginal = PersonsStore.RootPath;

            // Retrieve the root path for the files of the application.
            RootPath = PersonsStore.RootPath;

            // Load the lines.
            Load();
        }

        /// <summary>
        /// Event handling for the event PenaltyAlreadyExistsAtThisDate.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandlePenaltyAlreadyExistsAtThisDate(object sender, EventArgs e)
        {
            InformationMessage = String.Format("{0} a déjà une pénalité en date du {1}.", (sender as Line).Person.FirstName, ProcessedDate.ToString("d", CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Checks if the target folder exists; if it doesn't, we create it.<para />
        /// This method also checks if the user has the right to write in the target folder.
        /// </summary>
        private void CheckChosenFolder()
        {
            var pathTmpFile = Path.Combine(RootPath, "DELETEME.tmp");

            try
            {
                Directory.CreateDirectory(RootPath);

                // The simplest way to determine if the user has the right to create in the folder
                // is to create a dummy file.
                using (FileStream fs = new FileStream(pathTmpFile, FileMode.Create, FileAccess.Write))
                {
                    fs.WriteByte(0xff);
                }
            }
            catch (Exception ex)
            {
                // TODO: find a way to make the border of PathTextBox red if an error occurs.
                // -> in the view!
                ErrorPathInvalid = String.Format("Erreur : le chemin est invalide : {0}.", ex.Message);
            }
            finally
            {
                if (File.Exists(pathTmpFile))
                {
                    File.Delete(pathTmpFile);
                }
            }
        }

        /// <summary>
        /// Chooses a folder to store the files.
        /// </summary>
        private void SelectRootPathFolder()
        {
            // TODO: see if that fits the MVVM pattern.

            var fbd = new FolderBrowserDialog();

            // If the path is relative but not empty, we make it absolute because the FolderBrowserDialog
            // doesn't support empty or relative path.
            if (!String.IsNullOrWhiteSpace(RootPath) && !Path.IsPathRooted(RootPath))
            {
                RootPath = Path.GetFullPath(RootPath);
            }

            fbd.SelectedPath = RootPath;

            var result = fbd.ShowDialog();

            if (result == DialogResult.OK)
            {
                RootPath = fbd.SelectedPath.ToString();
            }
        }

        /// <summary>
        /// Loads the lines from the file, after checking that the file exists.
        /// </summary>
        private void LoadAfterCheck()
        {
            // Reset the error state.
            ErrorPathInvalid = string.Empty;
            var isValidPath = false;

            try
            {
                // Check the path and update it if it's valid.
                isValidPath = File.Exists(Path.Combine(RootPath, PersonsStore.CroissantLinesFileName));

                UpdatePath();
            }
            catch (Exception ex)
            {
                ErrorPathInvalid = String.Format("Erreur : le chemin est invalide : {0}.", ex.Message);
            }

            // Load the file if the path is valid or show an error message to the user.
            if (isValidPath)
            {
                Load();
            }
            else
            {
                ErrorPathInvalid = "Erreur : le chemin spécifié n'existe pas.";
            }
        }

        /// <summary>
        /// Load the lines from the file.
        /// </summary>
        private void Load()
        {
            Lines = new ObservableCollection<Line>(PersonsStore.ReadCroissantLines());
        }

        /// <summary>
        /// Adds a penalty to the specified line.
        /// </summary>
        /// <param name="line">Line to add the croissant.</param>
        private void AddPenalty(Line line)
        {
            // Empty the information message.
            InformationMessage = string.Empty;

            // Add the penalty to the line.
            line.AddPenalty(ProcessedDate);
        }

        /// <summary>
        /// Removes a penalty sets on a croissant.
        /// </summary>
        /// <param name="parameter">The parameter containing information about the line and the croissant to remove the penalty from.</param>
        private void RemovePenalty(RemovePenaltyParameter parameter)
        {
            parameter.Line.RemovePenalty(parameter.Croissant.Date.Value);
            // TODO: RemovePenalty: manage a RemovePenaltyCommand
            // -> instanciation, add to CheckCommands, manage it in the view.


            // Add a "SelectedLine" in the view model and use it? -> remove the line in the method parameter
        }

        /// <summary>
        /// Checks if the RemovePenaltyCommand can be executed.
        /// </summary>
        private bool CanRemovePenalty()
        {
            // TODO: implement CanRemovePenalty
            // We need to pass a RemovePenaltyParameter to access the line. Or, use the selected line. 1st option
            // seems better.
            // The command must be deactivated if a penalty exists on the Friday of the week, because
            // the logic on Friday changes (a person can have 1, 2 or 3 penalties on this day).

            return true;
        }

        /// <summary>
        /// Saves the lines and the configuration to the corresponding files.
        /// </summary>
        private void Save()
        {
            // Reset the error state.
            ErrorPathInvalid = string.Empty;

            // Perform checks.
            CheckChosenFolder();

            // Continue only if there is no error.
            if (String.IsNullOrWhiteSpace(ErrorPathInvalid))
            {
                UpdatePath();

                // Update the config file.
                PersonsStore.WriteConfig();

                // Save the lines.
                PersonsStore.WriteCroissantLines(this.Lines.ToList());
            }
        }

        /// <summary>
        /// The user wants to add a person.
        /// </summary>
        private void AddPerson()
        {
            // TODO: implement AddPerson and add a button to add a person in the XAML
            // -> probably with a new screen that prompt the user firstname and lastname
            // -> this action will always be available, so the command associated must be
            // set to true for methodToDetermineCanExecute
            throw new NotImplementedException();
        }

        /// <summary>
        /// The user wants to remove a person.
        /// </summary>
        /// <param name="person">Person to remove.</param>
        private void RemovePerson(Person person)
        {
            // TODO: implement RemovePerson and add a button to remove a person in the XAML
            // -> this action will always be available, so the command associated must be
            // set to true for methodToDetermineCanExecute
            throw new NotImplementedException();
        }

        /// <summary>
        /// Manages the application files when the root path changed.
        /// </summary>
        private void ManageFilesOnPathChanged()
        {
            // move existing files into the new directory if the directory changed.
            var personsFilePathOriginal = Path.Combine(RootPathOriginal, PersonsStore.DefaultPeopleFileName);
            var personsFilePathTarget = Path.Combine(RootPath, PersonsStore.DefaultPeopleFileName);
            if (File.Exists(personsFilePathOriginal) && personsFilePathOriginal != personsFilePathTarget)
            {
                File.Move(personsFilePathOriginal, personsFilePathTarget);
            }
        }

        /// <summary>
        /// Updates the path and manage the files.
        /// </summary>
        private void UpdatePath()
        {
            // Update the application configuration.
            PersonsStore.RootPath = RootPath;

            // If the path changed, move files.
            ManageFilesOnPathChanged();
        }

        // TODO: create a button, for instance "send report".
        // - generate a mail with Outlook displaying the table as it looks like in the application ;
        // - if someone has to bring the croissants, tell it.

        /// <summary>
        /// Checks if every command is executable.
        /// </summary>
        private void CheckCommands()
        {
            SelectRootPathFolderCommand.RaiseCanExecuteChanged();
            LoadFileCommand.RaiseCanExecuteChanged();
            AddPenaltyCommand.RaiseCanExecuteChanged();
            RemovePenaltyCommand.RaiseCanExecuteChanged();
            SelectLineCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }
    }
}
