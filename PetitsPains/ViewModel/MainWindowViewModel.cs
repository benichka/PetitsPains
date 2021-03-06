﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Microsoft.Office.Interop.Outlook;
using PetitsPains.Command;
using PetitsPains.Data;
using PetitsPains.Model;
using PetitsPains.Resources;
using PetitsPains.Utils;
using PetitsPains.View;
using OutlookApp = Microsoft.Office.Interop.Outlook.Application;

namespace PetitsPains.ViewModel
{
    /// <summary>
    /// View model for the MainWindow view
    /// </summary>
    public class MainWindowViewModel : ViewModelBase
    {
        #region properties
        private ItemsChangeObservableCollection<Line> _Lines;
        /// <summary>List of lines.</summary>
        public ItemsChangeObservableCollection<Line> Lines
        {
            get { return this._Lines; }
            private set
            {
                SetProperty(CheckCommands, ref this._Lines, value);
                this._Lines.CollectionChanged += HandleLinesCollectionChanged;

                // Subscribe to each event that the line can raise.
                foreach (var line in this._Lines)
                {
                    line.PenaltyAlreadyExistsAtThisDate += HandlePenaltyAlreadyExistsAtThisDate;
                    line.PropertyChanged += HandleLinePropertyChanded;
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

        private bool _IsSendingEmail;
        /// <summary>Boolean indicating is the report email is being sent.</summary>
        public bool IsSendingEmail
        {
            get { return this._IsSendingEmail; }
            set
            {
                this._IsSendingEmail = value;
                CheckCommands();
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

        /// <summary>Command associated with the button to remove a penalty from a line.</summary>
        public CommandHandler RemovePenaltyCommand { get; private set; }

        /// <summary>Command associated with the button to activate a croissant.</summary>
        public CommandHandler ActivateCroissantCommand { get; set; }

        /// <summary>Command to select a line.</summary>
        public CommandHandler<Line> SelectLineCommand { get; private set; }

        /// <summary>Command to remove a line.</summary>
        public CommandHandler<Line> RemoveLineCommand { get; private set; }

        /// <summary>Command to update a person in a line.</summary>
        public CommandHandler<Line> UpdateLineCommand { get; set; }

        /// <summary>Command to add a line.</summary>
        public CommandHandler AddLineCommand { get; set; }

        /// <summary>Command to send the situation as an email.</summary>
        public CommandHandler EmailCommand { get; set; }

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

            RemovePenaltyCommand = new CommandHandler(RemovePenalty, CanRemovePenalty);

            ActivateCroissantCommand = new CommandHandler(ActivateCroissant, CanActivateCroissant);

            SelectLineCommand = new CommandHandler<Line>((line) => SelectedLine = line, () => true);

            RemoveLineCommand = new CommandHandler<Line>(RemoveLine, () => true);

            UpdateLineCommand = new CommandHandler<Line>(UpdateLine, () => true);

            AddLineCommand = new CommandHandler(AddLine, () => true);

            EmailCommand = new CommandHandler(EmailSituation, () => (Lines.Count > 0 && !IsSendingEmail));

            // The Save button can not be clicked if the path is empty.
            SaveCommand = new CommandHandler(Save, () => !String.IsNullOrWhiteSpace(RootPath));

            IsSendingEmail = false;

            // By default, the proccesed date is the current day.
            ProcessedDate = DateTime.Now;

            // Load the configuration.
            PetitsPainsStore.ReadConfig();

            // Set the original root path.
            this._RootPathOriginal = PetitsPainsStore.RootPath;

            // Retrieve the root path for the files of the application.
            RootPath = PetitsPainsStore.RootPath;

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
        /// Event handling for the event PropertyChanged of a line.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleLinePropertyChanded(object sender, EventArgs e)
        {
            // When the line changes, we need to re-check our commands
            CheckCommands();
        }

        /// <summary>
        /// Event handling for the event CollectionChanged of the list of lines.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandleLinesCollectionChanged(object sender, EventArgs e)
        {
            // When the collection change, we need to re-check our commands
            CheckCommands();
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
            catch (System.Exception ex)
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
            // TODO: call the folder browser dialog using the MVVM pattern.

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
                isValidPath = File.Exists(Path.Combine(RootPath, PetitsPainsStore.CroissantLinesFileName));

                UpdatePath();
            }
            catch (System.Exception ex)
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
            Lines = new ItemsChangeObservableCollection<Line>(PetitsPainsStore.ReadCroissantsLines());
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

            // Check the commands: they state can change due to the added penalty.
            CheckCommands();
        }

        /// <summary>
        /// Removes a penalty sets on a croissant.
        /// </summary>
        private void RemovePenalty()
        {
            if (SelectedLine != null && SelectedLine.SelectedCroissant != null)
            {
                SelectedLine.RemovePenalty(SelectedLine.SelectedCroissant.Date.Value);
            }
            else
            {
                InformationMessage = "Impossible de supprimer une pénalité inexistante.";
            }

            // Check the commands: they state can change due to the added penalty.
            CheckCommands();
        }

        /// <summary>
        /// Checks if the RemovePenaltyCommand can be executed.
        /// </summary>
        private bool CanRemovePenalty()
        {
            if (SelectedLine != null && SelectedLine.SelectedCroissant != null)
            {
                return SelectedLine.SelectedCroissant.State == Croissant.CroissantState.IsUsed;
            }
            else
            {
                return false;
            }
            // TODO: Eventually, better implementation of CanRemovePenalty.
            // Why? If a penalty exists on the Friday of the week, a person can have 1, 2 or 3 penalties on this day.
            // Removal of penalties can have unsuspected consequences.
            // See with the implementation of Line.RemovePenalty to make this coherent.
        }

        /// <summary>
        /// Activates a croissant that is deactivated.
        /// </summary>
        private void ActivateCroissant()
        {
            if (SelectedLine != null && SelectedLine.SelectedCroissant != null)
            {
                // A deactivated croissant doesn't have a date of deactivation,
                // so we simply ask the line to reactivate one of its croissant.
                SelectedLine.ReactivatedCroissant();
            }
            else
            {
                InformationMessage = "Impossible de gérer une pénalité inexistante.";
            }

            // Check the commands: they state can change due to the activated croissant.
            CheckCommands();
        }

        /// <summary>
        /// Checks if the ActivateCroissantCommand can be executed.
        /// </summary>
        /// <returns>True if that's the case, false otherwise.</returns>
        private bool CanActivateCroissant()
        {
            if (SelectedLine != null && SelectedLine.SelectedCroissant != null)
            {
                return SelectedLine.SelectedCroissant.State == Croissant.CroissantState.IsDeactivated;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Emails the situation to everyone in the list.
        /// </summary>
        private async void EmailSituation()
        {
            // Clean the messages.
            InformationMessage = string.Empty;

            // Generate the email content based on the template
            var emailTemplate = new EmailTemplate(ProcessedDate, Lines);
            var emailTemplateContent = emailTemplate.TransformText();

            // Send the mail via Outlook
            // Check that the email button is disable just after clicking it.
            IsSendingEmail = true;
            try
            {
                await Task.Run(() =>
                {
                    OutlookApp outlookApp = new OutlookApp();
                    MailItem mailItem = outlookApp.CreateItem(OlItemType.olMailItem);

                    foreach (var line in Lines)
                    {
                        mailItem.Recipients.Add(line.Person.Email);
                    }
                    mailItem.Recipients.ResolveAll();

                    mailItem.Subject = String.Format("Soumission des CRA : rapport du {0}", ProcessedDate.ToString("d"));

                    // Images are attached to the mail with a contentId; that way, they will be accessible in the mail
                    // via the attribute "cid".
                    // For instance: <img src="cid:croissantEmpty" />
                    // In Outlook, this attribute is the property that has the schema name
                    // http://schemas.microsoft.com/mapi/proptag/0x3712001E.
                    var appPath = AppDomain.CurrentDomain.BaseDirectory;

                    Attachment croissantEmptyAttachment = mailItem.Attachments.Add(Path.Combine(appPath, "Assets/croissant_empty.png"));
                    croissantEmptyAttachment.PropertyAccessor.SetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001E", "croissantEmpty");

                    Attachment croissantFilledAttachment = mailItem.Attachments.Add(Path.Combine(appPath, "Assets/croissant_filled.png"));
                    croissantFilledAttachment.PropertyAccessor.SetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001E", "croissantFilled");

                    Attachment croissantGreyedAttachment = mailItem.Attachments.Add(Path.Combine(appPath, "Assets/croissant_greyed.png"));
                    croissantGreyedAttachment.PropertyAccessor.SetProperty("http://schemas.microsoft.com/mapi/proptag/0x3712001E", "croissantGreyed");

                    mailItem.HTMLBody = emailTemplateContent;

                    mailItem.Display(false);
                });
            }
            catch (System.Exception)
            {
                InformationMessage = "Erreur lors de la préparation du message";
            }

            IsSendingEmail = false;

            Save();
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
                PetitsPainsStore.WriteConfig();

                // Save the lines.
                PetitsPainsStore.WriteCroissantsLines(this.Lines.ToList());
            }
        }

        /// <summary>
        /// The user wants to add a line.
        /// </summary>
        private void AddLine()
        {
            // TODO: call a dialog using the MVVM pattern to add a line.

            AddPersonWindow addPersonView = new AddPersonWindow(Lines);
            var result = addPersonView.ShowDialog();

            // Do not forget to subscribe to line.PenaltyAlreadyExistsAtThisDate and
            // line.PropertyChanged.
            if (result.HasValue && result.Value)
            {
                // If a person has been added, their are located at the end of the list
                Lines.Last().PenaltyAlreadyExistsAtThisDate += HandlePenaltyAlreadyExistsAtThisDate;
                Lines.Last().PropertyChanged += HandleLinePropertyChanded;
            }
        }

        /// <summary>
        /// The user wants to remove a line.
        /// </summary>
        /// <param name="line">Line to remove.</param>
        private void RemoveLine(Line line)
        {
            // TODO: call a dialog using the MVVM pattern to confirm the line removal.
            string msgtext = String.Format("Voulez vous supprimer la ligne pour {0} ?", line.Person.ToString());
            string txt = "Confirmer la suppression";
            MessageBoxButton button = MessageBoxButton.YesNo;

            MessageBoxResult result = System.Windows.MessageBox.Show(msgtext, txt, button, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Lines.Remove(line);
            }
        }

        /// <summary>
        /// The user wants to update a line.
        /// </summary>
        /// <param name="line">Line to update.</param>
        private void UpdateLine(Line line)
        {
            // TODO: call a dialog using the MVVM pattern to update the line.
            UpdatePersonWindow updatePersonWindow = new UpdatePersonWindow(SelectedLine);
            var result = updatePersonWindow.ShowDialog();
        }

        /// <summary>
        /// Manages the application files when the root path changed.
        /// </summary>
        private void ManageFilesOnPathChanged()
        {
            // move existing files into the new directory if the directory changed.
            var personsFilePathOriginal = Path.Combine(RootPathOriginal, PetitsPainsStore.DefaultPeopleFileName);
            var personsFilePathTarget = Path.Combine(RootPath, PetitsPainsStore.DefaultPeopleFileName);
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
            PetitsPainsStore.RootPath = RootPath;

            // If the path changed, move files.
            ManageFilesOnPathChanged();
        }

        /// <summary>
        /// Checks if every command is executable.
        /// </summary>
        private void CheckCommands()
        {
            SelectRootPathFolderCommand.RaiseCanExecuteChanged();
            LoadFileCommand.RaiseCanExecuteChanged();
            AddPenaltyCommand.RaiseCanExecuteChanged();
            RemovePenaltyCommand.RaiseCanExecuteChanged();
            ActivateCroissantCommand.RaiseCanExecuteChanged();
            SelectLineCommand.RaiseCanExecuteChanged();
            EmailCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
        }
    }
}
