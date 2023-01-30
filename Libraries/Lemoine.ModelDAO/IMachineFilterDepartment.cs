// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Department to add/remove to a Machine Filter
  /// </summary>
  public interface IMachineFilterDepartment: IMachineFilterItem
  {
    /// <summary>
    /// Associated department
    /// </summary>
    IDepartment Department { get; }
  }
}
