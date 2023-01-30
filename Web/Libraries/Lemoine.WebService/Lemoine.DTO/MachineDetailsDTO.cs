// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO used to describe machine which details information.
  /// </summary>
  public class MachineDetailsDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machine
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// display priority order of machine
    /// </summary>
    public int? DisplayPriority { get; set; }

    /// <summary>
    /// Id of company
    /// </summary>
    public int? CompanyId { get; set; }
    
    /// <summary>
    /// Id of department
    /// </summary>
    public int? DepartmentId { get; set; }

    /// <summary>
    /// Id of machinecategory
    /// </summary>
    public int? MachineCategoryId { get; set; }

    /// <summary>
    /// Id of machinecategory
    /// </summary>
    public int? SubCategoryId { get; set; }
    
    /// <summary>
    /// Id of cell
    /// </summary>
    public int? CellId { get; set; }
  }
}
