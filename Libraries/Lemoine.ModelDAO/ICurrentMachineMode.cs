// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table CurrentMachineMode
  /// </summary>
  public interface ICurrentMachineMode: IDataWithVersion
  {
    /// <summary>
    /// Id
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Associated monitored machine
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; }
    
    /// <summary>
    /// UTC date/time stamp of the data
    /// </summary>
    DateTime DateTime { get; set; }

    /// <summary>
    /// UTC date/time stamp of the data
    /// </summary>
    DateTime Change { get; }
        
    /// <summary>
    /// Reference to the CNC machine mode
    /// 
    /// Not null
    /// </summary>
    IMachineMode MachineMode { get; set; }
  }
}
