using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PetitsPains.Model
{
    /// <summary>
    /// Represents a croissant.<para />
    /// A croissant has multiple states to know if it can be use and if it has a penalty
    /// on it.
    /// </summary>
    [Serializable]
    public class Croissant : ISerializable, INotifyPropertyChanged, IComparable<Croissant>
    {
        /// <summary>
        /// Represents the state of a croissant.
        /// </summary>
        public enum CroissantState
        {
            /// <summary>The croissant is available: a date can be set on it.</summary>
            IsAvailable,
            /// <summary>The croissant is used: a date is set on it.</summary>
            IsUsed,
            /// <summary>The croissant is deactivated: it can not be use.</summary>
            IsDeactivated
        }

        private CroissantState _State;
        /// <summary>Croissant state.</summary>
        public CroissantState State
        {
            get { return this._State; }
            set
            {
                SetProperty(ref this._State, value);
            }
        }

        private DateTime? _Date;
        /// <summary>
        /// The date when a person forgot to submit its CRA. If it's null, it means that the croissant can be used
        /// by the person.
        /// </summary>
        public DateTime? Date
        {
            get { return this._Date; }
            set
            {
                SetProperty(ref this._Date, value);
                if (value != null)
                {
                    State = CroissantState.IsUsed;   
                }
                else if(State != CroissantState.IsDeactivated)
                {
                    // If the croissant is marked deactivated, it cannot be
                    // marked as available
                    State = CroissantState.IsAvailable;
                }
            }
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Croissant()
        {
            State = CroissantState.IsAvailable;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="date">Date of the penalty.</param>
        public Croissant(DateTime? date)
        {
            Date = date;
            // The state is set by the date
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Context.</param>
        protected Croissant(SerializationInfo info, StreamingContext context)
        {
            Date = info.GetValue("Date", typeof(DateTime?)) as DateTime?;
            State = (CroissantState)info.GetValue("State", typeof(CroissantState));
        }

        /// <summary>
        /// Serialization method
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Date", Date);
            info.AddValue("State", State);
        }

        /// <summary>
        /// Serialization method
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Context.</param>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            GetObjectData(info, context);
        }

        /// <summary>
        /// Compares a croissant with another one.
        /// </summary>
        /// <param name="other">Other croissant to compare to.</param>
        /// <returns>-1 if the date of the current croissant comes before the other croissant's, 0 if it's equal and 1 if it comes after.</returns>
        public int CompareTo(Croissant other)
        {
            if (Date == null && other.Date != null)
            {
                // A croissant that has no date comes after a croissant that has one.
                return 1;
            }
            else if (Date != null && other.Date == null)
            {
                // A croissant that has a date comes before a croissant that has not.
                return -1;
            }
            else if (Date == null && other.Date == null)
            {
                return 0;
            }
            else
            {
                return DateTime.Compare(Date.Value, other.Date.Value);
            }
        }

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
        /// <param name="storage">Initial value.</param>
        /// <param name="value">Updated value.</param>
        /// <param name="propertyName">Property name.</param>
        /// <returns>True if the field value changed, false otherwise.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                RaisedPropertyChanged(propertyName);
                return true;
            }
        }
        #endregion event handling
    }
}
