using System;
using PetitsPains.Command;
using PetitsPains.Model;

namespace PetitsPains.ViewModel
{
    public class UpdatePersonViewModel : ViewModelBase
    {
        #region Properties
        private Person _PersonToUpdate;
        /// <summary>First name for the person to add.</summary>
        public Person PersonToUpdate
        {
            get { return this._PersonToUpdate; }
            set
            {
                SetProperty(CheckCommands, ref this._PersonToUpdate, value);
                this._PersonToUpdate.PropertyChanged += HandlePersonPropertyChanged;
            }
        }

        /// <summary>Original first name for the person.</summary>
        private readonly string oldFirstName;

        /// <summary>Original last name for the person.</summary>
        private readonly string oldLastName;

        /// <summary>Original persoId for the person.</summary>
        private readonly string oldPersoId;

        /// <summary>Original email for the person.</summary>
        private readonly string oldEMail;
        #endregion Properties

        #region Commands
        /// <summary>Command associated with the button to validate the adding.</summary>
        public CommandHandler<IDialog> ValidateUpdatePersonCommand { get; private set; }

        /// <summary>Command associated with the button to cancel the action.</summary>
        public CommandHandler<IDialog> CancelCommand { get; private set; }
        #endregion Commands

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Line">Line to update.</param>
        public UpdatePersonViewModel(Line line)
        {
            ValidateUpdatePersonCommand = new CommandHandler<IDialog>(ValidateUpdatePerson, CanValidateUpdatePerson);

            CancelCommand = new CommandHandler<IDialog>(Cancel, () => true);

            this.oldFirstName = line.Person.FirstName;
            this.oldLastName = line.Person.LastName;
            this.oldPersoId = line.Person.PersoId;
            this.oldEMail = line.Person.Email;

            PersonToUpdate = line.Person;
        }

        /// <summary>
        /// Event handling for the event PropertyChanged of the Person.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandlePersonPropertyChanged(object sender, EventArgs e)
        {
            // Simply refresh the command.
            CheckCommands();
        }

        /// <summary>
        /// Checks if the person has changed: at least one of their property changed.
        /// </summary>
        /// <returns>True if that's the case, false otherwise.</returns>
        private bool PersonChanged()
        {
            return !(String.Equals(PersonToUpdate.FirstName, this.oldFirstName) &&
                    String.Equals(PersonToUpdate.LastName, this.oldLastName) &&
                    String.Equals(PersonToUpdate.PersoId, this.oldPersoId) &&
                    String.Equals(PersonToUpdate.Email, this.oldEMail));
        }

        /// <summary>
        /// Validate the update of the person and close the view.
        /// </summary>
        /// <param name="window">View where the update is done.</param>
        private void ValidateUpdatePerson(IDialog window)
        {
            // The person is automatically updated because it is passed as a parameter.
            // We just have to close the view!
            if (window != null)
            {
                window.DialogResult = true;
                window.Close();
            }
        }

        /// <summary>
        /// Checks if the ValidateUpdatePersonCommand can be executed. 
        /// </summary>
        /// <returns>True if that's the case, false otherwise.</returns>
        private bool CanValidateUpdatePerson()
        {
            return PersonChanged();
        }

        /// <summary>
        /// Cancel the action and close the window.
        /// </summary>
        /// <param name="window">Window where the action is performed.</param>
        private void Cancel(IDialog window)
        {
            // First, a rollback on all fields is done.
            if (PersonToUpdate != null && PersonChanged())
            {
                PersonToUpdate.FirstName = oldFirstName;
                PersonToUpdate.LastName = oldLastName;
                PersonToUpdate.PersoId = oldPersoId;
                PersonToUpdate.Email = oldEMail;
            }

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
            ValidateUpdatePersonCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
        }
    }
}
