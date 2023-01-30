// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO for GetLastWorkInformationV2
  /// </summary>
  public class MonitoredMachineOperationBarDTO
  {
    /// <summary>
    /// Value of field monitoredmachineoperationbarof a given machine
    /// </summary>
    public Lemoine.Model.OperationBar MonitoredMachineOperationBar { get; set; }
  }
}
