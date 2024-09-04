// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.BaseControls.List;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace ConfiguratorMachineCategories
{
  /// <summary>
  /// Description of ItemCommon.
  /// </summary>
  public abstract class ItemCommon: GenericItem
  {
    #region Members
    string m_elementNamePlural;
    Type m_type;
    #endregion // Members

    internal const string ELEMENTS = "elements";
    internal const string POSITION = "position";
    static readonly ILog log = LogManager.GetLogger(typeof (ItemCommon).FullName);

    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return m_elementNamePlural; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Rename / add or remove " + m_elementNamePlural.ToLower() + ".";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines", "category", "identifications", "categories", m_elementNamePlural };
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
    public override string Subcategory { get { return "Attributes"; } }
    
    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags { get { return LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[m_type] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }
    
    /// <summary>
    /// All pages provided by the configurator
    /// </summary>
    public IList<IConfiguratorPage> Pages {
      get {
        var pages = new List<IConfiguratorPage>();
        var page = new Page(m_elementNamePlural);
        page.OnValidate += Validate;
        page.OnFillMachines += FillMachines;
        pages.Add(page);
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected ItemCommon(string elementNamePlural, Type type) : base()
    {
      m_elementNamePlural = elementNamePlural;
      m_type = type;
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
      data.InitValue(ELEMENTS, typeof(Container), new Container(GetElements()), true);
      data.InitValue(POSITION, typeof(int), -1, false);
      
      return data;
    }
    #endregion // Configurator methods
    
    #region Abstract methods
    protected abstract IList<Container.Element> GetElements();
    protected abstract void FillMachines(ListTextValue list, Container.Element element);
    protected abstract void Validate(Container container);
    #endregion // Abstract methods
  }
}
