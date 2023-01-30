// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Collections;

namespace Lemoine.Cnc.Data
{
  /// <summary>
  /// Description of ICncDataQueue.
  /// </summary>
  public interface ICncDataQueue
    : IExtendedQueue<ExchangeData>
  {
    /// <summary>
    /// Reference to the machine Id
    /// </summary>
    int MachineId { get; set; }
    
    /// <summary>
    /// Reference to the machine module Id
    /// </summary>
    int MachineModuleId { get; set; }
  }
}
