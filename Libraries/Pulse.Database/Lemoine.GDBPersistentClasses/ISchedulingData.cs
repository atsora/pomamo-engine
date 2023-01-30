// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Type of data that can be scheduled
  /// (currently Process and Component)
  /// </summary>
  public interface ISchedulingData
  {
    /// <summary>
    /// Estimated hours
    /// </summary>
    double EstimatedHours {
      get;
      set;
    }
    
  }
}
