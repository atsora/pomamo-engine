// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Response DTO returned by service GetMachineTree.
  /// </summary>
  public class MachineTreeDTO
  {
    /// <summary>
    /// 
    /// </summary>
    public List<CompanyDTO> Companies { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<DepartmentDTO> Departments { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<MachineCategoryDTO> MachineCategories { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<MachineDetailsDTO> MachineDetails { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public List<MachineCellDTO> Cells { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<MachineSubCategoryDTO> SubCategories { get; set; }
    
  }
}
