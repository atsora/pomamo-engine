// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Methods to wait in a thread
  /// </summary>
  public static class WaitMethods
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WaitMethods).FullName);

    /// <summary>
    /// Sleep a given time span
    /// </summary>
    /// <param name="sleepTime"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stop">nullable. Method to interrupt the stop</param>
    /// <param name="checkFrequency">Check frequency for SetActive and Stop check. Default: 1s</param>
    /// <returns>sleepTime was reached, else interrupted by the cancellation token or the stop function</returns>
    public static bool Sleep (TimeSpan sleepTime, CancellationToken cancellationToken = default, Func<bool> stop = null, TimeSpan? checkFrequency = null)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Sleep: sleep {sleepTime}");
      }
      return SleepUntil (DateTime.UtcNow.Add (sleepTime), cancellationToken, stop, checkFrequency);
    }

    /// <summary>
    /// Sleep until a specific time
    /// </summary>
    /// <param name="until"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stop">nullable. Method to interrupt the sleep</param>
    /// <param name="checkFrequency">Check frequency for SetActive and Stop check. Default: 1s</param>
    /// <returns>until time was reached, else interrupted by the cancellation token or the stop function</returns>
    public static bool SleepUntil (DateTime until, CancellationToken cancellationToken = default, Func<bool> stop = null, TimeSpan? checkFrequency = null)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"SleepUntil: sleep until {until}");
      }
      while (DateTime.UtcNow < until) {
        if (cancellationToken.IsCancellationRequested) {
          if (log.IsDebugEnabled) {
            log.Debug ($"SleepUntil: cancellation requested => return");
          }
          return false;
        }
        if (null != stop) {
          if (stop ()) {
            if (log.IsDebugEnabled) {
              log.Debug ("SleepUntil: stop requested => return");
            }
            return false;
          }
        }
        TimeSpan sleepDuration = until.Subtract (DateTime.UtcNow);
        if (sleepDuration.Ticks <= 0) {
          if (log.IsDebugEnabled) {
            log.Debug ($"SleepUntil: sleep duration {sleepDuration} is negative => return at once");
          }
          return true;
        }
        var maxSleepDuration = checkFrequency ?? TimeSpan.FromSeconds (1);
        if (maxSleepDuration < sleepDuration) {
          cancellationToken.WaitHandle.WaitOne (maxSleepDuration);
        }
        else {
          cancellationToken.WaitHandle.WaitOne (sleepDuration);
        }
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"SleepUntil: {until} reached => return true");
      }
      return true;
    }
  }
}
