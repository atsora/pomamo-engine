// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Plugin.ActivityIsProduction
{
  /// <summary>
  /// FactExtension
  /// </summary>
  static internal class FactExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (FactExtension).FullName);

    /// <summary>
    /// Does a fact corresponds to an activity
    /// </summary>
    /// <param name="fact"></param>
    /// <returns></returns>
    internal static bool IsActivity (this IFact fact, Configuration configuration)
    {
      var machineMode = fact.CncMachineMode;

      return IsActivityWithIncludeExclude (IsActivityFromRunningAuto (machineMode, configuration), machineMode, configuration);
    }

    static bool IsActivityFromRunningAuto (IMachineMode machineMode, Configuration configuration)
    {
      var running = machineMode.Running;
      if (!running.HasValue || !running.Value) {
        return false; // Only active
      }

      var auto = machineMode.Auto;
      if (auto.HasValue) {
        if (auto.Value) {
          return true; // Auto (and active)
        }
        else { // Manual
          return configuration.Manual;
        }
      }
      else { // !auto.HasValue
        var manual = machineMode.Manual;
        if (manual.HasValue) {
          if (manual.Value) {
            return configuration.Manual;
          }
          else { // Not manual and auto unknown ?
            log.ErrorFormat ("Match: machine mode {0} is not manual and auto is unknown",
              machineMode);
            return configuration.NorManualNorAuto;
          }
        }
        else { // !manual.HasValue
          return configuration.NorManualNorAuto;
        }
      }
    }

    static bool IsActivityWithIncludeExclude (bool result, IMachineMode machineMode, Configuration configuration)
    {
      if (result) {
        return !IsExcluded (machineMode, configuration);
      }
      else {
        return IsIncluded (machineMode, configuration);
      }
    }

    static bool IsIncluded (IMachineMode machineMode, Configuration configuration)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != configuration);

      if (null == configuration.IncludeMachineModeIds) {
        return false;
      }
      else {
        return configuration.IncludeMachineModeIds
          .Any (x => x == machineMode.Id);
      }
    }

    static bool IsExcluded (IMachineMode machineMode, Configuration configuration)
    {
      Debug.Assert (null != machineMode);
      Debug.Assert (null != configuration);

      if (null == configuration.ExcludeMachineModeIds) {
        return false;
      }
      else {
        return configuration.ExcludeMachineModeIds
          .Any (x => x == machineMode.Id);
      }
    }

    /// <summary>
    /// Does a fact corresponds to a short period
    /// </summary>
    internal static bool IsShort (this IFact fact, IEnumerable<IMachineMode> machineModes, TimeSpan maximumDuration)
    {
      if (!machineModes.Any ()) {
        return false;
      }

      if (!fact.DateTimeRange.IsEmpty () && (maximumDuration <= fact.DateTimeRange.Duration.Value)) {
        return false;
      }

      return machineModes.Any (m => IsSubMachineMode (m, fact.CncMachineMode));
    }

    static bool IsSubMachineMode (IMachineMode ancestor, IMachineMode descendant)
    {
      return Lemoine.Business.ServiceProvider.Get (new Lemoine.Business.MachineMode.IsDescendantOrSelfOf (ancestor, descendant));
    }
  }
}
