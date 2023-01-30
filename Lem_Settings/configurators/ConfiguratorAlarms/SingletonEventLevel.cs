// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Alert;
using Pulse.Extensions.Database;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of SingletonEventLevel.
  /// </summary>
  public sealed class SingletonEventLevel
  {
    #region Members
    Dictionary<string, List<IEventLevel>> m_levels; // sorted by event type
    IDictionary<string, IList<string>> m_inputItems = new Dictionary<string, IList<string>> ();
    IList<IEventLevel> m_allLevels;
    Dictionary<string, string> m_displayedName;
    public const string DEFAULT_EVENT = "EventLongPeriod";
    #endregion // Members

    #region Constructors
    SingletonEventLevel ()
    {
      m_levels = new Dictionary<string, List<IEventLevel>> ();
      m_allLevels = new List<IEventLevel> ();
      m_displayedName = new Dictionary<string, string> ();

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          // Get all levels
          m_allLevels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindAll ();

          // List of event types and associated levels
          var dataTypeText = new Dictionary<string, string> ();
          var dataTypeLevels = new Dictionary<string, IList<IEventLevel>> ();
          var dataTypeInputItems = new Dictionary<string, IEnumerable<string>> ();
          foreach (var extension in Lemoine.Extensions.ExtensionManager.GetExtensions<IEventExtension> ()) {
            if (!dataTypeText.ContainsKey (extension.Type)) {
              dataTypeText[extension.Type] = extension.TypeText;
            }
          }
          foreach (var extension in Lemoine.Extensions.ExtensionManager.GetExtensions<IConfigEMailExtension> ()) {
            if (!dataTypeText.ContainsKey (extension.DataType)) {
              dataTypeText[extension.DataType] = extension.DataTypeText;
            }
            if (extension.InputType.Equals (ConfigEMailInputType.List)
                && !dataTypeInputItems.ContainsKey (extension.DataType)) {
              dataTypeInputItems[extension.DataType] = extension.InputList;
            }
            /*
            if (extension.InputType.Equals (ConfigEMailInputType.Text)
                && !dataTypeInputItems.ContainsKey (extension.DataType)) {
              dataTypeInputItems[extension.DataType] = extension.InputList;
            }
            */
          }
          if (!dataTypeText.ContainsKey (DEFAULT_EVENT)) {
            dataTypeText[DEFAULT_EVENT] = "Long period";
          }
          if (!dataTypeText.ContainsKey ("CncValue")) {
            dataTypeText["EventCncValue"] = "Cnc value";
          }
          if (!dataTypeText.ContainsKey ("ToolLife")) {
            dataTypeText["EventToolLife"] = "Tool life";
          }
          if (!dataTypeText.ContainsKey ("UnansweredPeriod")) {
            dataTypeText["UnansweredPeriod"] = "Unanswered Period";
          }

          // Add other types that could be in the emailconfig table
          var emailConfigs = ModelDAOHelper.DAOFactory.EmailConfigDAO.FindAll ();
          foreach (var emailConfig in emailConfigs) {
            var dataType = emailConfig.DataType;
            if (!string.IsNullOrEmpty (dataType) && !dataTypeText.ContainsKey (dataType)) {
              dataTypeText[dataType] = dataType;
            }
          }

          if (!dataTypeLevels.ContainsKey (DEFAULT_EVENT)) {
            dataTypeLevels[DEFAULT_EVENT] = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.GetLevels ();
          }
          if (!dataTypeLevels.ContainsKey ("EventCncValue")) {
            dataTypeLevels["EventCncValue"] = ModelDAOHelper.DAOFactory.EventCncValueConfigDAO.GetLevels ();
          }
          if (!dataTypeLevels.ContainsKey ("EventToolLife")) {
            dataTypeLevels["EventToolLife"] = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.GetLevels ();
          }
          if (!dataTypeLevels.ContainsKey ("CncAlarmByMessage")) {
            dataTypeLevels["CncAlarmByMessage"] = ModelDAOHelper.DAOFactory.EventCncValueConfigDAO.GetLevels ();  // FRC
          }
          if (!dataTypeLevels.ContainsKey ("CncAlarmByNumber")) {
            dataTypeLevels["CncAlarmByNumber"] = ModelDAOHelper.DAOFactory.EventCncValueConfigDAO.GetLevels ();  // FRC
          }
          if (!dataTypeLevels.ContainsKey ("ReserveCapacityInfo")) {
            dataTypeLevels["ReserveCapacityInfo"] = ModelDAOHelper.DAOFactory.EventCncValueConfigDAO.GetLevels ();  // FRC
          }

          foreach (var eventTypeKey in dataTypeText.Keys) {
            string eventTypeLabel = dataTypeText[eventTypeKey];
            IList<IEventLevel> levels;
            dataTypeLevels.TryGetValue (eventTypeKey, out levels);
            IEnumerable<string> inputItems;
            dataTypeInputItems.TryGetValue (eventTypeKey, out inputItems);
            AddLevels (eventTypeKey, eventTypeLabel, levels, inputItems);
          }
        }
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get all data types possible (Event long period, cnc value...)
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetDataTypes ()
    {
      return Instance.m_displayedName.Keys;
    }

    /// <summary>
    /// Get all levels possible within a data type
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static IList<IEventLevel> GetLevels (string dataType)
    {
      return !string.IsNullOrEmpty (dataType) && Instance.m_levels.ContainsKey (dataType) &&
        Instance.m_levels[dataType] != null && Instance.m_levels[dataType].Count > 0 ?
        Instance.m_levels[dataType] : Instance.m_allLevels;
    }

    /// <summary>
    /// Get all possible input items
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static IList<string> GetInputItems (string dataType)
    {
      return !string.IsNullOrEmpty (dataType) && Instance.m_inputItems.ContainsKey (dataType) &&
        Instance.m_inputItems[dataType] != null && Instance.m_inputItems[dataType].Count > 0 ?
        Instance.m_inputItems[dataType] : new List<string> ();
    }

    /// <summary>
    /// Displayed string of a data type for use in the GUI
    /// </summary>
    /// <param name="dataType"></param>
    /// <returns></returns>
    public static string GetDisplayedName (string dataType)
    {
      return Instance.m_displayedName.ContainsKey (dataType) ?
        Instance.m_displayedName[dataType] : dataType;
    }

    void AddLevels (string key, string displayedName, IList<IEventLevel> levels, IEnumerable<string> inputItems)
    {
      m_displayedName[key] = displayedName;

      m_levels[key] = new List<IEventLevel> ();
      if (levels != null) {
        foreach (IEventLevel level in levels) {
          m_levels[key].Add (level);
        }
      }

      m_inputItems[key] = new List<string> ();
      if (null != inputItems) {
        foreach (string inputItem in inputItems) {
          m_inputItems[key].Add (inputItem);
        }
      }
    }
    #endregion // Methods

    #region Instance
    static SingletonEventLevel Instance { get { return Nested.instance; } }
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested () { }
      internal static readonly SingletonEventLevel instance = new SingletonEventLevel ();
    }
    #endregion // Instance
  }
}
