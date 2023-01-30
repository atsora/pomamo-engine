// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Business.Config;
using Lemoine.Extensions.Database;
using System.Linq;
using Pulse.Extensions.Database;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table BetweenCycles
  /// </summary>
  [Serializable]
  public class BetweenCycles : IBetweenCycles, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachine m_machine;
    DateTime m_begin;
    DateTime m_end;
    IOperationCycle m_previousCycle;
    IOperationCycle m_nextCycle;
    double? m_offsetDuration = null;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (BetweenCycles).FullName);

    #region Getters / Setters
    /// <summary>
    /// BetweenCycles Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// BetweenCycles Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the monitored machine
    /// </summary>
    public virtual IMachine Machine
    {
      get { return this.m_machine; }
    }

    /// <summary>
    /// Begin date/time of the period
    /// </summary>
    public virtual DateTime Begin
    {
      get { return this.m_begin; }
    }

    /// <summary>
    /// End date/time of the period
    /// </summary>
    public virtual DateTime End
    {
      get { return this.m_end; }
    }

    /// <summary>
    /// Previous operation cycle
    /// </summary>
    public virtual IOperationCycle PreviousCycle
    {
      get
      {
        Debug.Assert (null != m_previousCycle);
        return m_previousCycle;
      }
    }

    /// <summary>
    /// Next operation cycle
    /// </summary>
    public virtual IOperationCycle NextCycle
    {
      get
      {
        Debug.Assert (null != m_nextCycle);
        return m_nextCycle;
      }
      set
      {
        Debug.Assert (null != value);
        if (!object.Equals (m_nextCycle, value)) {
          m_nextCycle = value;
          OperationCycle operationCycle = (OperationCycle)value;
          UpdateOffsetDuration ();
        }
      }
    }

    /// <summary>
    /// <see cref="Lemoine.Model.IBetweenCycles.OffsetDuration">IBetweenCycles.OffsetDuration</see>
    /// </summary>
    public virtual Nullable<Double> OffsetDuration
    {
      get { return m_offsetDuration; }
      protected set
      {
        if (object.Equals (m_offsetDuration, value)) {
          // Nothing to do
          return;
        }
        else {
          m_offsetDuration = value;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// 
    /// Warning: the operation and offset duration is not set in this method
    /// 
    /// previousCycle and nextCycle must not be transient
    /// </summary>
    /// <param name="previousCycle"></param>
    /// <param name="nextCycle"></param>
    internal BetweenCycles (IOperationCycle previousCycle, IOperationCycle nextCycle)
    {
      Debug.Assert (null != previousCycle);
      Debug.Assert (null != nextCycle);
      Debug.Assert (previousCycle.Machine.Equals (nextCycle.Machine));
      Debug.Assert (previousCycle.End.HasValue);
      Debug.Assert (nextCycle.Begin.HasValue);

      // previousCycle and nextCycle must not be transient because the bidirectional link
      // between OperationCycle and BetweenCycles was removed
      // and else UpdateOffsetDuration won't work correctly
      Debug.Assert (0 != previousCycle.Id);
      Debug.Assert (0 != nextCycle.Id);

      m_machine = previousCycle.Machine;
      m_begin = previousCycle.End.Value;
      m_end = nextCycle.Begin.Value;
      m_previousCycle = previousCycle;
      m_nextCycle = nextCycle;
    }

    /// <summary>
    /// Protected constructor for NHibernate
    /// </summary>
    internal protected BetweenCycles ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Update the internal OffsetDuration property
    /// 
    /// This method must be run in a session
    /// and the operation must be correct before calling this method (running for example UpdateOperation)
    /// </summary>
    internal protected virtual void UpdateOffsetDuration ()
    {
      var extensions = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Extension.GlobalExtensions<IBetweenCyclesOffsetDurationExtension> ());
      if (extensions.Any ()) {
        if (1 < extensions.Count () && log.IsWarnEnabled) {
          log.WarnFormat ("UpdateOffsetDuration: {0} extensions define an offset duration",
            extensions.Count ());
        }
        var extension = extensions
          .OrderByDescending (x => x.Priority)
          .First ();
        var offsetDuration = extension.ComputeOffsetDuration (this);
        if (log.IsDebugEnabled) {
          log.DebugFormat ("UpdateOffsetDuration: {0}", offsetDuration);
        }
        this.OffsetDuration = offsetDuration;
        return;
      }

      // Default:
      log.Debug ("UpdateOffsetDuration: " +
                 "no offset duration could be determined");
      this.OffsetDuration = null;
    }

    /// <summary>
    /// <see cref="Object.ToString" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[BetweenCycles {this.Id} {this.Machine?.ToStringIfInitialized ()} Begin={this.Begin} End={this.End}]";
      }
      else {
        return $"[BetweenCycles {this.Id}]";
      }
    }

    /// <summary>
    ///   Indicates whether the current object
    ///   is equal to another object of the same type
    /// </summary>
    /// <param name="other">An object to compare with this object</param>
    /// <returns>true if the current object is equal to the other parameter; otherwise, false</returns>
    public virtual bool Equals (IBetweenCycles other)
    {
      return this.Equals ((object)other);
    }

    /// <summary>
    ///   Determines whether the specified Object
    ///   is equal to the current Object
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
      BetweenCycles other = obj as BetweenCycles;
      if (null == other) {
        return false;
      }
      if (this.Id != 0) {
        return (other.Id == this.Id);
      }
      return false;
    }

    /// <summary>
    ///   Serves as a hash function for a particular type
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
        return base.GetHashCode ();
      }
    }
  }
}
