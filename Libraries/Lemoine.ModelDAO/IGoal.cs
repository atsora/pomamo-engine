// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Collections;

namespace Lemoine.Model
{
  /// <summary>
  /// Description of IGoal.
  /// </summary>
  public interface IGoal: IDataWithId<int>, IDataWithVersion
  {
    /// <summary>
    /// Type, cannot be null
    /// </summary>
    IGoalType Type { get; }
    
    /// <summary>
    /// Value
    /// </summary>
    double Value { get; set; }
    
    /// <summary>
    /// MachineObservationState, can be null
    /// </summary>
    IMachineObservationState MachineObservationState { get; set; }
    
    /// <summary>
    /// Company, can be null
    /// </summary>
    ICompany Company { get; set; }
    
    /// <summary>
    /// Department, can be null
    /// </summary>
    IDepartment Department { get; set; }
    
    /// <summary>
    /// Machine category, can be null
    /// </summary>
    IMachineCategory Category { get; set; }
    
    /// <summary>
    /// Machine subcategory, can be null
    /// </summary>
    IMachineSubCategory SubCategory { get; set; }
    
    /// <summary>
    /// Cell, can be null
    /// </summary>
    ICell Cell { get; set; }
    
    /// <summary>
    /// Machine, can be null
    /// </summary>
    IMachine Machine { get; set; }
  }
}
