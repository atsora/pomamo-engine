// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Machine category to add/remove to a Machine Filter
  /// </summary>
  public interface IMachineFilterMachineCategory: IMachineFilterItem
  {
    /// <summary>
    /// Associated machine category
    /// </summary>
    IMachineCategory MachineCategory { get; }
  }
}
