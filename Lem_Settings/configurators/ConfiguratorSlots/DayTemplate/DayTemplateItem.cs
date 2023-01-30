// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorSlots.DayTemplate
{
  /// <summary>
  /// Description of DayTemplateItem.
  /// </summary>
  internal class DayTemplateItem : AbstractItem, IConfigurator
  {
    internal const string DAY_TEMPLATE = "day_template";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Day template schedule"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "A day template represents a typical day that is going to be encountered " +
          "a lot of time during a year. It is, in particular, characterized by a cut-off time " +
          "(the time during the day after which it is considered the next day begins). " +
          "We can for example imagine several kinds of days depending on the seasons with " +
          "summer and winter times.\n\n" +
          "This configurator allows you to schedule different periods with different day templates. " +
          "You don't edit the templates but you define which one is used according to the time.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "templates", "days", "periods", "time", "shifts" };
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
        dic[typeof(IDayTemplateSlot)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IDayTemplate)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;

    /// <summary>
    /// All pages provided by the configurator
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IConfiguratorPage> Pages {
      get {
        IList<IConfiguratorPage> pages = new List<IConfiguratorPage>();
        pages.Add(new Page1(new DayTemplatePage1()));
        // No page 2 since the selection of elements is not needed
        pages.Add(new Page3(new DayTemplatePage3()));
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Methods
    protected override void InitializeData(ItemData data, ItemData otherData)
    {
      data.CurrentPageName = "";
      IList<object> elements = new List<object>();
      elements.Add(null);
      data.InitValue(AbstractItem.SELECTED_ITEMS, typeof(IList<object>), elements, true);
      
      data.CurrentPageName = "Page3";
      data.InitValue(DAY_TEMPLATE, typeof(IDayTemplate), null, true);
    }
    #endregion // Methods
  }
}
