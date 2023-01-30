// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Cell to add/remove to a Machine Filter
  /// </summary>
  public interface IMachineFilterCell: IMachineFilterItem
  {
    /// <summary>
    /// Associated cell
    /// </summary>
    ICell Cell { get; }
  }
}
