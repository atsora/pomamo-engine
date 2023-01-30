// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO used to describe company in which belongs a group of machines.
  /// </summary>
  public class CompanyDTO
  {
    /// <summary>
    /// Id of company
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of company
    /// </summary>
    public string Name { get; set; }
  }
}
