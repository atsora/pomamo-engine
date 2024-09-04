// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorLine
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IConfigurator
  {
    // General
    internal const string LINE = "line";
    internal const string OPERATION = "operation";
    internal const string MACHINE = "machine";
    
    // Page 1
    internal const string LINE_DATA = "line_data";
    internal const string LINE_EDITED = "line_edited";
    
    // Page 2
    internal const string LINE_NAME = "line_name";
    internal const string LINE_CODE = "line_code";
    internal const string PARTS = "parts";
    
    // Page 3
    internal const string OPERATION_NAME = "operation_name";
    internal const string OPERATION_CODE = "operation_code";
    internal const string OPERATION_MAX_PART = "operation_max_part_per_cycle";
    
    // Page 4
    internal const string DELETE_PART = "delete_part";
    internal const string DELETE_OPERATIONS = "delete_operations";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Edit lines"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Existing lines may be edited here: their properties, " +
          "operations associated and machines involved.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines", "lines", "operations" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "configurator"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Production line"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Line settings"; } }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(ILine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IOperation)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(Lemoine.Model.IComponent)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IPart)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the configurator
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IConfiguratorPage> Pages {
      get {
        IList<IConfiguratorPage> pages = new List<IConfiguratorPage>();
        pages.Add(new Page0(Context));
        pages.Add(new Page1(Context));
        pages.Add(new Page2());
        pages.Add(new Page3());
        pages.Add(new Page4());
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
      data.InitValue(LINE, typeof(ILine), null, true);
      data.InitValue(OPERATION, typeof(IOperation), null, true);
      data.InitValue(MACHINE, typeof(IMachine), null, true);
      data.InitValue(LINE_DATA, typeof(LineData), new LineData(), true);
      
      // Page 1
      data.CurrentPageName = "Page1";
      data.InitValue(LINE_EDITED, typeof(bool), false, false);
      
      // Page 2
      data.CurrentPageName = "Page2";
      data.InitValue(LINE_NAME, typeof(string), "", true);
      data.InitValue(LINE_CODE, typeof(string), "", true);
      data.InitValue(PARTS, typeof(PartData), new PartData(), true);
      
      // Page 3
      data.CurrentPageName = "Page3";
      data.InitValue(OPERATION_NAME, typeof(string), "", true);
      data.InitValue(OPERATION_CODE, typeof(string), "", true);
      data.InitValue(OPERATION_MAX_PART, typeof(int), -1, true);
      
      // Page 4
      data.CurrentPageName = "Page4";
      data.InitValue(DELETE_PART, typeof(bool), false, true);
      data.InitValue(DELETE_OPERATIONS, typeof(bool), false, true);
      
      return data;
    }
    #endregion // Configurator methods
  }
}
