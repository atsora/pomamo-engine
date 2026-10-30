// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Machine modes whose time is counted by <see cref="MachiningDuration"/>,
  /// <see cref="MachiningDurationSince"/> and <see cref="MachiningDurationReached"/>
  /// </summary>
  public enum MachineModeFilter
  {
    /// <summary>
    /// The machine modes the machine is considered running in: the machining time
    /// </summary>
    Running = 0,
    /// <summary>
    /// The machine modes the machine is powered on in: neither off, nor unknown
    /// (no data, unavailable, acquisition error)
    /// </summary>
    PowerOn = 1,
    /// <summary>
    /// The machine modes the machine is running automatically in: the operating time
    /// </summary>
    AutoRunning = 2,
  }

  /// <summary>
  /// Extension methods of <see cref="MachineModeFilter"/>
  /// </summary>
  public static class MachineModeFilterExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachineModeFilterExtensions).FullName);

    /// <summary>
    /// Maximum depth of the machine mode tree, to protect against a loop in the parents
    /// </summary>
    static readonly int MAX_DEPTH = 32;

    /// <summary>
    /// Is the time of a machine mode counted by a filter?
    ///
    /// To be run inside a session: the parents of the machine mode may be read
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="machineMode">null: not counted</param>
    /// <returns></returns>
    public static bool Matches (this MachineModeFilter filter, IMachineMode machineMode)
    {
      if (machineMode is null) {
        return false;
      }
      switch (filter) {
        case MachineModeFilter.Running:
          return machineMode.Running.HasValue && machineMode.Running.Value;
        case MachineModeFilter.PowerOn:
          return (machineMode.MachineModeCategory != MachineModeCategoryId.Unknown)
            && !IsDescendantOrSelfOf (machineMode, (int)MachineModeId.Off);
        case MachineModeFilter.AutoRunning:
          return machineMode.Running.HasValue && machineMode.Running.Value
            && IsAuto (machineMode);
        default:
          log.Fatal ($"Matches: unknown filter {filter}");
          throw new ArgumentException ($"Unknown machine mode filter {filter}", nameof (filter));
      }
    }

    /// <summary>
    /// Suffix of the cache keys of the requests that take a filter: empty for
    /// <see cref="MachineModeFilter.Running"/>, so that the keys of the machining time
    /// do not change
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static string GetCacheKeySuffix (this MachineModeFilter filter) =>
      (filter == MachineModeFilter.Running) ? "" : $".{filter}";

    /// <summary>
    /// Is a machine mode, or one of its parents, the machine mode of an id?
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <param name="ancestorId"></param>
    /// <returns></returns>
    static bool IsDescendantOrSelfOf (IMachineMode machineMode, int ancestorId)
    {
      var current = machineMode;
      for (var i = 0; (null != current) && (i < MAX_DEPTH); ++i) {
        if (current.Id == ancestorId) {
          return true;
        }
        current = current.Parent;
      }
      return false;
    }

    /// <summary>
    /// Is a machine mode an automatic one? The first Auto property that is set, from the
    /// machine mode up to its parents, says it
    /// </summary>
    /// <param name="machineMode">not null</param>
    /// <returns></returns>
    static bool IsAuto (IMachineMode machineMode)
    {
      var current = machineMode;
      for (var i = 0; (null != current) && (i < MAX_DEPTH); ++i) {
        if (current.Auto.HasValue) {
          return current.Auto.Value;
        }
        current = current.Parent;
      }
      return false;
    }
  }
}
