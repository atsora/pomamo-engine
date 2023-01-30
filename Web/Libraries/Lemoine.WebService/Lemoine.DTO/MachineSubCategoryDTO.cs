// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO used to describe machine SUB-category in which belongs a machines.
  /// </summary>
  public class MachineSubCategoryDTO
  {
    /// <summary>
    /// Id of machine sub-category
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machine sub-category
    /// </summary>
    public string Name { get; set; }
  }
}
