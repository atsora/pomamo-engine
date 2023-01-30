// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Structure that includes both the running time and the total time
  /// </summary>
  public struct RunTotalTime
  {
    /// <summary>
    /// Motion time
    /// </summary>
    public TimeSpan Run;
    /// <summary>
    /// Not running time
    /// </summary>
    public TimeSpan NotRunning;
    /// <summary>
    /// Total time
    /// </summary>
    public TimeSpan Total;
  }

  /// <summary>
  /// DAO for IMachineActivitySummary.
  /// </summary>
  public interface IMachineActivitySummaryDAO : IGenericByMachineUpdateDAO<IMachineActivitySummary, int>
  {
    /// <summary>
    /// Get the run and total time for a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="beginDay"></param>
    /// <param name="endDay"></param>
    /// <returns></returns>
    RunTotalTime? GetRunTotalTime (IMachine machine, DateTime beginDay, DateTime endDay);

    /// <summary>
    /// Get the run and total time for a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dayRange"></param>
    /// <returns></returns>
    RunTotalTime? GetRunTotalTime (IMachine machine, DayRange dayRange);

    /// <summary>
    /// Get the run time for a specified day range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="beginDay"></param>
    /// <param name="endDay"></param>
    /// <returns></returns>
    TimeSpan? GetRunTime (IMachine machine, DateTime beginDay, DateTime endDay);

    /// <summary>
    /// Find the machine activity summaries in a day range
    /// </summary>
    /// <param name="machine">not null</param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IMachineActivitySummary> FindInDayRange (IMachine machine,
                                                          DayRange range);

    /// <summary>
    /// Find the machine activity summaries in a day range
    /// with an early fetch of the machine mode
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IMachineActivitySummary> FindInDayRangeWithMachineMode (IMachine machine,
                                                                  DayRange range);
  }
}
