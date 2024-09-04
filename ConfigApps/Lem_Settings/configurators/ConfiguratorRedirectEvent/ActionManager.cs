// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace ConfiguratorRedirectEvent
{
  /// <summary>
  /// Description of ActionManager.
  /// </summary>
  public class ActionManager
  {
    #region Members
    readonly IList<Action> m_actions = new List<Action>();
    readonly bool m_advancedAllowed = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ActionManager).FullName);
    static readonly string ALERT_CONFIG_DIRECTORY_KEY = "Alert.ConfigDirectory";
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="advancedAllowed"></param>
    public ActionManager(bool advancedAllowed)
    {
      m_advancedAllowed = advancedAllowed;
      
      // List of all alerts
      FillActions();
    }
    #endregion // Constructors

    #region Methods
    public static bool IsAdminRightRequired()
    {
      string path = Lemoine.Info.ConfigSet.Get<string> (ALERT_CONFIG_DIRECTORY_KEY);
      bool needed = false;
      try {
        // 2 methods from https://stackoverflow.com/questions/1410127/c-sharp-test-if-user-has-write-access-to-a-folder
#if NET45 || NET48
        System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(path);
#endif // NET45 || NET48
        using (FileStream fs = File.Create(Path.Combine(path, Path.GetRandomFileName()),
                                           1, FileOptions.DeleteOnClose)) {}
      } catch (Exception) {
        needed = true;
      }
      return needed;
    }
    
    void FillActions()
    {
      m_actions.Clear();

      // Scan all files
      string path = Lemoine.Info.ConfigSet.Get<string> (ALERT_CONFIG_DIRECTORY_KEY);
      FillActions(new DirectoryInfo(path));
    }
    
    void FillActions(DirectoryInfo dir)
    {
      foreach (var file in dir.GetFiles()) {
        // If a .template.config exists along with a .config, we skip it
        if (file.FullName.EndsWith(".template.config")) {
          string otherFile = file.FullName.Remove(
            file.FullName.Length - ".template.config".Length, ".template".Length);
          if (File.Exists(otherFile)) {
            continue;
          }
        }
        
        var alert = new Action(file);
        if (alert.IsValid && (m_advancedAllowed || !alert.AdvancedMode)) {
          m_actions.Add(alert);
        }
      }
      
      // Recursion
      foreach (var subDir in dir.GetDirectories()) {
        FillActions (subDir);
      }
    }
    
    /// <summary>
    /// Get a list of actions, by event type and action type
    /// </summary>
    /// <param name="eventType">null or empty is all</param>
    /// <param name="actionType">null or empty is all</param>
    /// <returns></returns>
    public IList<Action> GetAlerts(string eventType, string actionType)
    {
      var alerts = new List<Action>();
      
      foreach (var alert in m_actions) {
        if ((string.IsNullOrEmpty(eventType) || alert.EventType == eventType) &&
            (string.IsNullOrEmpty(actionType) || alert.ActionType == actionType)) {
          alerts.Add(alert);
        }
      }
      
      return alerts.OrderBy(o=>o.Title).ToList();
    }
    
    /// <summary>
    /// Get the list of all possible event types
    /// </summary>
    /// <returns></returns>
    public IList<string> GetPossibleEventTypes()
    {
      var eventTypes = new List<string>();
      
      foreach (var alert in m_actions) {
        if (!string.IsNullOrEmpty(alert.EventType) && !eventTypes.Contains(alert.EventType)) {
          eventTypes.Add(alert.EventType);
        }
      }

      eventTypes.Sort();
      
      return eventTypes;
    }
    
    /// <summary>
    /// Get the list of all possible action types
    /// </summary>
    /// <returns></returns>
    public IList<string> GetPossibleActionTypes()
    {
      var actionTypes = new List<string>();
      
      foreach (var alert in m_actions) {
        if (!string.IsNullOrEmpty(alert.ActionType) && !actionTypes.Contains(alert.ActionType)) {
          actionTypes.Add(alert.ActionType);
        }
      }

      actionTypes.Sort();
      
      return actionTypes;
    }
#endregion // Methods
  }
}
