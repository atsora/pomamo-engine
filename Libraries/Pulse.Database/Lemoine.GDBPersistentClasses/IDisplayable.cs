// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Check if an object has a Display property
  /// </summary>
  public interface IDisplayable
  {
    /// <summary>
    /// Display property
    /// </summary>
    string Display { get; }
  }
}
