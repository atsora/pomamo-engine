// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table UserSlot
  /// 
  /// Analysis table where are stored all the user periods on the site.
  /// </summary>
  [Serializable]
  public class UserSlot
    : GenericUserSlot
    , IUserSlot
    , ICloneable
  {
    #region Members
    IShift m_shift;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (UserSlot).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected UserSlot()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="user"></param>
    /// <param name="range"></param>
    public UserSlot(IUser user, UtcDateTimeRange range)
      : base (user, range)
    {
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// (optional) Reference to the Shift
    /// </summary>
    public virtual IShift Shift {
      get { return m_shift; }
      set { m_shift = value; }
    }
    #endregion // Getters / Setters
    
    #region Methods
    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo(object obj)
    {
      if (obj is UserSlot) {
        IUserSlot other = (IUserSlot) obj;
        if (other.User.Equals (this.User)) {
          return this.BeginDateTime.CompareTo (other.BeginDateTime);
        }
        else {
          log.ErrorFormat ("CompareTo: " +
                           "trying to compare user slots " +
                           "for different users {0} {1}",
                           this, other);
          throw new ArgumentException ("Comparison of UserSlot from different users");
        }
      }
      
      log.ErrorFormat ("CompareTo: " +
                       "object {0} of invalid type",
                       obj);
      throw new ArgumentException ("object is not a UserSlot");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo(IUserSlot other)
    {
      if (other.User.Equals (this.User)) {
        return this.BeginDateTime.CompareTo (other.BeginDateTime);
      }

      log.ErrorFormat ("CompareTo: " +
                       "trying to compare user slots " +
                       "for different users {0} {1}",
                       this, other);
      throw new ArgumentException ("Comparison of UserSlot from different users");
    }
    
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

        if (this.Shift != null) {
          hashCode += 1000000011 * this.Shift.GetHashCode();
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
      UserSlot other = obj as UserSlot;
      if (other == null) {
        return false;
      }

      return Bound.Equals<DateTime> (this.BeginDateTime, other.BeginDateTime)
        && object.Equals(this.User, other.User)
        && object.Equals(this.Shift, other.Shift);
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
      return false;
    }
    
    /// <summary>
    /// <see cref="Slot.ReferenceDataEquals" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals(ISlot obj)
    {
      IUserSlot other = obj as IUserSlot;
      if (other == null) {
        return false;
      }

      return NHibernateHelper.EqualsNullable (this.User, other.User, (a, b) => a.Id == b.Id)
        && NHibernateHelper.EqualsNullable (this.Shift, other.Shift, (a, b) => a.Id == b.Id);
    }
    #endregion // Methods

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[UserSlot {this.Id} {this.User} Range={this.DateTimeRange}]";
      }
      else {
        return $"[UserSlot {this.Id}]";
      }
    }
  }
}
