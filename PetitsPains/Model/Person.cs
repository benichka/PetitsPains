using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PetitsPains.Model
{
    /// <summary>
    /// Represents a person.
    /// </summary>
    [Serializable]
    public class Person : ISerializable, IComparable<Person>, INotifyPropertyChanged
    {
        private string _FirstName;
        /// <summary>First name.</summary>
        public string FirstName
        {
            get { return this._FirstName; }
            set
            {
                SetProperty(ref this._FirstName, value);
            }
        }

        private string _LastName;
        /// <summary>Last name.</summary>
        public string LastName
        {
            get { return this._LastName; }
            set
            {
                SetProperty(ref this._LastName, value);
            }
        }

        private string _PersoId;
        /// <summary>PersoId; usually a combination of first name + last name.</summary>
        public string PersoId
        {
            get { return this._PersoId; }
            set
            {
                SetProperty(ref this._PersoId, value);
            }
        }

        private string _Email;
        /// <summary>Email.</summary>
        public string Email
        {
            get { return this._Email; }
            set
            {
                SetProperty(ref this._Email, value);
            }
        }

        /// <summary>
        /// Default constructor, needed for a JavascriptSerializer.
        /// </summary>
        public Person()
        {

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstName">Person's first name.</param>
        /// <param name="lastName">Person's last name.</param>
        public Person(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstName">Person's first name.</param>
        /// <param name="lastName">Person's last name.</param>
        /// <param name="email">Person's email.</param>
        public Person(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="firstName">Person's first name.</param>
        /// <param name="lastName">Person's last name.</param>
        /// <param name="persoId">Person's persoId.</param>
        /// <param name="email">Person's email.</param>
        public Person(string firstName, string lastName, string persoId, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            PersoId = persoId;
            Email = email;
        }

        /// <summary>
        /// Represents a person as a string.
        /// </summary>
        /// <returns>The person as a string.</returns>
        public override string ToString()
        {
            return FirstName + " " + LastName;
        }

        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Context.</param>
        protected Person(SerializationInfo info, StreamingContext context)
        {
            FirstName = info.GetValue("FirstName", typeof(string)) as string;
            LastName = info.GetValue("LastName", typeof(string)) as string;
            PersoId = info.GetValue("PersoId", typeof(string)) as string;
            Email = info.GetValue("Email", typeof(string)) as string;
        }

        /// <summary>
        /// Serialization method.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("FirstName", FirstName);
            info.AddValue("LastName", LastName);
            info.AddValue("PersoId", PersoId);
            info.AddValue("Email", Email);
        }

        /// <summary>
        /// Serialization method.
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
        /// Compares a person with another one.
        /// </summary>
        /// <param name="other">Other person to compare to.</param>
        /// <returns>-1 if the first name of the current person comes before the other person's, 0 if it's equal and 1 if it comes after</returns>
        public int CompareTo(Person other)
        {
            return FirstName.CompareTo(other.FirstName);
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
