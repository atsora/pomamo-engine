// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Analysis
{
  /// <summary>
  /// SwitchCatchUpMode
  /// </summary>
  public static class SwitchCatchUpMode
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SwitchCatchUpMode).FullName);

    static readonly string GLOBAL_KEY_SUFFIX = "g";

    public static string GetGlobalApplicationStateKey ()
    {
      return $"Analysis.CatchUp.{GLOBAL_KEY_SUFFIX}";
    }

    /// <summary>
    /// Get the corresponding application state
    /// </summary>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public static string GetApplicationStateKey (IMachine machine)
    {
      Debug.Assert (null != machine);

      return $"Analysis.CatchUp.{machine.Id}";
    }

    /// <summary>
    /// Turn on the catch-up mode
    /// </summary>
    public static void TurnOn ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ();
        var keySuffixes = machines
          .Select (x => x.Id.ToString ())
          .Append (GLOBAL_KEY_SUFFIX);
        foreach (var keySuffix in keySuffixes) {
          var key = $"Analysis.CatchUp.{keySuffix}";
          var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (key);
          if (applicationState is null) {
            using (var transaction = session.BeginTransaction ("TurnOnCatchUpMode")) {
              applicationState = ModelDAOHelper.ModelFactory.CreateApplicationState (key);
              applicationState.Value = true;
              ModelDAOHelper.DAOFactory.ApplicationStateDAO
                .MakePersistent (applicationState);
              transaction.Commit ();
            }
          }
        }
      }
    }

    /// <summary>
    /// Turn off the catch-up mode
    /// </summary>
    public static void TurnOff ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
          .FindAll ();
        var keySuffixes = machines
          .Select (x => x.Id.ToString ())
          .Append (GLOBAL_KEY_SUFFIX);
        foreach (var keySuffix in keySuffixes) {
          var key = $"Analysis.CatchUp.{keySuffix}";
          var applicationState = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (key);
          if (null != applicationState) {
            using (var transaction = session.BeginTransaction ("TurnOffCatchUpMode")) {
              ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .MakeTransient (applicationState);
              transaction.Commit ();
            }
          }
        }
      }
    }
  }
}
