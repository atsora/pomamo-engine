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
  internal class ItemView : GenericItem, IView
  {
    internal const string CURRENT_CNC = "current_cnc";
    internal const string CURRENT_FOCUS = "current_focus";
    internal const string DATE_RANGE = "date_range";

    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title => "View alarms";

    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description => "Display all alarms associated with their corresponding severity and focus state.";

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords
    {
      get {
        return new String[] { "alarm", "alert", "alarms", "alerts", "cnc", "reports",
          "warnings", "errors", "severities", "severity", "focus" };
      }
    }

    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "view"; } }

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
        dic[typeof (ICncAlarm)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof (ICncAlarmSeverityPattern)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// All pages provided by the configurator
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IViewPage> Pages
    {
      get {
        IList<IViewPage> pages = new List<IViewPage> ();
        pages.Add (new PageView ());
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
      data.InitValue (CURRENT_CNC, typeof (string),
                     otherData != null && otherData.IsStored<string> (CURRENT_CNC) ? otherData.Get<string> (CURRENT_CNC) : "", true);
      data.InitValue (CURRENT_FOCUS, typeof (PageView.FocusState), PageView.FocusState.FOCUS_STATE_ALL, true);
      data.InitValue (DATE_RANGE, typeof (UtcDateTimeRange), new UtcDateTimeRange (new LowerBound<DateTime> (null), new UpperBound<DateTime> (null)), true);

      return data;
    }
    #endregion // Configurator methods
  }
}
