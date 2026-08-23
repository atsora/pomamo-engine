// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Extensions.AutoReason
{
  /// <summary>
  /// Extension of the auto-reason service that collects some delayed actions
  /// before applying them
  /// 
  /// <see cref="IActionableAutoReason"/> for the auto-reasons
  /// <see cref="IActionableAutoMachineStateTemplate"/> for the auto machine state templates
  /// </summary>
  public interface IActionableAutoExtension
    : Lemoine.Extensions.IExtension
    , Lemoine.Threading.IChecked
  {
    /// <summary>
    /// Delayed actions
    /// </summary>
    IEnumerable<IAutoReasonAction> DelayedActions { get; }

    /// <summary>
    /// Reset the delayed actions
    /// </summary>
    void ResetDelayedActions ();

    /// <summary>
    /// Get a logger
    /// </summary>
    /// <returns></returns>
    ILog GetLogger ();
  }

  /// <summary>
  /// Extensions to <see cref="IActionableAutoExtension"/>
  /// </summary>
  public static class ActionableAutoExtensions
  {
    /// <summary>
    /// Process the pending actions
    /// </summary>
    /// <param name="extension">not null</param>
    public static void ProcessPendingActions (this IActionableAutoExtension extension)
    {
      var log = extension.GetLogger ();

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        foreach (var commitNumberActions in extension.DelayedActions.GroupBy (x => x.CommitNumber).OrderBy (x => x.Key)) {
          if (log.IsDebugEnabled) {
            log.Debug ($"ProcessPendingActions: process commitNumber={commitNumberActions.Key}");
          }
          extension.RunPendingActionsTransaction (commitNumberActions);
        }
      }
    }

    /// <summary>
    /// Process in a same transaction the pending actions of a specific commit number
    /// </summary>
    /// <param name="extension">not null</param>
    /// <param name="commitNumber"></param>
    public static void RunPendingActionsTransaction (this IActionableAutoExtension extension, int commitNumber)
    {
      extension.RunPendingActionsTransaction (extension.DelayedActions.Where (x => x.CommitNumber == commitNumber));
    }

    /// <summary>
    /// Process the specified pending actions in a same transaction
    /// </summary>
    /// <param name="extension">not null</param>
    /// <param name="actions">not null</param>
    public static void RunPendingActionsTransaction (this IActionableAutoExtension extension, IEnumerable<IAutoReasonAction> actions)
    {
      var pluginName = extension.GetPluginName ();
      var log = extension.GetLogger ();

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var stateActions = actions.OfType<IStateAction> ().ToList ();
        var mainActions = actions.Where (x => !(x is IStateAction)).ToList ();
        try {
          using (var transaction = session.BeginTransaction ("AutoReason." + pluginName + ".ApplyDelayedActions", TransactionLevel.ReadCommitted)) {
            // Apply first the state actions then the main actions (reason, machine state template, ...)
            // for performance reasons
            foreach (var delayedStateAction in stateActions) {
              extension.SetActive ();
              if (log.IsDebugEnabled) {
                log.Debug ($"RunPendingActionsTransaction: plugin {pluginName}, apply delayed state action {delayedStateAction.Name}");
              }
              delayedStateAction.Run ();
            } // Loop on state actions
            foreach (var delayedMainAction in mainActions) {
              extension.SetActive ();
              if (log.IsDebugEnabled) {
                log.Debug ($"RunPendingActionsTransaction: plugin {pluginName}, apply delayed action {delayedMainAction.Name}");
              }
              delayedMainAction.Run ();
            } // Loop on main actions
            transaction.Commit ();
          } // Transaction
        }
        catch (Exception ex) {
          log.Error ("RunPendingActionsTransaction: exception, retry later", ex);
          try {
            if (mainActions.Any ()) { // at least one main action => reset the states, not to skip any data
              foreach (var delayedStateAction in stateActions) {
                delayedStateAction.Reset ();
              }
            }
          }
          catch (Exception ex1) {
            log.Fatal ("RunPendingActionsTransaction: problem in IStateAction.Reset", ex1);
            throw;
          }
        }
        finally {
          extension.ResetDelayedActions ();
        }
      }
    }
  }
}
