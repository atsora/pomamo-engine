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
  /// Persistent class of table CurrentMachineMode
  /// </summary>
  [Serializable]
  public class CurrentMachineMode : ICurrentMachineMode, IVersionable
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMonitoredMachine m_monitoredMachine;
    DateTime m_dateTime = DateTime.UtcNow;
    DateTime m_change = DateTime.UtcNow;
    IMachineMode m_machineMode;
    #endregion

    static readonly ILog staticLog = LogManager.GetLogger (typeof (CurrentMachineMode).FullName);
    ILog log = LogManager.GetLogger (typeof (CurrentMachineMode).FullName);

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
    public virtual IMonitoredMachine MonitoredMachine
    {
      get { return m_monitoredMachine; }
      protected set {
        m_monitoredMachine = value;
        log = LogManager.GetLogger (string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }

    /// <summary>
    /// UTC date/time stamp
    /// </summary>
    public virtual DateTime DateTime
    {
      get { return m_dateTime; }
      set {
        switch (value.Kind) {
        case DateTimeKind.Unspecified:
          log.WarnFormat ("DateTime.set: " +
                          "unspecified DateTimeKind => suppose it is a universal time");
          m_dateTime = new DateTime (value.Ticks, DateTimeKind.Utc);
          break;
        case DateTimeKind.Utc:
          m_dateTime = value;
          break;
        case DateTimeKind.Local:
          m_dateTime = value.ToUniversalTime ();
          break;
        default:
          throw new Exception ("Invalid value for DateTimeKind");
        }
      }
    }

    /// <summary>
    /// Reference to the CNC machine mode
    /// </summary>
    public virtual IMachineMode MachineMode
    {
      get { return m_machineMode; }
      set {
        Debug.Assert (null != value);
        if (null == value) {
          log.ErrorFormat ("MachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ("Null fact machine mode");
        }

        var now = DateTime.UtcNow;
        this.DateTime = now;
        if (null == m_machineMode) {
          this.Change = now;
          m_machineMode = value;
        }
        else if (value.Id != m_machineMode.Id) {
          this.Change = now;
          m_machineMode = value;
        }
      }
    }

    /// <summary>
    /// UTC date/time change
    /// </summary>
    public virtual DateTime Change
    {
      get { return m_change; }
      protected internal set {
        switch (value.Kind) {
        case DateTimeKind.Unspecified:
          log.WarnFormat ("Change.set: " +
                          "unspecified DateTimeKind => suppose it is a universal time");
          m_change = new DateTime (value.Ticks, DateTimeKind.Utc);
          break;
        case DateTimeKind.Utc:
          m_change = value;
          break;
        case DateTimeKind.Local:
          m_change = value.ToUniversalTime ();
          break;
        default:
          throw new Exception ("Invalid value for DateTimeKind");
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected CurrentMachineMode ()
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    internal protected CurrentMachineMode (IMonitoredMachine machine)
    {
      this.MonitoredMachine = machine;
    }
    #endregion // Constructors

    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[CurrentMachineMode {this.Id} {this.MonitoredMachine?.ToStringIfInitialized ()}]";
      }
      else {
        return $"[CurrentMachineMode {this.Id}]";
      }
    }
  }
}
