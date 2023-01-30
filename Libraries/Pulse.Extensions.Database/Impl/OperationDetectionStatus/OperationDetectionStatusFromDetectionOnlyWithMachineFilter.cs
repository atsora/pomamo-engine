// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Pulse.Extensions.Database.Impl.OperationDetectionStatus
{
  /// <summary>
  /// Get the operation detection status from the machinemoduleanalysisstatus
  /// and machinemoduledetection tables without considering the auto-sequences
  /// 
  /// Configurable version with a machine filter
  /// </summary>
  public class OperationDetectionStatusFromDetectionOnlyWithMachineFilter<TConfiguration>
    : OperationDetectionStatusWithMachineFilter<TConfiguration>
    , IOperationDetectionStatusExtension
    where TConfiguration : Pulse.Extensions.Configuration.IConfigurationWithMachineFilter, IOperationDetectionStatusConfiguration, new()
  {
    ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusFromDetectionOnlyWithMachineFilter<TConfiguration>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationDetectionStatusFromDetectionOnlyWithMachineFilter ()
      : base ((x) => new OperationDetectionStatusFromDetectionOnly (x))
    {
    }
  }
}
