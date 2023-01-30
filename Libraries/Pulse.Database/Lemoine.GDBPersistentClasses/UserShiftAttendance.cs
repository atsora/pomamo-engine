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
  /// Modification class to easily process and insert the changes of user attendance.
  /// The associated shift is pre-fetched before using this class.
  /// 
  /// This is not a persistent class, no database table is associated to it !
  /// </summary>
  public class UserShiftAttendance: UserAssociation
  {
    #region Members
    IShift m_shift;
    bool m_addUser = true;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    internal protected UserShiftAttendance (IUser user, UtcDateTimeRange range, IModification mainModification)
      : base (user, range, mainModification)
    {
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="user"></param>
    /// <param name="addUser">false if you want to remove this period of time</param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    internal protected UserShiftAttendance (IUser user, bool addUser, UtcDateTimeRange range, IModification mainModification)
      : base (user, range, mainModification)
    {
      m_addUser = addUser;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// <see cref="IModification"/>
    /// </summary>
    [XmlIgnore]
    public override string ModificationType
    {
      get { return "UserShiftAttendance"; }
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
      if (typeof (TSlot).Equals (typeof (UserSlot))) {
        if (m_addUser) {
          IUserSlot userSlot = ModelDAOHelper.ModelFactory
            .CreateUserSlot (this.User, this.Range);
          if (null != this.Shift) {
            userSlot.Shift = this.Shift;
          }
          else { // Fallback to default user shift
            userSlot.Shift = this.User.Shift;
          }
          return (TSlot) userSlot;
        }
        else { // Do not add the user
          return null;
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
    /// <param name="range">merge period range</param>
    /// <returns></returns>
    public override TSlot MergeDataWithOldSlot<TSlot>(TSlot oldSlot,
                                                      UtcDateTimeRange range)
    {
      Debug.Assert (null != oldSlot);
      Debug.Assert (oldSlot is UserSlot);
      Debug.Assert (null != (oldSlot as GenericUserSlot).User);
      Debug.Assert (null != this.User);
      Debug.Assert (object.Equals (this.User, (oldSlot as GenericUserSlot).User));
      
      if (oldSlot is UserSlot) {
        if (m_addUser) {
          IUserSlot oldUserSlot = oldSlot as UserSlot;
          
          IUserSlot newUserSlot = (IUserSlot) oldUserSlot.Clone ();
          if (null != this.Shift) {
            newUserSlot.Shift = this.Shift;
          } // else keep its shift
          
          return (TSlot) newUserSlot;
        }
        else { // false == m_addUser => remove the period
          return null;
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
      Debug.Assert (!IsAnalysisCompleted ());

      Apply ();
      
      // Analysis is done
      MarkAsCompleted ("");
    }

    /// <summary>
    /// Apply the modification while keeping it transient
    /// </summary>
    public override void Apply ()
    {
      UserSlotDAO userSlotDAO = new UserSlotDAO ();
      userSlotDAO.Caller = this;
      this.Insert<UserSlot, IUserSlot, UserSlotDAO> (userSlotDAO);
    }
  }
}
