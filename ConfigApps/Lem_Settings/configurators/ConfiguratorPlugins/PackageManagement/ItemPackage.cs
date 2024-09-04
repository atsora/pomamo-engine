// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.Extensions.Plugin;
using Lemoine.Extensions.Interfaces;

namespace ConfiguratorPlugins
{
  /// <summary>
  /// Description of ItemPackage.
  /// </summary>
  internal class ItemPackage : GenericItem, IConfigurator
  {
    internal const string CURRENT_PACKAGE_NAME = "current_package_name";
    internal const string CURRENT_PLUGIN = "current_plugin";
    internal const string CURRENT_ASSOCIATION = "current_association_name";
    internal const string ACTIVATED = "activated";
    internal const string CUSTOM_ACTION = "custom_action";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return Context.ViewMode ? "View packages" : "Manage packages"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "A package adds extra features.\n\nUse this configurator to " +
          "install, configure or uninstall packages. Specific actions provided by packages " +
          "may further be found here.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "packages, plugins, addons, extensions, add-ons" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "configurator"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Global configurations"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Installation"; } }
    
    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED |
          LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
      }
    }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IPlugin)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IPackage)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
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
        pages.Add(new PagePackage());
        pages.Add(new PagePluginConf());
        pages.Add(new PagePackageInformation());
        pages.Add(new PagePluginInformation());
        pages.Add(new PagePluginAction());
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
      data.InitValue(CURRENT_PACKAGE_NAME, typeof(string), "", true);
      data.InitValue(CURRENT_PLUGIN, typeof(IPluginDll), null, true);
      data.InitValue(CURRENT_ASSOCIATION, typeof(IPackagePluginAssociation), null, true);
      data.InitValue(CUSTOM_ACTION, typeof(int), -1, false);
      data.InitValue(ACTIVATED, typeof(bool), true, true);
      
      // Specific data for page PagePluginConf
      data.CurrentPageName = "PagePluginConf";
      
      return data;
    }
    #endregion // Configurator methods
  }
}
