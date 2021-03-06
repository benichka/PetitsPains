﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PetitsPains.Enums;
using PetitsPains.Utils;

namespace PetitsPains.Model
{
    /// <summary>
    /// Represents a line that contains information about a person.
    /// </summary>
    [Serializable]
    public class Line : ISerializable, INotifyPropertyChanged
    {
        #region properties
        private Person _Person;
        /// <summary>Person for the line.</summary>
        public Person Person
        {
            get { return this._Person; }
            set
            {
                SetProperty(ref this._Person, value);
                this._Person.PropertyChanged += HandlePersonChanged;
            }
        }

        /// <summary>List of croissants for the person.</summary>
        public ItemsChangeObservableCollection<Croissant> Croissants { get; set; }

        private Croissant _SelectedCroissant;
        /// <summary>Selected croissant in the Croissants collection.</summary>
        public Croissant SelectedCroissant
        {
            get { return this._SelectedCroissant; }
            set
            {
                SetProperty(ref this._SelectedCroissant, value);
            }
        }

        private readonly int _CroissantsSlots;
        /// <summary>Maximum croissants slots.</summary>
        public int CroissantsSlots
        {
            get
            {
                return this._CroissantsSlots;
            }
        }

        private ObservableCollection<DateTime> _PenaltiesAdded;
        /// <summary>List of penalties added for this instance.</summary>
        public ObservableCollection<DateTime> PenaltiesAdded
        {
            get { return this._PenaltiesAdded; }
            set
            {
                SetProperty(ref this._PenaltiesAdded, value);
            }
        }

        private bool _HasToBringCroissants;
        /// <summary>A croissant has been deactivated; the person has to bring the croissants.</summary>
        public bool HasToBringCroissants
        {
            get { return this._HasToBringCroissants; }
            private set
            {
                SetProperty(ref this._HasToBringCroissants, value);
            }
        }

        /// <summary>Default value for the maximum croissant penalties a person can have before bringing the croissants.</summary>
        public static int CroissantsSlotsDefault
        {
            get { return 10; }
        }
        #endregion properties

        #region events and delegates
        /// <summary>Handler for the PenaltyAlreadyExistsAtThisDate event.</summary>
        public delegate void PenaltyAlreadyExistsAtThisDateHandler(object sender, EventArgs e);

        /// <summary>
        /// Event raised when an attempt to set a penalty on a existing date is made
        /// (when a penalty for this date already exists).
        /// </summary>
        public event PenaltyAlreadyExistsAtThisDateHandler PenaltyAlreadyExistsAtThisDate;
        #endregion events and delegates

        #region constructors
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Line() : this(CroissantsSlotsDefault)
        { }

        /// <summary>
        /// Constructs a line and initialises the maximum penalties a person can have.
        /// </summary>
        /// <param name="croissantsSlots">Number of croissant slots to set to the line.</param>
        public Line(int croissantsSlots)
        {
            this._CroissantsSlots = croissantsSlots;

            Croissants = new ItemsChangeObservableCollection<Croissant>();
            for (int i = 0; i < CroissantsSlots; i++)
            {
                Croissants.Add(new Croissant());
            }

            PenaltiesAdded = new ObservableCollection<DateTime>();
        }
        #endregion constructors

