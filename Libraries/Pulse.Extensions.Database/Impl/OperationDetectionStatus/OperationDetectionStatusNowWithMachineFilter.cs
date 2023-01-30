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
  /// Return now for  the operation detection status
  /// because the operation never changes
  /// 
  /// Configurable version with a machine filter
  /// </summary>
  public class OperationDetectionStatusNowWithMachineFilter<TConfiguration>
    : OperationDetectionStatusWithMachineFilter<TConfiguration>
    , IOperationDetectionStatusExtension
    where TConfiguration : Pulse.Extensions.Configuration.IConfigurationWithMachineFilter, IOperationDetectionStatusConfiguration, new ()
  {
    ILog log = LogManager.GetLogger (typeof (OperationDetectionStatusNowWithMachineFilter<TConfiguration>).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationDetectionStatusNowWithMachineFilter ()
      : base ((x) => new OperationDetectionStatusNow (x))
    {
    }
  }
}
