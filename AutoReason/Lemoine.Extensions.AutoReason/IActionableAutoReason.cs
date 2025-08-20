// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// I
  /// </summary>
  public interface IActionableAutoReason: IAutoReasonExtension, Lemoine.Threading.IChecked
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
  /// Extensions to the IActionalAutoReason
  /// </summary>
  public static class ActionalAutoReasonExtensions
  {
    /// <summary>
    /// Process the pending actions
    /// </summary>
    /// <param name="extension"></param>
    public static void ProcessPendingActions (this IActionableAutoReason extension)
    {
      var pluginName = extension.GetPluginName ();
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
    /// Process the pending actions in a same transaction
    /// </summary>
    /// <param name="extension"></param>
    public static void RunPendingActionsTransaction (this IActionableAutoReason extension, int commitNumber)
    {
       extension.RunPendingActionsTransaction (extension.DelayedActions.Where (x => x.CommitNumber == commitNumber));
    }

    /// <summary>
    /// Process the pending actions in a same transaction
    /// </summary>
    /// <param name="extension"></param>
    public static void RunPendingActionsTransaction (this IActionableAutoReason extension, IEnumerable<IAutoReasonAction> actions)
    {
      var pluginName = extension.GetPluginName ();
      var log = extension.GetLogger ();

      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var stateActions = actions.OfType<IStateAction> ().ToList ();
        var reasonActions = actions.OfType<IReasonAction> ().ToList ();
        try {
          using (var transaction = session.BeginTransaction ("AutoReason." + pluginName + ".ApplyDelayedActions", TransactionLevel.ReadCommitted)) {
            // Apply first the state actions then the reason actions for performance reasons
            foreach (var delayedStateAction in stateActions) {
              extension.SetActive ();
              if (log.IsDebugEnabled) {
                log.Debug ($"RunPendingActionsTransaction: plugin {pluginName}, apply delayed state action {delayedStateAction.Name}");
              }
              delayedStateAction.Run ();
            } // Loop on state actions
            foreach (var delayedReasonAction in reasonActions) {
              extension.SetActive ();
              if (log.IsDebugEnabled) {
                log.Debug ($"RunPendingActionsTransaction: plugin {pluginName}, apply delayed reason action {delayedReasonAction.Name}");
              }
              delayedReasonAction.Run ();
            } // Loop on reason actions
            transaction.Commit ();
          } // Transaction
        }
        catch (Exception ex) {
          log.Error ("RunPendingActionsTransaction: exception, retry later", ex);
          try {
            if (reasonActions.Any ()) { // at least one reason action => reset the states, not to skip any reason
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
