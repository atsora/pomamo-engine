// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorRedirectEvent
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IConfigurator
  {
    #region Members
    readonly bool m_advancedMode;
    #endregion // Members
    
    internal static readonly string CURRENT_ACTION = "current_action";
    internal static readonly string ACTIVATION_STATE = "activation_state";
    internal static readonly string ACTION_MANAGER = "action_manager";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return Context.ViewMode ? "Event to action redirections" : "Redirect events to actions"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "This configurator links events to specific actions such as triggering email alerts, " +
          "sending emails, writing logs.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "alarms", "alerts", "events", "emails", "notifications" };
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
    public override string Subcategory { get { return "Events"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IEvent)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IEventLevel)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }
    
    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN |
          LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED |
          LemSettingsGlobal.ItemFlag.ONLY_LCTR;
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
        pages.Add(new Page1());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="advancedMode"></param>
    public Item(bool advancedMode) : base()
    {
      m_advancedMode = advancedMode;
    }
    #endregion // Constructors

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
      data.InitValue(CURRENT_ACTION, typeof(string), "", true);
      data.InitValue(ACTIVATION_STATE, typeof(bool), true, true);
      data.InitValue(ACTION_MANAGER, typeof(ActionManager), new ActionManager(m_advancedMode), false);
      
      return data;
    }
    #endregion // Configurator methods
  }
}
