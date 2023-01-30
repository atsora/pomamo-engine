// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IWorkOrderMachineStamp.
  /// </summary>
  public interface IWorkOrderMachineStamp : IMachineStamp
  {
    /// <summary>
    /// Reference to the WorkOrder
    /// </summary>
    IWorkOrder WorkOrder { get; set; }
  }
}
