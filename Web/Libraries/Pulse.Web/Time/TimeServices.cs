// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.Time
{
  /// <summary>
  /// Date/time Services
  /// </summary>
  public class DateTimeServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request CurrentDay
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CurrentDayRequestDTO request)
    {
      return new CurrentDayService ().Get (this.GetCacheClient (),
                                           base.RequestContext,
                                           base.Request,
                                           request);
    }

    /// <summary>
    /// Response to GET request CurrentRange
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CurrentRangeRequestDTO request)
    {
      return new CurrentRangeService ().Get (this.GetCacheClient (),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }

    /// <summary>
    /// Response to GET request CurrentTime
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CurrentTimeRequestDTO request)
    {
      return new CurrentTimeService ().GetSync (request);
    }

    /// <summary>
    /// Response to GET request DayToDayTimeRange
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (DayToDateTimeRangeRequestDTO request)
    {
      return new DayToDateTimeRangeService ().Get (this.GetCacheClient (),
                                                   base.RequestContext,
                                                   base.Request,
                                                   request);
    }


    /// <summary>
    /// Response to GET request PastRange
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (PastRangeRequestDTO request)
    {
      return new PastRangeService ().Get (this.GetCacheClient (),
                                          base.RequestContext,
                                          base.Request,
                                          request);
    }

    /// <summary>
    /// Response to GET request RangeAround
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (RangeAroundRequestDTO request)
    {
      return new RangeAroundService ().Get (this.GetCacheClient (),
                                           base.RequestContext,
                                           base.Request,
                                           request);
    }
  }
}
#endif // NSERVICEKIT
