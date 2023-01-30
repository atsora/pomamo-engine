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
  /// Persistent class of table MachineStatus
  /// 
  /// This new table stores the current status of a machine
  /// </summary>
  [Serializable]
  public class MachineStatus: IMachineStatus
  {
    #region Members
    int m_id = 0;
    int m_version = 0;
    IMonitoredMachine m_monitoredMachine;
    IMachineMode m_cncMachineMode;
    IMachineMode m_machineMode;
    IMachineStateTemplate m_machineStateTemplate;
    IMachineObservationState m_machineObservationState;
    IShift m_shift;
    IReason m_reason;
    string m_reasonDetails;
    double m_reasonScore = 0.0;
    ReasonSource m_reasonSource = ReasonSource.Default;
    int m_autoReasonNumber = 0;
    bool m_overwriteRequired;
    DateTime m_reasonSlotEnd;
    UpperBound<DateTime> m_consolidationLimit;
    bool m_manualActivity = false;
    DateTime? m_manualActivityEnd;
    #endregion // Members

    ILog log = LogManager.GetLogger(typeof (MachineStatus).FullName);

    #region Constructors
    /// <summary>
    /// Default constructor is forbidden
    /// </summary>
    protected MachineStatus ()
    { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="monitoredMachine"></param>
    public MachineStatus(IMonitoredMachine monitoredMachine)
    {
      this.MonitoredMachine = monitoredMachine;
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// ID of the referenced machine
    /// </summary>
    public virtual int Id {
      get { return m_id; }
    }
    
    /// <summary>
    /// Version
    /// </summary>
    public virtual int Version {
      get { return m_version; }
    }
    
    /// <summary>
    /// Reference to the MonitoredMachine
    /// </summary>
    public virtual IMonitoredMachine MonitoredMachine {
      get { return m_monitoredMachine; }
      protected set
      {
        m_monitoredMachine = value;
        log = LogManager.GetLogger(string.Format ("{0}.{1}",
                                                  this.GetType ().FullName,
                                                  value.Id));
      }
    }
    
    /// <summary>
    /// Reference to the CNC MachineMode
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineMode CncMachineMode {
      get { return m_cncMachineMode; }
      set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("CncMachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_cncMachineMode = value;
      }
    }
    
    /// <summary>
    /// Reference to the applied MachineMode
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineMode MachineMode {
      get { return m_machineMode; }
      set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("MachineMode.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_machineMode = value;
      }
    }
    
    /// <summary>
    /// Reference to a machine state template
    /// 
    /// May be null
    /// </summary>
    public virtual IMachineStateTemplate MachineStateTemplate
    {
      get { return m_machineStateTemplate; }
      set { m_machineStateTemplate = value; }
    }
    
    /// <summary>
    /// Reference to the MachineObservationState
    /// 
    /// Not null
    /// </summary>
    public virtual IMachineObservationState MachineObservationState {
      get { return m_machineObservationState; }
      set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("MachineObservationState.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_machineObservationState = value;
      }
    }
    
    /// <summary>
    /// Reference to the Shift
    /// 
    /// nullable
    /// </summary>
    public virtual IShift Shift {
      get { return m_shift; }
      set { m_shift = value; }
    }
    
    /// <summary>
    /// Reference to the reason
    /// </summary>
    public virtual IReason Reason {
      get { return m_reason; }
      set
      {
        Debug.Assert (null != value);
        if (value == null) {
          log.ErrorFormat ("Reason.set: " +
                           "null value");
          throw new ArgumentNullException ();
        }
        m_reason = value;
      }
    }
    
    /// <summary>
    /// Reason details
    /// </summary>
    public virtual string ReasonDetails {
      get { return m_reasonDetails; }
      set { m_reasonDetails = value; }
    }

    /// <summary>
    /// Reason score
    /// </summary>
    public virtual double ReasonScore
    {
      get { return m_reasonScore; }
      set { m_reasonScore = value; }
    }

    /// <summary>
    /// Number of auto reasons
    /// </summary>
    public virtual int AutoReasonNumber
    {
      get { return m_autoReasonNumber; }
      set { m_autoReasonNumber = value; }
    }

    /// <summary>
    /// Reason source
    /// </summary>
    public virtual ReasonSource ReasonSource
    {
      get { return m_reasonSource; }
      set { m_reasonSource = value; }
    }

    /// <summary>
    /// Should the operator overwrite the reason ?
    /// </summary>
    public virtual bool OverwriteRequired
    {
      get { return m_overwriteRequired; }
      set { m_overwriteRequired = value; }
    }
   
    /// <summary>
    /// UTC end date/time of the corresponding ReasonSlot
    /// </summary>
    public virtual DateTime ReasonSlotEnd {
      get { return m_reasonSlotEnd; }
      set { m_reasonSlotEnd = value; }
    }
    
    /// <summary>
    /// UTC end date/time of the corresponding ReasonMachineAssociation if applicable
    /// </summary>
    public virtual UpperBound<DateTime> ConsolidationLimit {
      get { return m_consolidationLimit; }
      set { m_consolidationLimit = value; }
    }
    
    /// <summary>
    /// Does the machine mode correspond to a manual activity ?
    /// </summary>
    public virtual bool ManualActivity {
      get { return m_manualActivity; }
      set { m_manualActivity = value; }
    }
    
    /// <summary>
    /// UTC end date/time of the corresponding ActivityManual if applicable
    /// </summary>
    public virtual Nullable<DateTime> ManualActivityEnd {
      get { return m_manualActivityEnd; }
      set { m_manualActivityEnd = value; }
    }
    #endregion // Getters / Setters

    /// <summary>
    /// Set the reason of the machine status from the last reason slot
    /// 
    /// Useful in case a default reason is used
    /// </summary>
    protected internal virtual void SetReasonFromLastReasonSlot ()
    {
      // Get the last reason slot (it should be the created reason slot)
      IReasonSlot reasonSlot =
        (new ReasonSlotDAO ()).GetLast (this.MonitoredMachine);

      // Check the got reason slot is coherent
      Debug.Assert (null != reasonSlot);
      /* // assert problem in the tests
      Debug.Assert (reasonSlot.MachineObservationState.Id == this.MachineObservationState.Id); // may be lazy => use the Id
      Debug.Assert (Bound.Equals<DateTime> (reasonSlot.EndDateTime, this.ReasonSlotEnd)); // Not valid for a new machine status
      */
      
      log.DebugFormat ("SetReasonFromLastReasonSlot: " +
                       "got reason {0} from reasonSlot {1}",
                       reasonSlot.Reason, reasonSlot);
      this.Reason = reasonSlot.Reason;
      this.ReasonDetails = reasonSlot.ReasonDetails;
      this.ReasonScore = reasonSlot.ReasonScore;
      this.ReasonSource = reasonSlot.ReasonSource;
      this.AutoReasonNumber = reasonSlot.AutoReasonNumber;
      this.OverwriteRequired = reasonSlot.OverwriteRequired;
      Debug.Assert (reasonSlot.EndDateTime.HasValue);
      this.ReasonSlotEnd = reasonSlot.EndDateTime.Value;
      this.ConsolidationLimit = reasonSlot.EndDateTime.Value;
    }
  }
}
