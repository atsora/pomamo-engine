// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Alert;
using Lemoine.Core.Log;
using Lemoine.Extensions.Alert;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.Group;
using System.Runtime.InteropServices;
using Lemoine.Business;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;
using Lemoine.Extensions;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// Listener that query WebService every x seconds
  /// </summary>
  [Serializable]
  public class ReserveCapacityListener : IListener
  {
    #region Members
    static readonly string GROUP_LIST_KEY = "ReserveCapacityAlert.GroupList";

    DateTime m_lastExecution = DateTime.UtcNow;
    TimeSpan m_frequency = TimeSpan.FromSeconds (2);
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReserveCapacityListener).FullName);

    #region Getters / Setters
    readonly IList<ReserveCapacityGroupListener> m_groupListeners = new List<ReserveCapacityGroupListener> ();
    int m_groupListenersIndex = 0;
    //readonly IList<ReserveCapacityGroupListener> m_machineListeners = new List<ReserveCapacityGroupListener> ();

    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReserveCapacityListener ()
    {
      string groupNameListConfig = Lemoine.Info.ConfigSet.LoadAndGet<string> (GROUP_LIST_KEY, null);

      if (null != groupNameListConfig) {
        log.Debug ($"ReserveCapacityListener: groupList={groupNameListConfig}");
        var groupList = Lemoine.Collections.EnumerableString.ParseListString (groupNameListConfig);
        // create a listener for each group in list
        foreach (string groupName in groupList) {
          log.Debug ($"ReserveCapacityListener: group name={groupName}");
          IGroup group = new Lemoine.Business.Machine.GroupFromId (groupName).Get ();
          if (null != group) {
            log.Debug ($"ReserveCapacityListener: found group={group.Id}");
            m_groupListeners.Add (new ReserveCapacityGroupListener (group));
          }
          else {
            log.Error ($"ReserveCapacityListener: group {groupName} not found");
          }
        }
      }
      else {
        log.Error ($"ReserveCapacityListener: group list is null");
      }

      //Initialize the index
      m_groupListenersIndex = 0;
    }

    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get in the listener the next data.
    /// Returns null when there is no data anymore to return
    /// </summary>
    /// <returns>new data or null</returns>
    /// 
    
    public XmlElement GetData ()
    {
      if (m_groupListeners.Count == 0) {
        return null;
      }

      Debug.Assert (m_groupListenersIndex < m_groupListeners.Count);
      ReserveCapacityGroupListener groupListener = m_groupListeners[m_groupListenersIndex];
      Debug.Assert (groupListener != null);

      var xml = groupListener.GetData ();
      if (xml != null) {
        return xml;
      }

      // Go to the next  group listener
      ++m_groupListenersIndex;
      if (m_groupListenersIndex == m_groupListeners.Count) {
        // Begin again from start later
        m_groupListenersIndex = 0;
        return null;
      }

      // Go to the next index
      return GetData ();
    }

    #endregion // Methods
  }
}
