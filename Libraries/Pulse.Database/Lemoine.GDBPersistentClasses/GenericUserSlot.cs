// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Description of UserSlot.
  /// </summary>
  public abstract class GenericUserSlot: BeginEndSlot, ISlot
  {
    ILog log = LogManager.GetLogger(typeof (GenericUserSlot).FullName);
    
    /// <summary>
    /// <see cref="Slot.GetLogger" />
    /// </summary>
    /// <returns></returns>
    protected override ILog GetLogger()
    {
      return log;
    }
    
    #region Members
    /// <summary>
    /// Associated user
    /// </summary>
    protected IUser m_user;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Reference to the User
    /// </summary>
    public virtual IUser User {
      get { return m_user; }
      protected set
      {
        m_user = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// The default constructor should not be used outside the assembly
    /// 
    /// This constructor must be followed by the Initialize method.
    /// </summary>
    protected GenericUserSlot ()
    {
    }
    
    /// <summary>
    /// Create a new UserSlot (factory method)
    /// </summary>
    /// <param name="type">Type of the User slot to create</param>
    /// <param name="user"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static GenericUserSlot Create (Type type,
                                          IUser user,
                                          UtcDateTimeRange range)
    {
      Debug.Assert (null != user);
      return (GenericUserSlot)
        type.GetConstructor (new Type[] {typeof (IUser), typeof (UtcDateTimeRange)})
        .Invoke (new Object[] {user, range});
    }

    /// <summary>
    /// Constructor (to override)
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    protected GenericUserSlot (IUser user,
                               UtcDateTimeRange range)
      : base (range)
    {
      Debug.Assert (null != user);
      if (null == user) {
        log.FatalFormat ("UserShiftSlot: " +
                         "user can't be null");
        throw new ArgumentNullException ("user");
      }
      this.m_user = user;
    }
    #endregion // Constructors
    
    #region Methods
    /// <summary>
    /// <see cref="Object.GetHashCode" />
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      int hashCode = 0;
      unchecked {
        hashCode += 1000000007 * BeginDateTime.GetHashCode();
        if (this.User != null) {
          hashCode += 1000000009 * this.User.GetHashCode();
        }
      }
      return hashCode;
    }
    
    /// <summary>
    /// <see cref="Object.Equals(object)" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      GenericUserSlot other = obj as GenericUserSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime)
        && object.Equals(this.User, other.User);
    }
    #endregion // Methods
  }
}
