using System;
using System.Collections.ObjectModel;
using System.Linq;
using PetitsPains.Command;
using PetitsPains.Model;

namespace PetitsPains.ViewModel
{
    /// <summary>
    /// ViewModel for the AddPersonView.
    /// </summary>
    public class AddPersonViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<Line> _Lines;
        /// <summary>Existing lines. The new person will be added to a new line in this list.</summary>
        public ObservableCollection<Line> Lines
        {
            get { return this._Lines; }
            set
            {
                SetProperty(CheckCommands, ref this._Lines, value);
            }
        }

        private string _FirstName;
        /// <summary>First name for the person to add.</summary>
        public string FirstName
        {
            get { return this._FirstName; }
            set
            {
                SetProperty(CheckCommands, ref this._FirstName, value);
            }
        }

        private string _LastName;
        /// <summary>Last name for the person to add.</summary>
        public string LastName
        {
            get { return this._LastName; }
            set
            {
                SetProperty(CheckCommands, ref this._LastName, value);
            }
        }

        private string _EMail;
        /// <summary>Email for the person to add.</summary>
        public string EMail
        {
            get { return this._EMail; }
            set
            {
                SetProperty(CheckCommands, ref this._EMail, value);
            }
        }
        #endregion Properties

        #region Commands
        /// <summary>Command associated with the button to validate the adding.</summary>
        public CommandHandler<IDialog> AddPersonCommand { get; private set; }

        /// <summary>Command associated with the button to cancel the action.</summary>
        public CommandHandler<IDialog> CancelCommand { get; private set; }
        #endregion Commands

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="Lines">Lines collection to whom to add the new person.</param>
        public AddPersonViewModel(ObservableCollection<Line> lines)
        {
            AddPersonCommand = new CommandHandler<IDialog>(AddPerson, AddPersonCanExecute);

            CancelCommand = new CommandHandler<IDialog>(Cancel, () => true);

            Lines = lines;
        }

        /// <summary>
        /// Add the person to the list and close the window.
        /// </summary>
        /// <param name="window">Window where the action is performed.</param>
        private void AddPerson(IDialog window)
        {
            // Add the person only if they aren't already in the list
            var personInList = Lines.FirstOrDefault(l => l.Person.FirstName.Equals(FirstName) && l.Person.LastName.Equals(LastName));
            if (personInList == null)
            {
                Lines.Add(new Line() { Person = new Person(FirstName, LastName, EMail) });
            }

            // Close the window
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        /// <summary>
        /// Checks that the person to add is a valid one.
        /// </summary>
        /// <returns>True if that's the case, false otherwise.</returns>
        private bool AddPersonCanExecute()
        {
            // TODO: check the email format.
            return !String.IsNullOrWhiteSpace(FirstName) && !String.IsNullOrWhiteSpace(LastName);
        }

        /// <summary>
        /// Cancel the action and close the window.
        /// </summary>
        /// <param name="window">Window where the action is performed.</param>
        private void Cancel(IDialog window)
        {
            if (window != null)
            {
                window.DialogResult = false;
                window.Close();
            }
        }

        /// <summary>
        /// Checks if every command is executable.
        /// </summary>
        private void CheckCommands()
        {
            AddPersonCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
