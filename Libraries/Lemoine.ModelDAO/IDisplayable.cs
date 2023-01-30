// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model interface for a displayable item
  /// 
  /// A IDisplayable has a Display property that can be used to display
  /// the item in an interface
  /// </summary>
  public interface IDisplayable
  {
    /// <summary>
    /// Get the string to display in the applications
    /// </summary>
    string Display { get; }
  }
}
