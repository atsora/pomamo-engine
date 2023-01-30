// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Company to add/remove to a Machine Filter
  /// </summary>
  public interface IMachineFilterCompany: IMachineFilterItem
  {
    /// <summary>
    /// Associated company
    /// </summary>
    ICompany Company { get; }
  }
}
