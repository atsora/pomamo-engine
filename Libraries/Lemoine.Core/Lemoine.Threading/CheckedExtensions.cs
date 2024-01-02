// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using Lemoine.Core.Log;

namespace Lemoine.Threading
{
  /// <summary>
  /// Extensions methods to the <see cref="IChecked" /> interface
  /// </summary>
  public static class CheckedExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CheckedExtensions).FullName);

    /// <summary>
    /// Sleep a given time span
    /// while setting active the checked thread every 100ms
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <param name="sleepTime"></param>
    /// <param name="stop">nullable. Method to interrupt the stop</param>
    /// <param name="checkFrequency">Check frequency for SetActive and Stop check. Default: 1s</param>
    /// <returns>sleepTime was reached, else interrupted by the cancellation token or the stop function</returns>
    public static bool Sleep (this IChecked checkedThread, TimeSpan sleepTime, Func<bool> stop = null, TimeSpan? checkFrequency = null)
    {
      return Sleep (checkedThread, sleepTime, CancellationToken.None, stop, checkFrequency);
    }

    /// <summary>
    /// Sleep a given time span
    /// while setting active the checked thread every 100ms
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <param name="sleepTime"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stop">nullable. Method to interrupt the stop</param>
    /// <param name="checkFrequency">Check frequency for SetActive and Stop check. Default: 1s</param>
    /// <returns>sleepTime was reached, else interrupted by the cancellation token or the stop function</returns>
    public static bool Sleep (this IChecked checkedThread, TimeSpan sleepTime, CancellationToken cancellationToken, Func<bool> stop = null, TimeSpan? checkFrequency = null)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Sleep: sleep {sleepTime}");
      }
      return SleepUntil (checkedThread, DateTime.UtcNow.Add (sleepTime), cancellationToken, stop, checkFrequency);
    }

    /// <summary>
    /// Sleep until a specific time
    /// while setting active the checked thread every 100ms
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <param name="until"></param>
    /// <param name="stop">nullable. Method to interrupt the sleep</param>
    /// <param name="checkFrequency">Check frequency for SetActive and Stop check. Default: 1s</param>
    /// <returns>until time was reached, else interrupted by the cancellation token or the stop function</returns>
    public static bool SleepUntil (this IChecked checkedThread, DateTime until, Func<bool> stop = null, TimeSpan? checkFrequency = null)
    {
      return SleepUntil (checkedThread, until, CancellationToken.None, stop, checkFrequency);
    }

    /// <summary>
    /// Sleep until a specific time
    /// while setting active the checked thread every 100ms
    /// </summary>
    /// <param name="checkedThread"></param>
    /// <param name="until"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="stop">nullable. Method to interrupt the sleep</param>
    /// <param name="checkFrequency">Check frequency for SetActive and Stop check. Default: 1s</param>
    /// <returns>until time was reached, else interrupted by the cancellation token or the stop function</returns>
    public static bool SleepUntil (this IChecked checkedThread, DateTime until, CancellationToken cancellationToken, Func<bool> stop = null, TimeSpan? checkFrequency = null)
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
        checkedThread?.SetActive ();
      }
      if (log.IsDebugEnabled) {
        log.Debug ($"SleepUntil: {until} reached => return true");
      }
      return true;
    }
  }
}
