// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

namespace Lemoine.Model
{
  /// <summary>
  /// Persistent class of analysis table ShiftSlot
  /// that keeps a track of all the shift periods
  /// </summary>
  public interface IShiftSlot: ISlotWithDayShift, IComparable<IShiftSlot>, IDataWithVersion
  {
    /// <summary>
    /// Reference to a shift template
    /// </summary>
    IShiftTemplate ShiftTemplate { get; set; }

    /// <summary>
    /// Effective duration of the shift slot considering the breaks
    /// </summary>
    TimeSpan? EffectiveDuration { get; }
    
    /// <summary>
    /// Elapsed effective duration of the shift slot considering the breaks
    /// </summary>
    TimeSpan? ElapsedEffectiveDuration { get; }

    /// <summary>
    /// Set of break periods
    /// </summary>
    ISet<IShiftSlotBreak> Breaks { get; }
    
    /// <summary>
    /// Add a break to the shift
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IShiftSlotBreak AddBreak (UtcDateTimeRange range);
    
    /// <summary>
    /// Was the template processed ?
    /// </summary>
    bool TemplateProcessed { get; set; }
    
    /// <summary>
    /// Process the template when it has not been processed yet
    /// 
    /// applicableRange must overlaps the date/time range of the slot
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="applicableRange"></param>
    /// <returns>true if completed, else false</returns>
    bool ProcessTemplate (CancellationToken cancellationToken, UtcDateTimeRange applicableRange);
    
    /// <summary>
    /// Get the elapsed effective duration at the specified UTC date/time
    /// </summary>
    /// <param name="utcNow"></param>
    /// <returns></returns>
    TimeSpan? GetElapsedEffectiveDuration (DateTime utcNow);
  }
}
