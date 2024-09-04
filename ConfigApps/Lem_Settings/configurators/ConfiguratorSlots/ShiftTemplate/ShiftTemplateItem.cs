// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorSlots.ShiftTemplate
{
  /// <summary>
  /// Description of ShiftTemplateItem.
  /// </summary>
  internal class ShiftTemplateItem : AbstractItem, IConfigurator
  {
    internal const string SHIFT_TEMPLATE = "shift_template";
    internal const string FORCE_ASSOCIATION = "force_association";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Global shift template schedule"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "A shift template is a series of shifts within a day or a week " +
          "that frequently occurs within a year.\n\n" +
          "This configurator allows you to schedule different periods with different shift templates. " +
          "You don't edit the templates but you define which one is used according to the time.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "templates", "shifts", "periods", "time", "days" };
      }
    }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Global configurations"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Day / Shift"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IShiftTemplateSlot)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IShiftTemplate)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
        pages.Add(new Page1(new ShiftTemplatePage1()));
        // No page 2 since the selection of elements is not needed
        pages.Add(new Page3(new ShiftTemplatePage3()));
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Methods
    protected override void InitializeData(ItemData data, ItemData otherData)
    {
      data.CurrentPageName = "";
      IList<object> items = new List<object>();
      items.Add(null);
      data.InitValue(AbstractItem.SELECTED_ITEMS, typeof(IList<object>), items, true);
      
      data.CurrentPageName = "Page3";
      data.InitValue(SHIFT_TEMPLATE, typeof(IShiftTemplate), null, true);
      data.InitValue(FORCE_ASSOCIATION, typeof(bool), false, true);
    }
    #endregion // Methods
  }
}
