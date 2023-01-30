// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml;
using Lemoine.Model;
using Lemoine.ModelDAO;
using System.Diagnostics;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// CncValue listener for a specific field
  /// </summary>
  [Serializable]
  public class CncAlarmStackLightListener : IListener
  {
    #region Members
    readonly IList<CncAlarmStackLightMachineModuleListener> m_machineModuleListeners = new List<CncAlarmStackLightMachineModuleListener>();
    int m_machineModuleListenerIndex = 0;
    #endregion // Members
    
    #region Constructor
    /// <summary>
    /// Default constructor
    /// </summary>
    public CncAlarmStackLightListener()
    {
      // Create a listener for each machine module
      IList<IMachineModule> machineModules;
      IField field;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession()) {
        machineModules = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindAllWithMonitoredMachine();
        field = ModelDAOHelper.DAOFactory.FieldDAO.FindById((int)FieldId.StackLight);
      }
      m_machineModuleListeners.Clear();
      foreach (var machineModule in machineModules) {
        m_machineModuleListeners.Add (new CncAlarmStackLightMachineModuleListener(field, machineModule));
      }

      // Initialize the index
      m_machineModuleListenerIndex = 0;
    }
    #endregion Constructor
    
    #region Methods
    /// <summary>
    /// Get in the listener the next data.
    /// Returns null when there is no data anymore to return
    /// </summary>
    /// <returns>new data or null</returns>
    public XmlElement GetData()
    {
      if (m_machineModuleListeners.Count == 0) {
        return null;
      }

      Debug.Assert(m_machineModuleListenerIndex < m_machineModuleListeners.Count);
      CncAlarmStackLightMachineModuleListener machineModuleListener = m_machineModuleListeners[m_machineModuleListenerIndex];
      Debug.Assert(machineModuleListener != null);

      var xml = machineModuleListener.GetData();
      if (xml != null) {
        return xml;
      }

      // Go to the next machine module listener
      ++m_machineModuleListenerIndex;
      if (m_machineModuleListenerIndex == m_machineModuleListeners.Count) {
        // Begin again from start later
        m_machineModuleListenerIndex = 0;
        return null;
      }
      
      // Go to the next index
      return GetData();
    }
    #endregion // Methods
  }
}
