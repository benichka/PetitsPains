using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PetitsPains.ViewModel
{
    /// <summary>
    /// Base class for the view models.<para />
    /// This class mainly handles the property changed interface.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region event handling
        /// <summary>Event handler.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the changed property event.
        /// </summary>
        /// <param name="propertyName">Changed property.</param>
        protected void RaisedPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (this.PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Field change notification only if the field has really changed.
        /// </summary>
        /// <typeparam name="T">Field type.</typeparam>
        /// <param name="checkCommands">Action that checks the commands everytime a property changes.</param>
        /// <param name="storage">Initial value.</param>
        /// <param name="value">Updated value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>True if the field value changed, false otherwise.</returns>
        protected bool SetProperty<T>(Action checkCommands, ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                RaisedPropertyChanged(propertyName);
                if (checkCommands != null)
                {
                    checkCommands(); 
                }
                return true;
            }
        }
        #endregion event handling
    }
}
