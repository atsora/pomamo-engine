// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table CncVariable
  /// </summary>
  public class CncVariable : GenericMachineModuleRangeSlot, ICncVariable, IVersionable, Lemoine.Collections.IDataWithId
  {
    #region Members
    string m_key = "";
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CncVariable).FullName);

    #region Getters / Setters
    /// <summary>
    /// Variable key
    /// </summary>
    public virtual string Key
    {
      get { return m_key; }
    }

    /// <summary>
    /// Variable value
    /// </summary>
    public virtual object Value { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Protected constructor with no arguments
    /// </summary>
    protected CncVariable () : base (false) { }

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="range"></param>
    /// <param name="key">not null</param>
    /// <param name="v">not null</param>
    public CncVariable (IMachineModule machineModule, UtcDateTimeRange range, string key, object v)
      : base (false, machineModule, range)
    {
      Debug.Assert (null != key);
      Debug.Assert (null != v);

      m_key = key;
      this.Value = v;
    }
    #endregion // Constructors

    /// <summary>
    /// Make the Cnc variable slot shorter
    /// </summary>
    /// <param name="newUpperBound"></param>
    public virtual void Stop (UpperBound<DateTime> newUpperBound)
    {
      Debug.Assert (!this.DateTimeRange.Upper.HasValue);
      Debug.Assert (Bound.Compare<DateTime> (newUpperBound, this.DateTimeRange.Upper) <= 0);

      bool upperInclusive = Bound.Equals<DateTime> (this.DateTimeRange.Lower, newUpperBound);
      Debug.Assert (!upperInclusive); // Right now, it should never be inclusive
      UtcDateTimeRange newRange =
        new UtcDateTimeRange (this.DateTimeRange.Lower, newUpperBound, true, upperInclusive);
      this.UpdateDateTimeRange (newRange);
    }

    /// <summary>
    /// IComparable implementation
    /// <see cref="IComparable.CompareTo" />
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override int CompareTo (object obj)
    {
      if (obj is CncVariable) {
        var other = (ICncVariable)obj;
        return CompareTo (other);
      }

      GetLogger ().ErrorFormat ("CompareTo: " +
                               "object {0} of invalid type",
                               obj);
      throw new ArgumentException ("object is not a ICncVariable");
    }

    /// <summary>
    /// IComparable implementation
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public virtual int CompareTo (ICncVariable other)
    {
      if (other.MachineModule.MonitoredMachine.Id == this.MachineModule.MonitoredMachine.Id) {
        int comparison = this.DateTimeRange.CompareTo (other.DateTimeRange);
        if (0 == comparison) {
          return string.Compare (this.Key, other.Key, StringComparison.InvariantCulture);
        }
        return comparison;
      }
      else {
        GetLogger ().ErrorFormat ("CompareTo: " +
                                  "trying to compare cnc variables " +
                                  "for different machine modules {0} {1}",
                                  this, other);
        throw new ArgumentException ("Comparison of cnc variables from different machine modules");
      }
    }

    /// <summary>
    /// Slot implementation
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool ReferenceDataEquals (ISlot obj)
    {
      var other = obj as ICncVariable;
      if (other == null) {
        return false;
      }

      return object.Equals (this.Key, other.Key)
        && object.Equals (this.Value, other.Value);
    }

    /// <summary>
    /// <see cref="Slot.IsEmpty" />
    /// </summary>
    /// <returns></returns>
    public override bool IsEmpty ()
    {
      return this.DateTimeRange.IsEmpty ();
    }

    /// <summary>
    /// <see cref="Slot.HandleAddedSlot" />
    /// </summary>
    public override void HandleAddedSlot ()
    {
      // Normally not used, because it is not a real analysis slot
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Slot.HandleModifiedSlot" />
    /// </summary>
    /// <param name="oldSlot"></param>
    public override void HandleModifiedSlot (ISlot oldSlot)
    {
      // Normally not used, because it is not a real analysis slot
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Slot.HandleRemovedSlot" />
    /// </summary>
    public override void HandleRemovedSlot ()
    {
      // Normally not used, because it is not a real analysis slot
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[CncVariable {this.Id} {this.Key}={this.Value}]";
      }
      else {
        return $"[CncVariable {this.Id}]";
      }
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ICncVariable other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    /// Determines whether the specified Object
    /// is equal to the current Object
    /// </summary>
    /// <param name="obj">The object to compare with the current object</param>
    /// <returns>true if the specified Object is equal to the current Object; otherwise, false</returns>
    public override bool Equals (object obj)
    {
      if (object.ReferenceEquals (this, obj)) {
        return true;
      }

      if (obj == null) {
        return false;
      }
      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ICncVariable other = obj as CncVariable;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type
    /// </summary>
    /// <returns>A hash code for the current Object</returns>
    public override int GetHashCode ()
    {
      if (0 != Id) {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * Id.GetHashCode ();
        }
        return hashCode;
      }
      else {
        int hashCode = 0;
        unchecked {
          hashCode += 1000000007 * base.GetHashCode ();
          hashCode += 1000000009 * this.Key.GetHashCode ();
          hashCode += 1000000011 * this.Value.GetHashCode ();
        }
        return hashCode;
      }
    }
  }
}
