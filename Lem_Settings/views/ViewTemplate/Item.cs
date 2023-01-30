// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;

namespace ViewTemplate
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IView
  {
    internal const string LINE_NAME = "line_name";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "View"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "This item serves as a template for all views.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "templates" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "view"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Templates"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return ""; } }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;
      }
    }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(Int32)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        return dic;
      }
    }
    
    /// <summary>
    /// All pages provided by the view
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IViewPage> Pages {
      get {
        IList<IViewPage> pages = new List<IViewPage>();
        pages.Add(new Page1());
        pages.Add(new Page2());
        pages.Add(new Page3());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region View methods
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
      data.InitValue(LINE_NAME, typeof(string), "", false);
      
      // Specific data for page 1
      data.CurrentPageName = "Page1";
      
      return data;
    }
    #endregion // View methods
  }
}
