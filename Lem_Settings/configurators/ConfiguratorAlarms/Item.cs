// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorAlarms
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IConfigurator
  {
    internal const string FILTER_MACHINES = "filter_machines";
    internal const string FILTER_USERS = "filter_users";
    internal const string FILTER_EVENT_TYPE = "filter_event_type";
    internal const string FILTER_LEVEL = "filter_level";
    internal const string FILTER_ITEM = "filter_item";
    internal const string CURRENT_ALERT = "current_alert";
    internal const string ALARM_MANAGER = "alarm_manager";
    internal const string TREEVIEW_MACHINE_ORDER = "treeview_machine_order";
    internal const string TREEVIEW_MACHINE_LINKED = "treeview_machine_linked";
    internal const string LIST_EMAILS_LINKED = "list_emails_linked";
    internal const string ALERT_NAME = "alert_name";
    internal const string ALERT_IS_ACTIVATED = "alert_is_activated";
    internal const string ALERT_EVENT_TYPE = "alert_event_type";
    internal const string ALERT_EVENT_LEVEL = "alert_event_level";
    internal const string ALERT_FILTER = "alert_filter";
    internal const string ALERT_DAYS_IN_WEEK = "alert_days_in_week";
    internal const string ALERT_START_DATE = "alert_start_date";
    internal const string ALERT_EXPIRATION = "alert_expiration";
    internal const string ALERT_PERIOD = "alert_period";
    internal const string ALERT_MACHINES = "alert_machines";
    internal const string ALERT_ALL_MACHINES = "alert_all_machines";
    internal const string ALERT_USERS = "alert_users";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Email subscription"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description { get { return "Users can receive an email for each alert emitted by the system.\n\n" +
          "With this item you can create and configure the email subscriptions."; } }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "alarms", "alerts", "events", "emails", "notifications", "reports",
          "warnings", "errors" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "configurator"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Notifications"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Alerts"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IEmailConfig)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IEventLevel)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IEvent)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }
    
    /// <summary>
    /// All pages provided by the configurator
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IConfiguratorPage> Pages {
      get {
        IList<IConfiguratorPage> pages = new List<IConfiguratorPage>();
        if (ContextManager.UserCategory == LemSettingsGlobal.UserCategory.END_USER) {
          pages.Add(new Page1bis());
        }
        else {
          pages.Add(new Page1());
        }

        pages.Add(new Page2());
        pages.Add(new Page3());
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
    public ItemData Initialize(ItemData otherData)
    {
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      data.InitValue(FILTER_MACHINES, typeof(IList<IMachine>), new List<IMachine>(), false);
      data.InitValue(FILTER_USERS, typeof(IList<EmailWithName>), new List<EmailWithName>(), false);
      data.InitValue(FILTER_EVENT_TYPE, typeof(string), "", false);
      data.InitValue(FILTER_LEVEL, typeof(IEventLevel), null, false);
      data.InitValue (FILTER_ITEM, typeof(string), "", false);
      data.InitValue(CURRENT_ALERT, typeof(Alarm), null, true);
      data.InitValue(ALARM_MANAGER, typeof(AlarmManager), new AlarmManager(), false);
      data.InitValue(TREEVIEW_MACHINE_ORDER, typeof(int), 0, false);
      
      // Specific data for page 2
      data.CurrentPageName = "Page2";
      data.InitValue(TREEVIEW_MACHINE_LINKED, typeof(bool), false, false);
      data.InitValue(LIST_EMAILS_LINKED, typeof(bool), false, false);
      
      // Specific data for page 3
      data.CurrentPageName = "Page3";
      data.InitValue(ALERT_NAME, typeof(string), "", true);
      data.InitValue(ALERT_IS_ACTIVATED, typeof(bool), true, true);
      data.InitValue(ALERT_EVENT_TYPE, typeof(string), "", true);
      data.InitValue(ALERT_EVENT_LEVEL, typeof(IEventLevel), null, true);
      data.InitValue (ALERT_FILTER, typeof (string), "", true);
      data.InitValue(ALERT_DAYS_IN_WEEK, typeof(int), 0, true);
      data.InitValue(ALERT_START_DATE, typeof(DateTime?), null, true);
      data.InitValue(ALERT_EXPIRATION, typeof(DateTime?), null, true);
      data.InitValue(ALERT_PERIOD, typeof(TimePeriodOfDay), new TimePeriodOfDay(), true);
      data.InitValue(ALERT_MACHINES, typeof(IList<IMachine>), null, true);
      data.InitValue(ALERT_ALL_MACHINES, typeof(bool), true, true);
      data.InitValue(ALERT_USERS, typeof(IList<EmailWithName>), null, true);
      
      return data;
    }
    #endregion // Configurator methods
  }
}
