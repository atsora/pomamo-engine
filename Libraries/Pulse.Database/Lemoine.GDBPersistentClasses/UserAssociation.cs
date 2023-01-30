// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Abstract base class for all the user association modification tables
  /// </summary>
  [Serializable,
   XmlInclude(typeof(UserShiftAssociation)),
   XmlInclude(typeof (UserAttendance))]
  public abstract class UserAssociation: PeriodAssociation
  {
    #region Members
    IUser m_user;
    #endregion

    #region Getters / Setters
    /// <summary>
    /// Reference to the User
    /// 
    /// It can't be null
    /// </summary>
    [XmlIgnore]
    public virtual IUser User {
      get { return m_user; }
      set
      {
        if (null == value) {
          log.Fatal ("UserAssociation: " +
                     "User can't be null");
          Debug.Assert (null != value);
          throw new ArgumentNullException ();
        }
        else {
          m_user = value;
          log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                    this.GetType ().FullName,
                                                    value.Id));
        }
      }
    }
    
    /// <summary>
    /// Reference to the User
    /// for Xml Serialization
    /// 
    /// It can't be null
    /// </summary>
    [XmlElement("User")]
    public virtual User XmlSerializationUser {
      get { return this.User as User; }
      set { this.User = value; }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    protected UserAssociation ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    protected UserAssociation (IUser user, UtcDateTimeRange range)
      : base (range)
    {
      this.User = user;
    }

    /// <summary>
    /// Constructor for abstract modifications, that are kept transient
    /// 
    /// You may add a mainModification reference to be used in the analysis logs
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <param name="mainModification"></param>
    internal protected UserAssociation (IUser user, UtcDateTimeRange range, IModification mainModification)
      : base (range, mainModification)
    {
      this.User = user;
    }
    #endregion // Constructors
    
    /// <summary>
    /// Get the impacted slots without considering any pre-fetched slots
    /// </summary>
    /// <param name="slotDAO">DAO to use to update the slot</param>
    /// <param name="pastOnly">Apply the association on the existing past slots only</param>
    public override IList<I> GetImpactedSlots<TSlot, I, TSlotDAO> (TSlotDAO slotDAO,
                                                                      bool pastOnly)
    {
      bool leftMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoLeftMerge);
      bool rightMerge = !this.Option.HasValue
        || !this.Option.Value.HasFlag (AssociationOption.NoRightMerge);
      
      IList<I> impactedSlots = slotDAO
        .GetImpactedUserSlotsForAnalysis (this.User,
                                          this.Range,
                                          this.DateTime,
                                          pastOnly,
                                          leftMerge,
                                          rightMerge);
      return impactedSlots;
    }

    /// <summary>
    /// <see cref="Lemoine.Model.ISerializableModel"></see>
    /// </summary>
    public override void Unproxy ()
    {
      string modificationType = this.ModificationType;
      NHibernateHelper.Unproxy<IUser> (ref m_user);
    }
  }
}
