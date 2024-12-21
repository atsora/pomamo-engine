// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineStatus
  /// 
  /// This new table stores the current status of a machine
  /// </summary>
  public interface IMachineStatus : IVersionable
  {
    /// <summary>
    /// Reference to the MonitoredMachine
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; }

    /// <summary>
    /// Reference to the CNC MachineMode
    /// 
    /// Not null
    /// </summary>
    IMachineMode CncMachineMode { get; set; }

    /// <summary>
    /// Reference to the applied MachineMode
    /// 
    /// Not null
    /// </summary>
    IMachineMode MachineMode { get; set; }

    /// <summary>
    /// Refrence to a MachineStateTemplate
    /// 
    /// May be null
    /// </summary>
    IMachineStateTemplate MachineStateTemplate { get; set; }

    /// <summary>
    /// Reference to the MachineObservationState
    /// 
    /// Not null
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }

    /// <summary>
    /// Reference to the Shift
    /// 
    /// Nullable
    /// </summary>
    IShift Shift { get; set; }

    /// <summary>
    /// Reference to the reason
    /// 
    /// Not null
    /// </summary>
    IReason Reason { get; set; }

    /// <summary>
    /// Reason details
    /// </summary>
    string ReasonDetails { get; set; }

    /// <summary>
    /// Reason data in Json format
    /// </summary>
    string JsonData { get; set; }

    /// <summary>
    /// Reason score
    /// </summary>
    double ReasonScore { get; set; }

    /// <summary>
    /// Reason source
    /// </summary>
    ReasonSource ReasonSource { get; set; }

    /// <summary>
    /// Number of auto reasons
    /// </summary>
    int AutoReasonNumber { get; set; }

    /// <summary>
    /// Overwrite required for the reason ?
    /// </summary>
    bool OverwriteRequired { get; set; }

    /// <summary>
    /// UTC end date/time of the corresponding ReasonSlot
    /// </summary>
    DateTime ReasonSlotEnd { get; set; }

    /// <summary>
    /// UTC end date/time of the corresponding ReasonMachineAssociation if applicable
    /// </summary>
    UpperBound<DateTime> ConsolidationLimit { get; set; }

    /// <summary>
    /// Does the machine mode correspond to a manual activity ?
    /// </summary>
    bool ManualActivity { get; set; }

    /// <summary>
    /// UTC end date/time of the corresponding ActivityManual if applicable
    /// </summary>
    Nullable<DateTime> ManualActivityEnd { get; set; }
  }
}
