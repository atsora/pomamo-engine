// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorJobComponentName
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IConfigurator
  {
    internal const string ELEMENT_TO_RENAME = "element_to_rename";
    internal const string JOB_NAME = "job_name";
    internal const string COMPONENT_NAME = "component_name";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Fix job and component names"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Job and component names are automatically taken from the isofile during the stamping process." +
          "Names are read either from the file name or from comments (CADName=... for example).\n\n" +
          "If names cannot be read correctly, this item will be used to manually set them.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "isofiles", "rename", "jobs", "components", "stamping" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "configurator"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Parts and Operations"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Operation properties"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IJob)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IComponent)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IWorkOrder)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IProject)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IOperation)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IIsoFile)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IStamp)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
        pages.Add(new Page1());
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
      data.InitValue(ELEMENT_TO_RENAME, typeof(ElementToRename), null, true);
      data.InitValue(JOB_NAME, typeof(string), "", true);
      data.InitValue(COMPONENT_NAME, typeof(string), "", true);
      
      return data;
    }
    #endregion // Configurator methods
  }
}
