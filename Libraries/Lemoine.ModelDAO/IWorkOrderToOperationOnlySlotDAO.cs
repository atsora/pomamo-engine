// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IWorkOrderToOperationOnlySlot.
  /// </summary>
  public interface IWorkOrderToOperationOnlySlotDAO
  {
    /// <summary>
    /// Find the unique slot at the specified UTC date/time
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    IWorkOrderToOperationOnlySlot FindAt (IMachine machine, DateTime dateTime, bool extend);
    
    /// <summary>
    /// Find all the slots that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    IList<IWorkOrderToOperationOnlySlot> FindOverlapsRange (IMachine machine, UtcDateTimeRange range, bool extend);

    /// <summary>
    /// Find all the slots that overlap the specified range
    /// with an early fetch of the work order, component, operation
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <param name="extend">Try to extend the slots</param>
    /// <returns></returns>
    IList<IWorkOrderToOperationOnlySlot> FindOverlapsRangeWithEagerFetch (IMachine machine, UtcDateTimeRange range, bool extend);
  }
}
