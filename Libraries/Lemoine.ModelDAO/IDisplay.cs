// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Display
  /// </summary>
  public interface IDisplay: IVersionable
  {
    /// <summary>
    /// Table name
    /// </summary>
    string Table { get; }
    
    /// <summary>
    /// Pattern to use to give a display name to a row of the associated table
    /// </summary>
    string Pattern { get; set; }
    
    /// <summary>
    /// Variant of the display
    /// </summary>
    string Variant { get; set; }
    
    /// <summary>
    /// Description
    /// </summary>
    string Description { get; set; }
  }
}
