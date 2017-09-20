using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PetitsPains.Model
{
    /// <summary>
    /// Represents a person.
    /// </summary>
    [Serializable]
    public class Person : ISerializable, IComparable<Person>
    {
        /// <summary>First name.</summary>
        public string FirstName { get; set; }

        /// <summary>Last name.</summary>
        public string LastName { get; set; }

        /// <summary>PersoID; usually a combination of first name + last name.</summary>
        public string PersoID { get; set; }

        /// <summary>Email.</summary>
        public string Email { get; set; }

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
    }
}
