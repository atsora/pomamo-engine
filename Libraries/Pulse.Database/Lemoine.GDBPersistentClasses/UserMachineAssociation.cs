// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table UserMachineAssociation
  /// 
  /// This new modification table records
  /// any new association between a user and a set of machines
  /// </summary>
  [Serializable]
  public class UserMachineAssociation: UserAssociation, IUserMachineAssociation
  {
    #region Members
    IDictionary<int, IUserMachineAssociationMachine> m_machines = new Dictionary<int, IUserMachineAssociationMachine> ();
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected UserMachineAssociation ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    public UserMachineAssociation (IUser user, UtcDateTimeRange range)
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
      get { return "UserMachineAssociation"; }
    }

    /// <summary>
    /// List of associated machine state templates / machines
    /// 
    /// The key is the machine id
    /// </summary>
    public virtual IDictionary<int, IUserMachineAssociationMachine> Machines {
      get { return m_machines; }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// Remove all the associated machines
    /// </summary>
    public virtual void RemoveAll ()
    {
      this.Machines.Clear ();
    }

    /// <summary>
    /// Add a machine and its associate machine state template
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="machineStateTemplate"></param>
    public virtual void Add (IMachine machine, IMachineStateTemplate machineStateTemplate)
    {
      this.Machines [machine.Id] = new UserMachineAssociationMachine (machine, machineStateTemplate);
    }
    
    /// <summary>
    /// Remove a machine
    /// </summary>
    /// <param name="machine"></param>
    public virtual void Remove (IMachine machine)
    {
      this.Machines.Remove (machine.Id);
    }
    #endregion // Methods
    
    /// <summary>
    /// <see cref="PeriodAssociation.ConvertToSlot" />
    /// </summary>
    /// <returns></returns>
    public override TSlot ConvertToSlot<TSlot> ()
    {
      Debug.Assert (null != this.Machines);
      
      if (typeof (TSlot).Equals (typeof (UserMachineSlot))) {
        if (0 == this.Machines.Count) {
          // Do not insert then any UserMachineSlot data
          return null;
        }
        else { // 0 != this.Machines.Count
          Debug.Assert (null != this.User);
          IUserMachineSlot userMachineSlot = ModelDAO.ModelDAOHelper.ModelFactory
            .CreateUserMachineSlot (this.User, this.Range);
          foreach (IUserMachineAssociationMachine userMachineAssociationMachine in this.Machines.Values) {
            userMachineSlot.Add (userMachineAssociationMachine.Machine, userMachineAssociationMachine.MachineStateTemplate);
          }
          return (TSlot) userMachineSlot;
        }
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
    /// <param name="range">Merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != this.Machines);
      Debug.Assert (null != oldSlot);
      Debug.Assert (oldSlot is GenericUserSlot);
      Debug.Assert (null != (oldSlot as GenericUserSlot).User);
      Debug.Assert (null != this.User);
      Debug.Assert (object.Equals (this.User, (oldSlot as GenericUserSlot).User));
      
      if (oldSlot is UserMachineSlot) {
        if (0 == this.Machines.Count) {
          // Remove the user/machines association
          return null;
        }
        else { // 0 != this.Machines.Count
          // There is no merge to do here, you can create a new item
          IUserMachineSlot newUserMachineSlot = ModelDAO.ModelDAOHelper.ModelFactory
            .CreateUserMachineSlot (this.User, this.Range);
          foreach (IUserMachineAssociationMachine userMachineAssociationMachine in this.Machines.Values) {
            newUserMachineSlot.Add (userMachineAssociationMachine.Machine, userMachineAssociationMachine.MachineStateTemplate);
          }
          return (TSlot) newUserMachineSlot;
        }
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

      { // - UserMachineSlot
        UserMachineSlotDAO userMachineSlotDAO = new UserMachineSlotDAO ();
        userMachineSlotDAO.Caller = this;
        this.Insert<UserMachineSlot, IUserMachineSlot, UserMachineSlotDAO> (userMachineSlotDAO);
      }
      
      { // - Re-match UserSlot and UserMachineAssociation
        // Note: this only adds some new machine state template, and do not remove
        //       any old machine state template associations that might be not valid any more
        //       But this should be sufficient for now
        IList<IUserSlot> userSlots = ModelDAOHelper.DAOFactory.UserSlotDAO
          .FindOverlapsRange (this.User, this.Range);
        foreach (IUserSlot userSlot in userSlots) {
          Debug.Assert (object.Equals (userSlot.User, this.User));
          UtcDateTimeRange intersection = new UtcDateTimeRange (this.Range.Intersects (userSlot.DateTimeRange));
          Debug.Assert (!intersection.IsEmpty ()); // Because slots in this.Range
          foreach (IUserMachineAssociationMachine element in this.Machines.Values) {
            IMachineStateTemplateAssociation newAssociation = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (element.Machine, element.MachineStateTemplate, intersection);
            newAssociation.Parent = this.MainModification ?? this;
            newAssociation.Priority = this.StatusPriority;
            ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO.MakePersistent (newAssociation);
          }
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
      foreach (IUserMachineAssociationMachine userMachineAssociationMachine in m_machines.Values) {
        ((UserMachineAssociation)userMachineAssociationMachine).Unproxy ();
      }
    }
  }
}
