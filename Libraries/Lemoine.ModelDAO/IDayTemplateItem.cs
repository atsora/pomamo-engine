// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table daytemplateitem
  /// that associates a cut-off time to a week day
  /// </summary>
  public interface IDayTemplateItem: IDataWithVersion, ISerializableModel
  {
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }
    
    /// <summary>
    /// Associated cut-off time (may be negative)
    /// </summary>
    TimeSpan CutOff { get; set; }
    
    /// <summary>
    /// Applicable week days
    /// </summary>
    WeekDay WeekDays { get; set; }
  }
}
