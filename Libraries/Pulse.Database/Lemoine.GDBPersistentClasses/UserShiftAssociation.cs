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

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table UserShiftAssociation
  /// 
  /// This new modification table records
  /// any new association between a user and a shift
  /// </summary>
  [Serializable]
  public class UserShiftAssociation: UserAssociation, IUserShiftAssociation
  {
    #region Members
    IShift m_shift;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected UserShiftAssociation ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    public UserShiftAssociation (IUser user, UtcDateTimeRange range)
      : base (user, range)
    {
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "UserShiftAssociation"; }
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
    #endregion // Getters / Setters
    
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      if (typeof (TSlot).Equals (typeof (UserShiftSlot))) {
        if (null == this.Shift) {
          // Do not insert then any UserShiftSlot data
          return null;
        }
        else { // null != this.Shift
          Debug.Assert (null != this.User);
          Debug.Assert (null != this.Shift);
          IUserShiftSlot userShiftSlot = ModelDAO.ModelDAOHelper.ModelFactory
            .CreateUserShiftSlot (this.User, this.Range, this.Shift);
          return (TSlot) userShiftSlot;
        }
      }
      else if (typeof (TSlot).Equals (typeof (UserSlot))) {
        // Do not insert any UserSlot data
        return null;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        throw new NotImplementedException ("Slot type not implemented");
      }
    }

    /// <summary>
    /// <see cref="PeriodAssociation.MergeDataWithOldSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      Debug.Assert (oldSlot is GenericUserSlot);
      Debug.Assert (null != (oldSlot as GenericUserSlot).User);
      Debug.Assert (null != this.User);
      Debug.Assert (object.Equals (this.User, (oldSlot as GenericUserSlot).User));
      
      if (oldSlot is UserShiftSlot) {
        if (null == this.Shift) {
          // Remove the user/shift association
          return null;
        }
        else { // null != this.Shift
          Debug.Assert (null != this.Shift);
          IUserShiftSlot oldUserShiftSlot = oldSlot as UserShiftSlot;
          
          IUserShiftSlot newUserShiftSlot = (IUserShiftSlot) oldUserShiftSlot.Clone ();
          newUserShiftSlot.Shift = this.Shift;
          
          return (TSlot) newUserShiftSlot;
        }
      }
      else if (oldSlot is UserSlot) {
        IUserSlot oldUserSlot = oldSlot as UserSlot;
        
        IUserSlot newUserSlot = (IUserSlot) oldUserSlot.Clone ();
        newUserSlot.Shift = this.Shift;
        
        return (TSlot) newUserSlot;
      }
      else {
        System.Diagnostics.Debug.Assert (false);
        log.FatalFormat ("MergeData: " +
                         "trying to merge the association with a not supported slot {0}",
                         typeof (TSlot));
        throw new ArgumentException ("Not supported user slot");
      }
    }
    
    /// <summary>
    /// Make the analysis
    /// </summary>
    public override void MakeAnalysis()
    {
      Debug.Assert (!IsMainModificationTransient ());
      Debug.Assert (!IsAnalysisCompleted ());

      if (Bound.Compare<DateTime> (this.End, this.Begin) < 0) { // Empty period: error
        string message = string.Format ("End={0} before Begin={1}",
                                        this.End, this.Begin);
        log.ErrorFormat ("MakeAnalysis: " +
                         "{0} " +
                         "=> finish in error",
                         message);
        AddAnalysisLog(LogLevel.ERROR, message);
        MarkAsError ();
        return;
      }

      UserShiftSlotDAO userShiftSlotDAO = new UserShiftSlotDAO ();
      userShiftSlotDAO.Caller = this;
      this.Insert<UserShiftSlot, IUserShiftSlot, UserShiftSlotDAO> (userShiftSlotDAO);
      
      if (null != this.Shift) {
        UserSlotDAO userSlotDAO = new UserSlotDAO ();
        userSlotDAO.Caller = this;
        this.Insert<UserSlot, IUserSlot, UserSlotDAO> (userSlotDAO);

        // Update the observation state slots
        IList<IObservationStateSlot> observationStateSlots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindByUserInRange (this.User, this.Range);
        foreach (IObservationStateSlot observationStateSlot in observationStateSlots) {
          Debug.Assert (object.Equals (observationStateSlot.User, this.User));
          if (object.Equals (observationStateSlot.Shift, this.Shift)) {
            // Nothing to do
            continue;
          }
          // Change the shift
          UtcDateTimeRange intersection = new UtcDateTimeRange (this.Range.Intersects (observationStateSlot.DateTimeRange));
          Debug.Assert (!intersection.IsEmpty ()); // Because observationStateSlots in this.Range
          IMachineObservationStateAssociation newAssociation = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (observationStateSlot.Machine, observationStateSlot.MachineObservationState, intersection);
          newAssociation.MachineStateTemplate = observationStateSlot.MachineStateTemplate;
          newAssociation.User = observationStateSlot.User;
          newAssociation.Shift = this.Shift;
          newAssociation.Parent = this.MainModification ?? this;
          newAssociation.Priority = this.StatusPriority;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (newAssociation);
        }
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

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      base.Unproxy ();
      NHibernateHelper.Unproxy<IShift> (ref m_shift);
    }
  }
}
