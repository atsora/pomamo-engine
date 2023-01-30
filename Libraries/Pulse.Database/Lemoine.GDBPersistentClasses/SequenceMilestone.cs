// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Serialization;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table SequenceMilestone
  /// </summary>
  public class SequenceMilestone
    : ISequenceMilestone
    , IVersionable
    , IEquatable<ISequenceMilestone>
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineModule m_machineModule;
    DateTime m_dateTime;
    ISequence m_sequence;
    TimeSpan m_milestone;
    bool m_completed = false;
    #endregion // Members

    ILog log = LogManager.GetLogger (typeof (SequenceMilestone).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected SequenceMilestone ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    public SequenceMilestone (IMachineModule machineModule)
    {
      this.MachineModule = machineModule;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// ID of the reference machine module
    /// </summary>
    public virtual int Id
    {
      get { return m_id; }
    }

    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return m_version; }
    }

    /// <summary>
    /// Reference to the MachineModule
    /// 
    /// not null
    /// </summary>
    public virtual IMachineModule MachineModule
    {
      get { return m_machineModule; }
      protected set {
        m_machineModule = value;
        log = LogManager.GetLogger ($"{this.GetType ().FullName}.{value.Id}");
      }
    }

    /// <summary>
    /// UTC Date/time of the milestone
    /// </summary>
    public virtual DateTime DateTime
    {
      get { return m_dateTime; }
      set { m_dateTime = value; }
    }

    /// <summary>
    /// <see cref="ISequenceMilestone"/>
    /// </summary>
    public virtual ISequence Sequence
    {
      get { return m_sequence; }
      set { m_sequence = value; }
    }

    /// <summary>
    /// <see cref="ISequenceMilestone"/>
    /// </summary>
    public virtual TimeSpan Milestone
    {
      get { return m_milestone; }
      set { m_milestone = value; }
    }

    /// <summary>
    /// <see cref="ISequenceMilestone"/>
    /// </summary>
    public virtual bool Completed
    {
      get { return m_completed; }
      set { m_completed = value; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// ToString
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      return $"[SequenceMilestone {this.Id}]";
    }

    /// <summary>
    /// Indicates whether the current object
    /// is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (ISequenceMilestone other)
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
      if (obj is null) {
        return false;
      }

      // Note: do not use here this.GetType () != obj.GetType
      //       because a Xxx may be compared with a XxxProxy
      //       which may return false although true might be returned
      ISequenceMilestone other = obj as SequenceMilestone;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return this.Id.Equals (other.Id)
          && this.Version.Equals (other.Version);
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
        }
        return hashCode;
      }
    }
  }
}
