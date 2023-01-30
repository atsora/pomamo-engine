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
    /// Delayed state actions
    /// </summary>
    IEnumerable<IStateAction> DelayedStateActions { get; }

    /// <summary>
    /// Delayed reason actions
    /// </summary>
    IEnumerable<IReasonAction> DelayedReasonActions { get; }

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
        try {
          using (var transaction = session.BeginTransaction ("AutoReason." + pluginName + ".ApplyDelayedActions", TransactionLevel.ReadCommitted)) {
            foreach (var delayedStateAction in extension.DelayedStateActions) {
              extension.SetActive ();
              if (log.IsDebugEnabled) {
                log.Debug ($"ProcessPendingActions: plugin {pluginName}, apply delayed state action {delayedStateAction.Name}");
              }
              delayedStateAction.Run ();
            } // Loop on state actions
            foreach (var delayedReasonAction in extension.DelayedReasonActions) {
              extension.SetActive ();
              if (log.IsDebugEnabled) {
                log.Debug ($"ProcessPendingActions: plugin {pluginName}, apply delayed reason action {delayedReasonAction.Name}");
              }
              delayedReasonAction.Run ();
            } // Loop on reason actions
            transaction.Commit ();
          } // Transaction
        }
        catch (Exception ex) {
          log.Error ("ProcessPendingActions: exception, retry later", ex);
          try {
            if (extension.DelayedReasonActions.Any ()) { // at least one reason action => reset the states, not to skip any reason
              foreach (var delayedStateAction in extension.DelayedStateActions) {
                delayedStateAction.Reset ();
              }
            }
          }
          catch (Exception ex1) {
            log.Fatal ("ProcessPendingActions: problem in IStateAction.Reset", ex1);
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
