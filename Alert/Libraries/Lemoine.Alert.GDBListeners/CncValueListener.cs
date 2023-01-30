// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System.Diagnostics;
using Lemoine.Extensions.Alert;

namespace Lemoine.Alert.GDBListeners
{
  /// <summary>
  /// CncValue listener for a specific field
  /// </summary>
  [Serializable]
  public class CncValueListener : IListener
  {
    #region Members
    IField m_field = null;
    readonly IList<CncValueMachineModuleListener> m_machineModuleListeners = new List<CncValueMachineModuleListener> ();
    int m_machineModuleListenerIndex = 0;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (CncValueListener).FullName);

    #region Getters / Setters
    /// <summary>
    /// Field ID
    /// </summary>
    [XmlAttribute("FieldId")]
    public int FieldId {
      get
      {
        if (null == m_field) {
          return 0;
        }
        else {
          return m_field.Id;
        }
      }
      set
      {
        IEnumerable<IMachineModule> machineModules;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          m_field = ModelDAOHelper.DAOFactory.FieldDAO.FindById (value);
          machineModules = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindAllWithMonitoredMachine ();
        }

        m_machineModuleListeners.Clear ();
        foreach (var machineModule in machineModules) {
          m_machineModuleListeners.Add (new CncValueMachineModuleListener (m_field, machineModule));
        }
        m_machineModuleListenerIndex = 0;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncValueListener ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get in the listener the next data.
    /// 
    /// Returns null when there is no data any more to return
    /// </summary>
    /// <returns>new data or null</returns>
    public XmlElement GetData ()
    {
      if (!m_machineModuleListeners.Any ()) {
        return null;
      }

      Debug.Assert (m_machineModuleListenerIndex < m_machineModuleListeners.Count);
      CncValueMachineModuleListener machineModuleListener = m_machineModuleListeners[m_machineModuleListenerIndex];
      Debug.Assert (null != machineModuleListener);

      var xml = machineModuleListener.GetData ();
      if (null != xml) {
        return xml;
      }
      else { // Go to the next machine module listener
        ++m_machineModuleListenerIndex;
        if (m_machineModuleListenerIndex == m_machineModuleListeners.Count) { // Begin again from start later
          m_machineModuleListenerIndex = 0;
          return null;
        }
        else {
          return GetData ();
        }
      }
    }
    #endregion // Methods
  }
}
