// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table UserAttendance
  /// 
  /// This new modification table records
  /// when a user checks in or checks out in the site.
  /// It can be associated to a time clock system.
  /// </summary>
  [Serializable]
  public class UserAttendance: GlobalModification, IUserAttendance
  {
    #region Members
    IUser m_user;
    IShift m_shift;
    DateTime? m_begin;
    DateTime? m_end;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UserAttendance).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected UserAttendance ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="user"></param>
    internal protected UserAttendance (IUser user)
    {
      this.User = user;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "UserAttendance"; }
    }

    /// <summary>
    /// Reference to a User
    /// 
    /// Not null
    /// </summary>
    [XmlIgnore]
    public virtual IUser User {
      get { return m_user; }
      protected set
      {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("User.set: " +
                           "null value");
          throw new ArgumentNullException ("UserAttendance.User");
        }
        m_user = value;
      }
    }
    
    /// <summary>
    /// Reference to the User
    /// for Xml Serialization
    /// </summary>
    [XmlElement("User")]
    public virtual User XmlSerializationUser {
      get { return this.User as User; }
      set { this.User = value; }
    }
    
    /// <summary>
    /// (optional) Reference to a Shift
    /// 
    /// Nullable
    /// </summary>
    [XmlIgnore]
    public virtual IShift Shift {
      get { return m_shift; }
      set { m_shift = value; }
    }
    
    /// <summary>
    /// Reference to the Shift
    /// for Xml Serialization
    /// </summary>
    [XmlElement("Shift")]
    public virtual Shift XmlSerializationShift {
      get { return this.Shift as Shift; }
      set { this.Shift = value; }
    }
    
    /// <summary>
    /// Local begin date/time for XML serialization
    /// </summary>
    [XmlAttribute("LocalBegin")]
    public virtual string LocalBegin {
      get {
        if (Begin.HasValue) {
          return Begin.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
        else {
          return null;
        }
      }
      
      set {
        if (value != null) {
          Begin = System.DateTime.Parse(value).ToUniversalTime();
        }
        else {
          Begin = null;
        }
      }
    }

    /// <summary>
    /// LocalBegin is never serialized
    /// </summary>
    public virtual bool LocalBeginSpecified{ get { return false; } }

    /// <summary>
    /// (optional) UTC date/time of a user check-in
    /// </summary>
    [XmlIgnore]
    public virtual Nullable<DateTime> Begin {
      get { return m_begin; }
      set { m_begin = value; }
    }
    
    /// <summary>
    /// UTC begin date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("Begin")]
    public virtual string SqlBegin {
      get
      {
        if (null == this.Begin) {
          return null;
        }
        else {
          return ((DateTime)this.Begin).ToString ("yyyy-MM-dd HH:mm:ss");
        }
      }
      set
      {
        if (null == value) {
          this.Begin = null;
        }
        else {
          this.Begin = System.DateTime.Parse (value);
        }
      }
    }

    
    /// <summary>
    /// Local end date/time for XML deserialization
    /// </summary>
    [XmlAttribute("LocalEnd")]
    public virtual string LocalEnd {
      get {
        if (End.HasValue) {
          return End.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
        else {
          return null;
        }
      }
      
      set {
        if (value != null) {
          End = System.DateTime.Parse(value).ToUniversalTime();
        }
        else {
          End = null;
        }
      }

    }

    /// <summary>
    /// LocalEnd is never serialized
    /// </summary>
    public virtual bool LocalEndSpecified{ get { return false; } }

    /// <summary>
    /// (optional) UTC date/time of a user check-out
    /// </summary>
    [XmlIgnore]
    public virtual Nullable<DateTime> End {
      get { return m_end; }
      set { m_end = value; }
    }

    /// <summary>
    /// UTC end date/time in SQL string for XML serialization
    /// </summary>
    [XmlAttribute("End")]
    public virtual string SqlEnd {
      get
      {
        if (null == this.End) {
          return null;
        }
        else {
          return ((DateTime)this.End).ToString ("yyyy-MM-dd HH:mm:ss");
        }
      }
      set
      {
        if (null == value) {
          this.End = null;
        }
        else {
          this.End = System.DateTime.Parse (value);
        }
      }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      ObservationStateSlotDAO observationStateSlotDAO =
        (ObservationStateSlotDAO) ModelDAOHelper.DAOFactory.ObservationStateSlotDAO;
      
      if (this.Begin.HasValue && !this.End.HasValue) { // clock-in
        UpdatePeriod ();
        UpdateSiteAttendance (true,
                              new UtcDateTimeRange (this.Begin.Value));
        UpdateMachineObservationStates ();
      }
      else if (!this.Begin.HasValue && this.End.HasValue) { // clock-out
        UpdateUserSlotClockOut ();
        UpdateSiteAttendance (false,
                              new UtcDateTimeRange (this.End.Value));
      }
      else { // full period
        Debug.Assert (this.Begin.HasValue);
        Debug.Assert (this.End.HasValue);
        UpdatePeriod ();
        UpdateSiteAttendance (true,
                              new UtcDateTimeRange (this.Begin.Value, this.End.Value));
        UpdateMachineObservationStates ();
      }
      
      // Analysis is done
      MarkAsCompleted ("");
    }
    
    /// <summary>
    /// Apply the modification while keeping it transient
    /// 
    /// It should be never called, because there is no transient modification to process.
    /// Use a persistent entity instead and MakeAnalysis.
    /// </summary>
    public override void Apply ()
    {
      Debug.Assert (false);
      log.FatalFormat ("Apply: not implemented/supported");
      throw new NotImplementedException ();
    }

    private void UpdateUserSlotClockOut ()
    {
      Debug.Assert (!this.Begin.HasValue);
      Debug.Assert (this.End.HasValue);
      
      // Check first if there is an existing UserSlot
      // that intersects this UserAttendance modification
      IList<IUserSlot> existingUserSlots = ModelDAOHelper.DAOFactory.UserSlotDAO
        .FindOverlapsRange (this.User, new UtcDateTimeRange (this.End.Value));
      
      // Update the existing UserSlot accordingly
      bool intersecting = false;
      foreach (IUserSlot existingUserSlot in existingUserSlots) {
        SetActive ();
        if (Bound.Compare<DateTime> (this.End.Value, existingUserSlot.BeginDateTime) <= 0) {
          // Remove it, it comes after
          string message =
            string.Format ("slot {0} removed because after check-out time {0}",
                           existingUserSlot, this.End.Value);
          log.WarnFormat ("UpdateUserSlotCheckOut: {0}",
                          message);
          AddAnalysisLog (LogLevel.WARN,
                          message);
          ModelDAOHelper.DAOFactory.UserSlotDAO
            .MakeTransient (existingUserSlot);
        }
        else {
          // make it shorter
          ((UserSlot)existingUserSlot).UpdateDateTimeRange (new UtcDateTimeRange (existingUserSlot.BeginDateTime,
                                                                                  this.End.Value));
          intersecting = true;
        }
      }
      if (!intersecting) { // No existing running user slot, raise a warning
        string message = string.Format ("check-out without any running slot" +
                                        "at {0} for user {1}",
                                        this.End.Value, this.User);
        log.WarnFormat ("UpdateUserSlotCheckOut: {0}",
                        message);
        AddAnalysisLog (LogLevel.WARN,
                        message);
      }
    }
    
    private void RemovePeriod (UtcDateTimeRange range)
    {
      // Note: UpdateUserSlotCheckOut is preferred to this method
      // because it raises some logs messages in case there is no corresponding slot
      var userShiftAttendance = new UserShiftAttendance (this.User, false, range, this.MainModification);
      userShiftAttendance.Apply ();
    }

    /// <summary>
    /// Update the period considering [this.Begin, this.End[
    /// </summary>
    private void UpdatePeriod ()
    {
      Debug.Assert (this.Begin.HasValue);
      
      if (null != this.Shift) {
        var userShiftAttendance =
          new UserShiftAttendance (this.User,
                                   new UtcDateTimeRange (this.Begin.Value, new UpperBound<DateTime> (this.End)),
                                   this.MainModification);
        userShiftAttendance.Shift = this.Shift;
        userShiftAttendance.Apply ();
      }
      else { // null == this.Shift
        // Try to determine the shifts from UserShiftSlot
        IList<IUserShiftSlot> userShiftSlots = ModelDAOHelper.DAOFactory.UserShiftSlotDAO
          .FindOverlapsRange (this.User, new UtcDateTimeRange (new LowerBound<DateTime> (this.Begin.Value), new UpperBound<DateTime> (this.End)));
        Bound<DateTime> begin = new LowerBound<DateTime> (this.Begin);
        foreach (IUserShiftSlot userShiftSlot in userShiftSlots) {
          Debug.Assert (null != userShiftSlot.Shift);
          Debug.Assert (begin.HasValue);
          if (Bound.Compare<DateTime> (begin.Value, userShiftSlot.BeginDateTime) < 0) {
            Debug.Assert (userShiftSlot.BeginDateTime.HasValue);
            // No shift in [begin, userShiftSlot.BeginDateTime[
            log.DebugFormat ("UpdateUserSlotPeriod: " +
                             "User={0} and No shift between {1} and {2}",
                             this.User, begin, userShiftSlot.BeginDateTime);
            UserShiftAttendance userShiftAttendance =
              new UserShiftAttendance (this.User, new UtcDateTimeRange (begin.Value, userShiftSlot.BeginDateTime.Value), this.MainModification);
            userShiftAttendance.Apply ();
          }
          {
            begin = userShiftSlot.BeginDateTime;
            UpperBound<DateTime> end = UpperBound.GetMinimum<DateTime> (new UpperBound<DateTime> (this.End),
                                                                        userShiftSlot.EndDateTime);
            log.DebugFormat ("UpdateUserSlotPeriod: " +
                             "User={0} and shift={1} between {2} and {3}",
                             this.User, userShiftSlot.Shift,
                             begin, end);
            UserShiftAttendance userShiftAttendance =
              new UserShiftAttendance (this.User, new UtcDateTimeRange (begin.Value, end), this.MainModification);
            userShiftAttendance.Shift = userShiftSlot.Shift;
            userShiftAttendance.Apply ();
            begin = userShiftSlot.EndDateTime;
          }
        }
        if (Bound.Compare<DateTime> (begin, (UpperBound<DateTime>) this.End) < 0) {
          // No shift in [begin, this.End[
          Debug.Assert (begin.HasValue);
          log.DebugFormat ("UpdateUserSlotPeriod: " +
                           "User={0} and No shift between {1} and {2}",
                           this.User, begin, this.End);
          UserShiftAttendance userShiftAttendance =
            new UserShiftAttendance (this.User, new UtcDateTimeRange (begin.Value, new UpperBound<DateTime>(this.End)), this.MainModification);
          userShiftAttendance.Apply ();
        }
      }
    }
    
    /// <summary>
    /// The global site attendance of a user changed.
    /// 
    /// Take care of updating the ObservationStateSlot and ReasonSlot
    /// accordingly.
    /// </summary>
    /// <param name="newSiteAttendance"></param>
    /// <param name="range"></param>
    void UpdateSiteAttendance (bool newSiteAttendance,
                               UtcDateTimeRange range)
    {
      // Find potential impacted ObservationStateSlots
      IList<IObservationStateSlot> impactedSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
        .GetAttendanceChangeImpacts (this.User, newSiteAttendance, range);

      IModification mainModification = (null != MainModification)
        ? this.MainModification
        : this;
      foreach (IObservationStateSlot impactedSlot in impactedSlots) {
        // Check in the machinestatetemplate table
        // if the MachineStateTemplate should be updated to a new value
        Debug.Assert (null != impactedSlot.MachineStateTemplate); // Because of the request above
        if (null == impactedSlot.MachineStateTemplate) {
          log.FatalFormat ("UpdateSiteAttendance: " +
                           "null machine state template in slot {0}",
                           impactedSlot);
          return;
        }
        IMachineStateTemplate newMachineStateTemplate =
          impactedSlot.MachineStateTemplate.SiteAttendanceChange;
        if (null == newMachineStateTemplate) {
          // No change required, do nothing
          continue;
        }
        
        Debug.Assert (impactedSlot.DateTimeRange.ContainsElement (range.Lower));
        
        // Apply the new MachineStateTemplate
        UtcDateTimeRange modifiedRange = new UtcDateTimeRange (range.Lower,
                                                               UpperBound.GetMinimum<DateTime> (impactedSlot.EndDateTime, range.Upper));
        IMachineStateTemplateAssociation newAssociation = ModelDAOHelper.ModelFactory
          .CreateMachineStateTemplateAssociation (impactedSlot.Machine, newMachineStateTemplate, modifiedRange);
        if (newMachineStateTemplate.UserRequired) {
          newAssociation.User = impactedSlot.User;
        }
        if (newMachineStateTemplate.ShiftRequired) {
          newAssociation.Shift = impactedSlot.Shift;
        }
        newAssociation.Parent = this.MainModification ?? this;
        newAssociation.Priority = this.StatusPriority;
        ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (newAssociation);
      }
    }
    
    /// <summary>
    /// Update the machine observation states from the usermachineslot table
    /// 
    /// this.Begin must have a value
    /// </summary>
    void UpdateMachineObservationStates ()
    {
      Debug.Assert (null != this.User);
      Debug.Assert (this.Begin.HasValue);
      
      // Determine the shift
      IShift shift = this.Shift;
      if (null == shift) { // The shift is unknown, try to get it from the user directly
        shift = this.User.Shift;
      }
      if (null == shift) { // The shift is still unknown, try to get it from usershiftslot
        IList<IUserShiftSlot> userShiftSlots = ModelDAOHelper.DAOFactory.UserShiftSlotDAO
          .FindOverlapsRange (this.User, new UtcDateTimeRange (this.Begin.Value, this.Begin.Value, true, true));
        if (0 < userShiftSlots.Count) {
          Debug.Assert (1 == userShiftSlots.Count);
          shift = userShiftSlots [0].Shift;
        }
      }
      
      IList<IUserMachineSlot> userMachineSlots = ModelDAOHelper.DAOFactory.UserMachineSlotDAO
        .FindOverlapsRange  (this.User, new UtcDateTimeRange (new LowerBound<DateTime> (this.Begin.Value), new UpperBound<DateTime> (this.End)));
      foreach (IUserMachineSlot userMachineSlot in userMachineSlots) {
        Debug.Assert (object.Equals (userMachineSlot.User, this.User));
        UtcDateTimeRange intersection = new UtcDateTimeRange (new UtcDateTimeRange (new LowerBound<DateTime> (this.Begin),
                                                                                    new UpperBound<DateTime> (this.End))
                                                              .Intersects (userMachineSlot.DateTimeRange));
        Debug.Assert (!intersection.IsEmpty ()); // Because slots in the specified range
        foreach (IUserMachineSlotMachine userMachineSlotMachine in userMachineSlot.Machines.Values) {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (userMachineSlotMachine.Machine,
                                                    userMachineSlotMachine.MachineStateTemplate,
                                                    intersection);
          association.User = this.User;
          association.Shift = shift;
          association.Parent = this.MainModification ?? this;
          association.Priority = this.StatusPriority;
          log.DebugFormat ("UpdateMachineObservationStates: " +
                           "create the association machine={0} machineStateTemplate={1} " +
                           "begin={2} end={3}",
                           association.Machine, association.MachineStateTemplate,
                           association.Begin, association.End);
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
        }
      }
    }

    /// <summary>
    /// Get the impacted activity analysis
    /// so that the activity analysis makes a pause
    /// </summary>
    /// <returns></returns>
    public override IList<IMachine> GetImpactedActivityAnalysis ()
    {
      // Managed by the sub-modifications
      return null;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<IUser> (ref m_user);
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }
  }
}
