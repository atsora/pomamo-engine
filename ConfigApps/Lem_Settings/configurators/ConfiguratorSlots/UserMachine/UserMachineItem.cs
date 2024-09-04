// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.UserMachine
{
  /// <summary>
  /// Description of UserMachineItem.
  /// </summary>
  internal class UserMachineItem : AbstractItem, IConfigurator
  {
    internal const string MACHINES = "machines";
    internal const string MACHINE_STATE_TEMPLATES = "machine_state_templates";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Plan machine attendance"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Machines may need a user nearby. You will be able to plan attendance " +
          "with this configurator.";
      }
    }

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "users", "plannings", "calendars", "planings",
          "attendance", "machines" };
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
        dic[typeof(IUserMachineSlot)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IUserMachineSlotMachine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IUser)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachineStateTemplate)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
        pages.Add(new Page1(new UserMachinePage1()));
        pages.Add(new Page2(new UserMachinePage2()));
        pages.Add(new Page3(new UserMachinePage3()));
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
      data.InitValue(MACHINES, typeof(IList<IMachine>), new List<IMachine>(), true);
      data.InitValue(MACHINE_STATE_TEMPLATES, typeof(IList<IMachineStateTemplate>), new List<IMachineStateTemplate>(), true);
    }
    #endregion // Methods
  }
}
