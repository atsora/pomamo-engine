// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// DTO used to describe machinecell in which belongs a group of machines.
  /// </summary>
  public class MachineCellDTO
  {
    /// <summary>
    /// Id of machinecell
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machinecell
    /// </summary>
    public string Name { get; set; }
  }
}
