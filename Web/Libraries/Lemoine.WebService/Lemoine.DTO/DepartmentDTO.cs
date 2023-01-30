// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO used to describe department in which belongs a group of machines.
  /// </summary>
  public class DepartmentDTO
  {
    /// <summary>
    /// Id of department
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of department
    /// </summary>
    public string Name { get; set; }
  }
}
