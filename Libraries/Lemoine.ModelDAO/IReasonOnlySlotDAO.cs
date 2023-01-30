// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReasonOnlySlot.
  /// </summary>
  public interface IReasonOnlySlotDAO
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time not trying to extend the slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    IReasonOnlySlot FindAt (IMachine machine, DateTime dateTime);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    IReasonOnlySlot FindAt (IMachine machine, DateTime dateTime, bool extend);

    /// <summary>
    /// Find the unique slot at the specified UTC date/time trying to extend the slots
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extendLimitRange">Utc date/time limit range to extend the slot</param>
    /// <param name="lowerExtendLimitReached">the lower value of extendLimitRange was reached</param>
    /// <param name="upperExtendLimitReached">the upper value of extendLimitRange was reached</param>
    /// <returns></returns>
    IReasonOnlySlot FindAt (IMachine machine, DateTime dateTime,
                            UtcDateTimeRange extendLimitRange, out bool lowerExtendLimitReached, out bool upperExtendLimitReached);
    
    /// <summary>
    /// Find all the slots that overlap the specified range not trying to extend the slots
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend);

    /// <summary>
    /// Find all the slots that overlap the specified range trying to extend the slots
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extendLimitRange">Utc date/time limit range to extend the slot</param>
    /// <param name="lowerExtendLimitReached">the lower value of extendLimitRange was reached</param>
    /// <param name="upperExtendLimitReached">the upper value of extendLimitRange was reached</param>
    /// <returns></returns>
    IList<IReasonOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range,
                                              UtcDateTimeRange extendLimitRange, out bool lowerExtendLimitReached, out bool upperExtendLimitReached);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// with an early fetch of the reason
    /// trying to extend the slots
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extendLimitRange">Utc date/time limit range to extend the slot</param>
    /// <param name="lowerExtendLimitReached">the lower value of extendLimitRange was reached</param>
    /// <param name="upperExtendLimitReached">the upper value of extendLimitRange was reached</param>
    /// <returns></returns>
    IList<IReasonOnlySlot> FindOverlapsRangeWithReason (IMachine machine, UtcDateTimeRange range,
                                                        UtcDateTimeRange extendLimitRange, out bool lowerExtendLimitReached, out bool upperExtendLimitReached);
  }
}
