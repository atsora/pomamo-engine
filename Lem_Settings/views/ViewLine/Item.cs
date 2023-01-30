// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ViewLine
{
  /// <summary>
  /// Description of View.
  /// </summary>
  internal class Item : GenericItem, IView
  {
    internal const string LINE_NAME = "line_name";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Lines overview"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Overview of all lines, with their operations and machines associated.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "lines", "machines", "operations" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "view"; } }
    
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
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IOperation)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IPart)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the view
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IViewPage> Pages {
      get {
        IList<IViewPage> pages = new List<IViewPage>();
        pages.Add(new Page1());
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
      return new ItemData();
    }
    #endregion // View methods
  }
}
