// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table operationslotsplit
  /// </summary>
  public interface IOperationSlotSplit: IVersionable
  {
    /// <summary>
    /// Reference to the Machine
    /// </summary>
    IMachine Machine { get; }

    /// <summary>
    /// Date/time up to which the operation slots are split
    /// </summary>
    DateTime End { get; set; }
  }
}
