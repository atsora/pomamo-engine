// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of ISerialNumberMachineStamp.
  /// </summary>
  public interface ISerialNumberMachineStamp: IMachineStamp
  {
    /// <summary>
    /// Reference to the SerialNumber
    /// </summary>
    string SerialNumber { get; set; }
  }
}