// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorAlarmFocus
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class ItemFocus : GenericItem, IConfigurator
  {
    internal const string SEVERITY = "severity";
    internal const string FOCUS = "focus";
    internal const string CURRENT_CNC = "current_cnc";
    internal const string CURRENT_SEVERITY = "current_severity";
    internal const string CURRENT_PATTERN = "current_pattern";
    internal const string SEVERITY_NAME = "severity_name";
    internal const string SEVERITY_DESCRIPTION = "severity_description";
    internal const string SEVERITY_STOP_STATUS = "severity_stop_status";
    internal const string PATTERN_ACQUISITION_INFO = "pattern_acquisition_info";
    internal const string PATTERN_TYPE = "pattern_type";
    internal const string PATTERN_NUMBER = "pattern_number";
    internal const string PATTERN_MESSAGE = "pattern_message";
    internal const string PATTERN_PROPERTIES = "pattern_properties";

    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Alarm focus"; } }

    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description
    {
      get {
        return "Each CNC provides several severities to classify its alarms. " +
          "This item defines which severity is important to track and which severity can be ignored. " +
          "You can also edit severities and change the way alarms are associated to them.";
      }
    }

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords
    {
      get {
        return new String[] { "alarm", "alarms", "cnc", "reports",
          "warnings", "errors", "severities", "severity", "focus" };
      }
    }

    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "configurator"; } }

    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }

    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Alarms"; } }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types
    {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType> ();
        dic[typeof (ICncAlarmSeverity)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof (ICncAlarmSeverityPattern)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof (ICncAlarm)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.NONE;

    /// <summary>
    /// All pages provided by the configurator
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IConfiguratorPage> Pages
    {
      get {
        IList<IConfiguratorPage> pages = new List<IConfiguratorPage> ();
        pages.Add (new Page1 ());
        pages.Add (new Page2 ());
        pages.Add (new PageSeverityDescription ());
        pages.Add (new PagePattern ());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Configurator methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize (ItemData otherData)
    {
      var data = new ItemData ();

      // Common data
      data.CurrentPageName = "";
      data.InitValue (SEVERITY, typeof (ICncAlarmSeverity), null, true);
      data.InitValue (FOCUS, typeof (bool?), null, true);
      data.InitValue (CURRENT_CNC, typeof (string), "", true);
      data.InitValue (CURRENT_SEVERITY, typeof (ICncAlarmSeverity), null, true);
      data.InitValue (CURRENT_PATTERN, typeof (ICncAlarmSeverityPattern), null, true);

      // Specific data for PageSeverityDescription
      data.CurrentPageName = "PageSeverityDescription";
      data.InitValue (SEVERITY_NAME, typeof (string), "", true);
      data.InitValue (SEVERITY_DESCRIPTION, typeof (string), "", true);
      data.InitValue (SEVERITY_STOP_STATUS, typeof (CncAlarmStopStatus), CncAlarmStopStatus.Unknown, true);

      // Specific data for PagePattern
      data.CurrentPageName = "PagePattern";
      data.InitValue (PATTERN_ACQUISITION_INFO, typeof (string), "", true);
      data.InitValue (PATTERN_TYPE, typeof (string), "", true);
      data.InitValue (PATTERN_NUMBER, typeof (string), "", true);
      data.InitValue (PATTERN_MESSAGE, typeof (string), "", true);
      data.InitValue (PATTERN_PROPERTIES, typeof (IDictionary<string, string>), null, true);

      return data;
    }
    #endregion // Configurator methods
  }
}
