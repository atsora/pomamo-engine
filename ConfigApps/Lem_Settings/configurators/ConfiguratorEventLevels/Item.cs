// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorEventLevel
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IConfigurator
  {
    internal const string EDITED_LEVELS = "edited_levels";
    internal const string DELETED_LEVELS = "deleted_levels";
    internal const string ADDED_LEVELS = "added_levels";
    internal const string CURRENT_LEVEL = "current_level";
    internal const string PRIORITY = "priority";
    internal const string NAME = "name";
    internal const string EDITED = "edited";
    internal const string HAS_TRANSLATION_KEY = "has_translation_key";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return Context.ViewMode ? "View event levels" : "Change event levels"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description { get { return "This item allows you to add, delete or edit the event levels."; } }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "alarms", "alerts", "events", "notifications", "levels", "critical",
          "criticity", "info", "warnings", "errors" };
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
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN |
          LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED;
      }
    }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IEventLevel)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
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
        var pages = new List<IConfiguratorPage>();
        pages.Add(new Page1());
        pages.Add(new Page2());
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
      data.InitValue(ADDED_LEVELS, typeof(IList<IEventLevel>), new List<IEventLevel>(), true);
      data.InitValue(CURRENT_LEVEL, typeof(IEventLevel), null, false);
      data.InitValue(EDITED, typeof(bool), false, false);
      
      // Data specific for page 1
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          data.InitValue(EDITED_LEVELS, typeof(IList<IEventLevel>), ModelDAOHelper.DAOFactory.EventLevelDAO.FindAll(), true);
        }
      }
      data.InitValue(DELETED_LEVELS, typeof(IList<IEventLevel>), new List<IEventLevel>(), true);
      
      // Data specific for page 2
      data.CurrentPageName = "Page2";
      data.InitValue(PRIORITY, typeof(int), 50, true);
      data.InitValue(NAME, typeof(string), "", true);
      data.InitValue(HAS_TRANSLATION_KEY, typeof(bool), false, true);
      
      return data;
    }
    #endregion // Configurator methods
  }
}
