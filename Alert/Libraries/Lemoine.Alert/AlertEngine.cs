// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml;

using Lemoine.Core.Log;
using Lemoine.Threading;
using Lemoine.Business.Config;
using System.Threading;
using Lemoine.Extensions.Alert;
using Microsoft.SqlServer.Server;
using System.Linq;

namespace Lemoine.Alert
{
  /// <summary>
  /// Class that checks all the alert data to process and process them if applicable
  /// </summary>
  public class AlertEngine: ThreadClass, IThreadClass, IChecked
  {
    static readonly string SLEEP_TIME_KEY = "Alert.Engine.Sleep";
    static readonly TimeSpan SLEEP_TIME_DEFAULT = TimeSpan.FromSeconds (2);

    #region Members
    readonly ConfigUpdateChecker m_configUpdateChecker = new ConfigUpdateChecker ();
    readonly IEnumerable<IListener> m_listeners;
    readonly IEnumerable<TriggeredAction> m_triggeredActions;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (AlertEngine).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public AlertEngine (IEnumerable<IListener> listeners, IEnumerable<TriggeredAction> triggeredActions)
    {
      m_listeners = listeners;
      m_triggeredActions = triggeredActions;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Main method:
    /// <item>get all the data to process</item>
    /// <item>if applicable, process them</item>
    /// </summary>
    public void RunOnePass (CancellationToken cancellationToken)
    {
      foreach (IListener listener in m_listeners) {
        if (log.IsDebugEnabled) {
          log.Debug ($"RunOnePass: switching to listener {listener.GetType ()}");
        }
        SetActive ();
        if (this.ExitRequested || cancellationToken.IsCancellationRequested) {
          return;
        }
        try {
          while (!this.ExitRequested && !cancellationToken.IsCancellationRequested) {
            SetActive ();
            XmlElement data = listener.GetData ();
            if (null == data) {
              break;
            }
            else { // There is a data
              log.Debug ($"RunOnePass: +++++++++ m_triggeredActions.count={m_triggeredActions.Count()}");
              foreach (TriggeredAction triggeredAction in m_triggeredActions) {
                log.Debug ($"RunOnePass: +++++++++ triggeredAction={triggeredAction}");
                SetActive ();
                if (this.ExitRequested || cancellationToken.IsCancellationRequested) {
                  return;
                }
                try {
                  if (triggeredAction.Trigger.Eval (data)) {
                    foreach (IAction action in triggeredAction.Actions) {
                      SetActive ();
                      log.Debug ($"RunOnePass: +++++++++ action={action}");
                      if (this.ExitRequested || cancellationToken.IsCancellationRequested) {
                        return;
                      }
                      try {
                        action.Execute (data);
                      }
                      catch (Exception actionException) {
                        log.Error ("RunOnePass:action error", actionException);
                      }
                    }
                  }
                }
                catch (Exception triggerException) {
                  log.Error ("RunOnePass: trigger error", triggerException);
                }
              }
            }
          }
        }
        catch (Exception ex) {
          log.Error ("RunOnePass: listener.GetData returned exception => go to the next listener",
            ex);
          continue;
        }
      }

      log.Debug ("RunOnePass: completed");
    }

    #endregion // Methods

    #region IThreadClass implementation
    /// <summary>
    /// Run
    /// </summary>
    protected override void Run (CancellationToken cancellationToken)
    {
      while (!this.ExitRequested && !cancellationToken.IsCancellationRequested) {
        if (!m_configUpdateChecker.Check ()) {
          log.Info ("Run: configuration updated, exit");
          this.SetExitRequested ();
          return;
        }
        SetActive ();

        RunOnePass (cancellationToken);

        if (this.ExitRequested || cancellationToken.IsCancellationRequested) {
          return;
        }
        SetActive ();

        if (log.IsDebugEnabled) {
          log.Debug ("Run: all the data from all listeners have been processed");
        }

        var sleepTime = Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (SLEEP_TIME_KEY, SLEEP_TIME_DEFAULT);
        this.Sleep (sleepTime, cancellationToken, () => this.ExitRequested);
      }
    }

    /// <summary>
    /// Logger
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger ()
    {
      return log;
    }
    #endregion // IThreadClass implementation
  }
}
