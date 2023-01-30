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
  /// Persistent class of table MachineModeDefaultReason
  /// 
  /// The default reason that is associated to a machine mode
  /// for a given machine observation state is defined in this table.
  /// </summary>
  [Serializable]
  public class MachineModeDefaultReason: IMachineModeDefaultReason
  {
    static readonly int DEFAULT_SCORE = 10;

    #region Members
    int m_id = 0;
    int m_version = 0;
    IMachineMode m_machineMode;
    IMachineObservationState m_machineObservationState;
    IMachineFilter m_includeMachineFilter = null;
    IMachineFilter m_excludeMachineFilter = null;
    TimeSpan? m_maximumDuration = null;
    IReason m_reason;
    bool m_overwriteRequired = false;
    bool m_auto = false;
    double m_score = DEFAULT_SCORE;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeDefaultReason).FullName);

    #region Constructors
    /// <summary>
    /// The default constructor is forbidden
    /// </summary>
    protected MachineModeDefaultReason ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineMode"></param>
    /// <param name="machineObservationState"></param>
    public MachineModeDefaultReason(IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != machineObservationState);
      this.m_machineMode = machineMode;
      this.m_machineObservationState = machineObservationState;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// MachineModeDefaultReason ID
    /// </summary>
    public virtual int Id
    {
      get { return this.m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version
    {
      get { return this.m_version; }
    }

    /// <summary>
    /// Reference to the Machine Mode
    /// </summary>
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
    }
    
    /// <summary>
    /// Reference to the Machine Observation State
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
    }
    
    /// <summary>
    /// Include a set of machine from the configuration with the help of a IMachineFilter
    /// </summary>
    public virtual IMachineFilter IncludeMachineFilter {
      get { return m_includeMachineFilter; }
      set { m_includeMachineFilter = value; }
    }

    /// <summary>
    /// Exclude a set of machine from the configuration with the help of a IMachineFilter
    /// </summary>
    public virtual IMachineFilter ExcludeMachineFilter {
      get { return m_excludeMachineFilter; }
      set { m_excludeMachineFilter = value; }
    }
    
    /// <summary>
    /// Maximum duration in seconds. If null, there is no limitation
    /// </summary>
    public virtual Nullable<TimeSpan> MaximumDuration {
      get { return m_maximumDuration; }
      set { m_maximumDuration = value; }
    }
    
    /// <summary>
    /// Reference to the default Reason
    /// </summary>
    public virtual IReason Reason {
      get { return m_reason; }
      set
      {
        if (null == value) {
          Debug.Assert (null != value);
          log.ErrorFormat ("Reason.set: " +
                           "a null reason is not allowed");
          throw new ArgumentNullException ("MachineModeDefaultReason.Reason");
        }
        m_reason = value;
      }
    }
    
    /// <summary>
    /// If TRUE, the operator must assign a new real reason to this period
    /// </summary>
    public virtual bool OverwriteRequired {
      get { return m_overwriteRequired; }
      set { m_overwriteRequired = value; }
    }

    /// <summary>
    /// Consider this is an auto reason
    /// </summary>
    public virtual bool Auto {
      get { return m_auto; }
      set { m_auto = value; }
    }

    /// <summary>
    /// Reason score
    /// </summary>
    public virtual double Score {
      get { return m_score;  }
      set { m_score = value; }
    }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Check if this default reason is applicable to the machine
    /// 
    /// Note: IncludeMachineFilter and ExcludeMachineFilter must be known,
    ///       so you probably want to run it inside a session
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    public virtual bool IsApplicableToMachine (IMachine machine)
    {
      // Test the includemachinefilter is ok
      if (null != this.IncludeMachineFilter) {
        if (!this.IncludeMachineFilter.IsMatch (machine)) {
          log.DebugFormat ("MatchMachine: " +
                           "the machine {0} does not match the include machine filter {1}",
                           machine,
                           IncludeMachineFilter);
          return false;
        }
      }
      
      // Test the excludemachinefilter is ok
      if (null != this.ExcludeMachineFilter) {
        if (this.ExcludeMachineFilter.IsMatch (machine)) {
          log.DebugFormat ("MatchMachine: " +
                           "the machine {0} matches the exclude machine filter {1}",
                           machine,
                           ExcludeMachineFilter);
          return false;
        }
      }
      
      return true;
    }
  }
}