        /// <summary>
        /// Returns the number of penalties that the person has.
        /// </summary>
        /// <returns>Number of penalties that the person has.</returns>
        public int GetPenaltiesCount()
        {
            if (Croissants != null)
            {
                var result = (from Croissant c in Croissants
                              where c.State == Croissant.CroissantState.IsUsed
                              select c).Count();
                return result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the number of penalties the person is allowed to have before
        /// bringing the croissants.
        /// </summary>
        /// <returns>The number of penalties the person is allowed to have before bringing the croissants.</returns>
        public int GetPenaltiesLimit()
        {
            if (Croissants != null)
            {
                var result = (from Croissant c in Croissants
                              where c.State != Croissant.CroissantState.IsDeactivated
                              select c).Count();

                return result;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the index of the next croissant to deactivate.
        /// </summary>
        /// <returns>The index of the next croissant to deactivate.</returns>
        private int GetNextIndexToDeactivate()
        {
            // We start backward because it is more likely that the person doesn't have a lot of penalties.
            var penaltyIndex = Croissants.Count - 1;
            while (penaltyIndex > 0 && Croissants[penaltyIndex].State == Croissant.CroissantState.IsDeactivated)
            {
                penaltyIndex--;
            }

            return penaltyIndex;
        }

        /// <summary>
        /// Returns the index of the last croissant to be deactivated.
        /// </summary>
        /// <returns>The index of the last croissant to be deactivated.</returns>
        private int GetLastDeactivatedIndex()
        {
            return GetNextIndexToDeactivate() + 1;
        }

        /// <summary>
        /// Adds a penalty to the line for the given date.
        /// </summary>
        /// <param name="date">Date to set the penalty on.</param>
        public void AddPenalty(DateTime date)
        {
            // Check that there is no existing penalty for the given date.
            // If that's the case, a penalty is added. Otherwise, the PenaltyAlreadyExistsAtThisDate event is raised.
            if (!Croissants.Any(c => c.Date.HasValue && c.Date.Value == date))
            {
                // Reordering the list of penalties in an ObservableCollection means new instanciation => add the new date at the correct index instead.

                int penaltiesToAdd = PenaltiesToAdd(date);

                for (int i = 0; i < penaltiesToAdd; i++)
                {
                    ProcessPenaltyAdding(date);
                }

                // A penalty was added for this instance
                // -> very simple processing...
                PenaltiesAdded.Add(date);
            }
            else
            {
                if (PenaltyAlreadyExistsAtThisDate != null)
                {
                    PenaltyAlreadyExistsAtThisDate(this, EventArgs.Empty); 
                }
            }
        }

        /// <summary>
        /// Removes a penalty sets on a croissant.
        /// </summary>
        /// <param name="date">Date to remove the penalty from.</param>
        public void RemovePenalty(DateTime date)
        {
            // Remove the penalties for this date
            var croissantsToUpdate = (from Croissant c in Croissants
                                      where c.Date.HasValue && c.Date.Value == date
                                      select c).ToList();

            foreach (var croissant in croissantsToUpdate)
            {
                croissant.Date = null;
            }

            // Sort the list of croissant to put the gaps to the right of the collection
            Croissants.BubbleSort(SortDirection.Ascending);

            // Update the list of penalties for this instance: simply remove the penalties at this date
            var penaltiesToRemove = (from p in PenaltiesAdded
                                     where p == date
                                     select p).ToList();

            foreach (var penalty in penaltiesToRemove)
            {
                PenaltiesAdded.Remove(penalty);
            }

            // TODO: Eventually, improve the removal of a penalty.
            // Right now, it is very simple: delete all penalties at this date. But it can have
            // consequences, especially it we remove a penalty on friday: 2 croissants are removed,
            // or 3 if the person didn't submit their CRA the whole week.
            // Plus, if we remove a penalty in the week and the person already has a penalty on friday
            // and didn't submit their CRA the rest of the week, the added "foutage de gueule" croissant
            // needs to be remove.
        }

        /// <summary>
        /// Manages the adding of a penalty.
        /// </summary>
        /// <param name="date">Date to set the penalty on.</param>
        private void ProcessPenaltyAdding(DateTime date)
        {
            // If the person didn't reach their penalties limit, just add one.
            if (GetPenaltiesCount() < GetPenaltiesLimit())
            {
                // Search the index to place the date.
                var newDateIndex = 0;
                while (newDateIndex < GetLastDeactivatedIndex() && Croissants[newDateIndex].Date.HasValue && Croissants[newDateIndex].Date.Value <= date)
                    newDateIndex++;

                // Move the existing dates accordingly.
                for (int i = GetNextIndexToDeactivate(); i > newDateIndex; i--)
                {
                    Croissants[i].Date = Croissants[i - 1].Date;
                }

                Croissants[newDateIndex].Date = date;
            }
            // Otherwise, they have to bring the croissants!
            // Their penalties limit is consequently lowered by one.
            else
            {
                // Reset the date for each croissant.
                Croissants.Select(c => { c.Date = null; return c; }).ToList();

                // Deactivate croissant.
                Croissants[GetNextIndexToDeactivate()].State = Croissant.CroissantState.IsDeactivated;

                HasToBringCroissants = true;
            }
        }

        /// <summary>
        /// Calculates the number of penalties to add to the person.
        /// </summary>
        /// <param name="date">Date to set the penalty on.</param>
        /// <returns>The number of penalties to add to the person.</returns>
        private int PenaltiesToAdd(DateTime date)
        {
            // The default number is 1
            var penaltiesToAdd = 1;

            // If the day is Friday or Monday, 2 penalties are added.
            // -> it means that the person forgot to submit their CRA
            // on thursday or friday.
            if ((date.DayOfWeek == DayOfWeek.Friday) ||
                (date.DayOfWeek == DayOfWeek.Monday))
            {
                penaltiesToAdd = 2;
            }

            // If the person forgot to submit their CRA the whole week, a penalty is added.
            if (CheckFoutageDeGueule(date))
            {
                penaltiesToAdd++;
            }

            return penaltiesToAdd;
        }

        /// <summary>
        /// Checks if the person forgot to submit their CRA at least one time in the week.
        /// </summary>
        /// <param name="date">Date to set the penalty on.</param>
        /// <returns>True if that's the case, false otherwise.</returns>
        private bool CheckAtLeastOnePenaltyThisWeek(DateTime date)
        {
            // The person is considered innocent.
            var result = false;

            DateTime firstDayOfProcessedWeek = FirstDayOfProcessedWeek(date);

            // We only check the "standards" working days => from Monday to Friday.
            DateTime dayToCheck = firstDayOfProcessedWeek;

            for (int i = 0; i < 5; i++)
            {
                // Check if this day has at least one penalty.
                var penaltiesForThisDay = (from Croissant c in Croissants
                                           where c.Date.HasValue && c.Date.Value == dayToCheck && c.State != Croissant.CroissantState.IsDeactivated
                                           select c).Count();

                if (penaltiesForThisDay > 0)
                {
                    result = true;

                    // No need to check further as long as the person omitted to submit their CRA at least one time in the week.
                    break;
                }

                dayToCheck = dayToCheck.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Checks if the person forgot to submit their CRA at least one time in the last week.
        /// </summary>
        /// <param name="date">Date to set the penalty on.</param>
        /// <returns>True if that's the case, false otherwise.</returns>
        private bool CheckAtLeastOnePenaltyLastWeek(DateTime date)
        {
            // We simply call the CheckAtLeastOnePenaltyThisWeek method with the first
            // day of the last week as parameter.
            DateTime firstDayOfLastWeek = FirstDayOfProcessedWeek(date).AddDays(-7);

            return CheckAtLeastOnePenaltyThisWeek(firstDayOfLastWeek);
        }

        /// <summary>
        /// Checks if a person forgot to submit their CRA the whole week.
        /// </summary>
        /// <param name="date">Date to set the penalty on.</param>
        /// <returns>True if that's the case, false otherwise.</returns>
        private bool CheckFoutageDeGueule(DateTime date)
        {
            // The person is considered guilty!
            var result = true;

            DateTime firstDayOfProcessedWeek = FirstDayOfProcessedWeek(date);

            // We only check the "standards" working days => from Monday to Friday.
            // Therefore, if the person already has penalties from Monday to Thursday,
            // it means that we are currently adding a penalty for Friday. -> we only
            // need to check from Monday (0) to Thursday (3).
            DateTime dayToCheck = firstDayOfProcessedWeek;
            for (int i = 0; i < 4; i++)
            {
                // If at least one day of the week is not in the dates, the person doesn't get the additional penalty
                var penaltiesForThisDay = (from Croissant c in Croissants
                             where c.Date.HasValue && c.Date.Value == dayToCheck && c.State != Croissant.CroissantState.IsDeactivated
                             select c).Count();

                if (penaltiesForThisDay == 0)
                {
                    result = false;

                    // No need to check further as long as the person submitted his CRA at least one day in this week
                    break;
                }

                dayToCheck = dayToCheck.AddDays(1);
            }

            return result;
        }

        /// <summary>
        /// Returns the first day (Monday) of the week of the processed date.
        /// </summary>
        /// <param name="date">Processed date.</param>
        /// <returns>The first day (Monday) of the week of the processed date.</returns>
        private DateTime FirstDayOfProcessedWeek(DateTime date)
        {
            var offset = 0;

            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    offset = -6;
                    break;
                case DayOfWeek.Monday:
                    offset = 0;
                    break;
                case DayOfWeek.Tuesday:
                    offset = -1;
                    break;
                case DayOfWeek.Wednesday:
                    offset = -2;
                    break;
                case DayOfWeek.Thursday:
                    offset = -3;
                    break;
                case DayOfWeek.Friday:
                    offset = -4;
                    break;
                case DayOfWeek.Saturday:
                    offset = -5;
                    break;
                default:
                    break;
            }

            return date.AddDays(offset).Date;
        }

        /// <summary>
        /// Reactivate a croissant that has been deactivated.
        /// </summary>
        public void ReactivatedCroissant()
        {
            // A deactivated croissant doesn't have a date of deactivation,
            // so we simply reactivate the last deactivated croissant in the slots.

            int? indexOfCroissantToReactivate = GetIndexOfCroissantToReactivate();
            if (indexOfCroissantToReactivate != null)
            {
                Croissants[indexOfCroissantToReactivate.Value].State = Croissant.CroissantState.IsAvailable;

                // We consider that if a croissant is reactivated, the person doesn't have
                // to bring the croissant anymore: the "admin" probably set a penalty by
                // mistake.
                // Very simple processing though.
                HasToBringCroissants = false;
            }
        }

        /// <summary>
        /// Get the index of the croissant to reactivate. This is the index of the
        /// last croissant that was deactivated.
        /// </summary>
        /// <returns>The index of the croissant to reactivate.</returns>
        private int? GetIndexOfCroissantToReactivate()
        {
            int? index = null;
            for (int i = 0; i < Croissants.Count; i++)
            {
                if (Croissants[i].State == Croissant.CroissantState.IsDeactivated)
                {
                    // The last croissant to be deactivated is the first croissant in
                    // the list of croissants that is... deactivated!
                    index = i;
                    break;
                }
            }

            return index;
        }

        #region serialisation
        /// <summary>
        /// Deserialization constructor.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Context.</param>
        protected Line(SerializationInfo info, StreamingContext context)
        {
            Person = info.GetValue("Person", typeof(Person)) as Person;
            Croissants = info.GetValue("Croissants", typeof(ItemsChangeObservableCollection<Croissant>)) as ItemsChangeObservableCollection<Croissant>;
            PenaltiesAdded = new ObservableCollection<DateTime>();
        }

        /// <summary>
        /// Serialization method.
        /// </summary>
        /// <param name="info">Serialization info.</param>
        /// <param name="context">Context.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Person", Person);
            info.AddValue("Croissants", Croissants);
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
        #endregion serialisation

        /// <summary>
        /// Event handling for the event PropertyChanged of the person.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event arguments.</param>
        private void HandlePersonChanged(object sender, EventArgs e)
        {
            // We simply raise a property changed event for the person.
            if (this.PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Person"));
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
