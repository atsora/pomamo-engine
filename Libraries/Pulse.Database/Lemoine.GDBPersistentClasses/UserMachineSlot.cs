// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table UserMachineSlot
  /// 
  /// Analysis table where are stored all the periods where a user is associated to Machine.
  /// </summary>
  [Serializable]
  public class UserMachineSlot
    : GenericUserSlot
    , IUserMachineSlot
    , ICloneable
  {
    #region Members
    IDictionary<int, IUserMachineSlotMachine> m_machines = new Dictionary<int, IUserMachineSlotMachine> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UserMachineSlot).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected UserMachineSlot()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="user">not null</param>
    /// <param name="range"></param>
    public UserMachineSlot(IUser user, UtcDateTimeRange range)
      : base (user, range)
    {
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Reference to the Machines (not null)
    /// </summary>
    public virtual IDictionary<int, IUserMachineSlotMachine> Machines {
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
      this.Machines [machine.Id] = new UserMachineSlotMachine (machine, machineStateTemplate);
    }
    
    /// <summary>
    /// Remove a machine
    /// </summary>
    /// <param name="machine"></param>
    public virtual void Remove (IMachine machine)
    {
      this.Machines.Remove (machine.Id);
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo(object obj)
    {
      if (obj is UserMachineSlot) {
        IUserMachineSlot other = (IUserMachineSlot) obj;
        if (other.User.Equals (this.User)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare user slots " +
                           "for different users {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of UserMachineSlot from different users");
        }
      }
      
      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not a UserMachineSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo(IUserMachineSlot other)
    {
      if (other.User.Equals (this.User)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare user slots " +
                       "for different users {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of UserMachineSlot from different users");
    }
    
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      if (object.ReferenceEquals(this,obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      UserMachineSlot other = obj as UserMachineSlot;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }
    
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      if (0 != Id) {
        return this.Id.GetHashCode ();
      }
      else {
        return base.GetHashCode ();
      }
    }
    
    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // There is nothing to do for this slot
    }
    
    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // There is nothing to do for this slot
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty()
    {
      return (0 == this.Machines.Count);
    }
    
    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals(ISlot obj)
    {
      IUserMachineSlot other = obj as IUserMachineSlot;
      if (other == null) {
        return false;
      }

      if (!NHibernateHelper.EqualsNullable (this.User, other.User, (a, b) => a.Id == b.Id)) {
        return false;
      }
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        return object.Equals (this.Machines, other.Machines); // There is a risk to have to unproxy an object
      }
    }
    #endregion // Methods
    
    #region ICloneable implementation
    /// <summary>
    /// Make a shallow copy
    /// <see cref="ICloneable.Clone" />
    /// </summary>
    /// <returns></returns>
    public override object Clone()
    {
      UserMachineSlot clone = base.Clone() as UserMachineSlot;
      clone.m_machines.Clear ();
      foreach (IUserMachineSlotMachine slotMachine in this.Machines.Values) {
        clone.Add (slotMachine.Machine, slotMachine.MachineStateTemplate);
      }
      return clone;
    }
    #endregion // ICloneable implementation

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[UserMachineSlot {this.Id} {this.User?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[UserMachineSlot {this.Id}]";
      }
    }
  }
}
