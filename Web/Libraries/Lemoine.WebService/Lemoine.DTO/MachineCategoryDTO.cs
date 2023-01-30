// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO used to describe machinecategory in which belongs a group of machines.
  /// </summary>
  public class MachineCategoryDTO
  {
    /// <summary>
    /// Id of machinecategory
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machinecategory
    /// </summary>
    public string Name { get; set; }
  }
}
