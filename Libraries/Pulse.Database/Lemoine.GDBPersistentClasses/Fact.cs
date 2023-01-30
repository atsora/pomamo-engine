// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table Fact
  /// </summary>
  [Serializable]
  public class Fact : IFact, IVersionable, IAutoSequencePeriod
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMonitoredMachine m_machine;
    DateTime m_begin;
    DateTime m_end;
    IMachineMode m_machineMode;
    #endregion

    static readonly ILog staticLog = LogManager.GetLogger (typeof (IFact).FullName);
    ILog log = LogManager.GetLogger (typeof (Fact).FullName);

    #region Getters / Setters
    /// <summary>
    /// Fact Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }

    /// <summary>
    /// Fact Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated monitored machine
    /// </summary>
    public virtual IMonitoredMachine Machine
    {
      get { return m_machine; }
      protected set {
        m_machine = value;
        log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }

    /// <summary>
    /// Begin UTC date/time
    /// </summary>
    public virtual DateTime Begin
    {
      get { return m_begin; }
      protected set {
        switch (value.Kind) {
        case DateTimeKind.Unspecified:
          log.WarnFormat ("Begin.set: " +
                          "unspecified DateTimeKind => suppose it is a universal time");
          m_begin = new DateTime (value.Ticks, DateTimeKind.Utc);
          break;
        case DateTimeKind.Utc:
          m_begin = value;
          break;
        case DateTimeKind.Local:
          m_begin = value.ToUniversalTime ();
          break;
        default:
          throw new Exception ("Invalid value for DateTimeKind");
        }
      }
    }

    /// <summary>
    /// End UTC date/time
    /// </summary>
    public virtual DateTime End
    {
      get { return m_end; }
      set {
        switch (value.Kind) {
        case DateTimeKind.Unspecified:
          log.WarnFormat ("End.set: " +
                          "unspecified DateTimeKind => suppose it is a universal time");
          m_end = new DateTime (value.Ticks, DateTimeKind.Utc);
          break;
        case DateTimeKind.Utc:
          m_end = value;
          break;
        case DateTimeKind.Local:
          m_end = value.ToUniversalTime ();
          break;
        default:
          throw new Exception ("Invalid value for DateTimeKind");
        }
      }
    }

    /// <summary>
    /// Range [Begin,End)
    /// </summary>
    public virtual UtcDateTimeRange Range
    {
      get { return new UtcDateTimeRange (this.Begin, this.End); }
    }

    /// <summary>
    /// Length of the fact
    /// </summary>
    public virtual TimeSpan Length
    {
      get { return m_end.Subtract (m_begin); }
    }

    /// <summary>
    /// Reference to the CNC machine mode (not null)
    /// </summary>
    public virtual IMachineMode CncMachineMode
    {
      get { return m_machineMode; }
      set {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("CncMachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ("Null fact machine mode");
        }
        m_machineMode = value;
      }
    }

    /// <summary>
    /// Is the machine module considered running ?
    /// </summary>
    public virtual bool Running
    {
      get { return CncMachineMode.Running.HasValue && CncMachineMode.Running.Value; }
    }

    /// <summary>
    /// Is the current mode an auto sequence mode ?
    /// </summary>
    public virtual bool AutoSequence
    {
      get { return CncMachineMode.AutoSequence; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected Fact ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="utcBegin"></param>
    /// <param name="utcEnd"></param>
    /// <param name="machineMode"></param>
    public Fact (IMonitoredMachine machine,
                 DateTime utcBegin,
                 DateTime utcEnd,
                 IMachineMode machineMode)
    {
      this.Machine = machine;
      this.Begin = utcBegin;
      this.End = utcEnd;
      this.CncMachineMode = machineMode;
    }
    #endregion // Constructors

    #region IWithDateTimeRange implementation
    /// <summary>
    /// <see cref="IWithDateTimeRange"/>
    /// </summary>
    public virtual UtcDateTimeRange DateTimeRange { get { return this.Range; } }
    #endregion // IWithDateTimeRange implementation

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[Fact {this.Id} {this.Machine?.ToStringIfInitialized ()} Range={this.DateTimeRange}]";
      }
      else {
        return $"[Fact {this.Id}]";
      }
    }
  }
}
