// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table MachineModuleStatus
  /// 
  /// This new table stores the current status of a machine module
  /// </summary>
  public interface IMonitoredMachineAnalysisStatus: IVersionable
  {
    /// <summary>
    /// Reference to the Monitored Machine
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; set; }

    /// <summary>
    /// Activity analysis date/time
    /// </summary>
    DateTime ActivityAnalysisDateTime { get; set; }
    
    /// <summary>
    /// Activity analysis counter
    /// </summary>
    int ActivityAnalysisCount { get ; set; }
  }
}
