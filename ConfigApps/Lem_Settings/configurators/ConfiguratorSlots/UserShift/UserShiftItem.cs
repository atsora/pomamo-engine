// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserShift
{
  /// <summary>
  /// Description of UserShiftItem.
  /// </summary>
  internal class UserShiftItem : AbstractItem, IConfigurator
  {
    internal const string SHIFT = "shift";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Plan shifts"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Assign shifts for users over a specified period.";
      }
    }

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "shifts", "users", "plannings", "calendars", "planings" };
      }
    }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Users"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Planning"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IUserShiftSlot)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IUser)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IShift)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
        pages.Add(new Page1(new UserShiftPage1()));
        pages.Add(new Page2(new UserShiftPage2()));
        pages.Add(new Page3(new UserShiftPage3()));
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Methods
    protected override void InitializeData(ItemData data, ItemData otherData)
    {
      data.CurrentPageName = "";
      IList<object> items = new List<object>();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        IList<IUser> users = ModelDAOHelper.DAOFactory.UserDAO.FindAll();
        foreach (IUser user in users) {
          items.Add(user);
        }
      }
      data.InitValue(AbstractItem.SELECTED_ITEMS, typeof(IList<object>), items, true);
      
      data.CurrentPageName = "Page3";
      data.InitValue(SHIFT, typeof(IShift), null, true);
    }
    #endregion // Methods
  }
}
