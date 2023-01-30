// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Persistent class of table AutoSequence
  /// </summary>
  [Serializable]
  public class AutoSequence: IAutoSequence
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineModule m_machineModule; // not null
    ISequence m_sequence;
    IOperation m_operation; // The sequence or the operation is not null
    DateTime m_begin; // not null
    UpperBound<DateTime> m_end;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (AutoSequence).FullName);
    static readonly ILog slog = LogManager.GetLogger(typeof (AutoSequence).FullName);

    #region Getters / Setters
    /// <summary>
    /// AutoSequence Id
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// AutoSequence Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Associated machine module
    /// </summary>
    public virtual IMachineModule MachineModule {
      get { return m_machineModule; }
      protected set
      {
        if (null == value) {
          log.Fatal ("MachineModule can't be null");
          throw new ArgumentNullException ();
        }
        else {
          m_machineModule = value;
          log = LogManager.GetLogger(string.Format ("{0}.{2}.{1}",
                                                    this.GetType ().FullName,
                                                    value.Id, value.MonitoredMachine.Id));
        }
      }
    }
    
    /// <summary>
    /// Associated auto-sequence
    /// </summary>
    public virtual ISequence Sequence {
      get { return m_sequence; }
    }
    
    /// <summary>
    /// Associated operation
    /// </summary>
    public virtual IOperation Operation {
      get { return m_operation; }
    }
    
    /// <summary>
    /// Raw begin date/time of the auto-sequence period
    /// without considering the activities
    /// </summary>
    public virtual DateTime Begin {
      get { return m_begin; }
      set { m_begin = value; }
    }
    
    /// <summary>
    /// Raw end date/time of the auto-sequence period
    /// without considering the activities
    /// </summary>
    public virtual UpperBound<DateTime> End {
      get { return m_end; }
      set { m_end = value; }
    }
    
    /// <summary>
    /// Range [Begin,End)
    /// </summary>
    public virtual UtcDateTimeRange Range {
      get { return new UtcDateTimeRange (this.Begin, this.End); }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected AutoSequence ()
    { }
    
    /// <summary>
    /// Constructors
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="sequence">not null</param>
    /// <param name="begin"></param>
    internal protected AutoSequence(IMachineModule machineModule, ISequence sequence, DateTime begin)
    {
      Debug.Assert (null != sequence);
      
      this.MachineModule = machineModule;
      this.m_sequence = sequence;
      this.m_begin = begin;
    }

    /// <summary>
    /// Constructors
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="operation"></param>
    /// <param name="begin"></param>
    internal protected AutoSequence(IMachineModule machineModule, IOperation operation, DateTime begin)
    {
      Debug.Assert (null != operation);
      
      this.MachineModule = machineModule;
      this.m_operation = operation;
      this.m_begin = begin;
    }
    #endregion // Constructors
    
    /// <summary>
    /// <see cref="Object.ToString()" />
    /// </summary>
    /// <returns></returns>
    public override string ToString ()
    {
      if (Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (this)) {
        return $"[AutoSequence {this.Id} {this.MachineModule?.ToStringIfInitialized ()} {this.Sequence?.ToStringIfInitialized ()} Range={this.Range}]";
      }
      else {
        return $"[AutoSequence {this.Id}]";
      }
    }
  }
}
