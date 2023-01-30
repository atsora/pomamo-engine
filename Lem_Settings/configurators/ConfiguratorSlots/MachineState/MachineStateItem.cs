// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorSlots.MachineState
{
  /// <summary>
  /// Description of MachineStateItem.
  /// </summary>
  internal class MachineStateItem : AbstractItem, IConfigurator
  {
    internal const string TREE_DISPLAY_ORDER = "tree_display_order";
    internal const string MACHINE_STATE_TEMPLATE = "machine_state_template";
    internal const string SHIFT = "shift";
    internal const string HAS_SHIFT = "has_shift";
    internal const string FORCE_REBUILD = "force_rebuild";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Machine state template schedule"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Change the state template of one or several machine(s) on a specified period.\n\n" +
          "A machine state template is a series of machine observation states that can be applied " +
          "in periods. Thanks to them it is possible to define when a machine is attended 8 hours " +
          "a day (normal work), and when the machine is not attended at all (annual vacation for " +
          "instance). This would require only two templates.\n\n" +
          "You don't define directly what state to use over a specified period for machines, but which " +
          "template is used according to the time.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines", "states", "templates", "periods", "time" };
      }
    }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Status"; } }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IObservationStateSlot)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachineStateTemplate)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.SECONDARY;
        dic[typeof(IShift)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
        pages.Add(new Page1(new MachineStatePage1()));
        pages.Add(new Page2(new MachineStatePage2()));
        pages.Add(new Page3(new MachineStatePage3()));
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Methods
    protected override void InitializeData(ItemData data, ItemData otherData)
    {
      data.CurrentPageName = "";

      // Initialize the list of machines
      IList<object> items = new List<object> ();
      if (otherData != null && otherData.IsStored<IMachine> ("machine")) {
        // First try to find if a machine is already specified
        IMachine machine = otherData.Get<IMachine> ("machine");
        if (machine != null) {
          items.Add (machine);
        }
      }
      if (items.Count == 0) {
        // By default, take all non obsolete machines
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAllNotObsolete ();
          foreach (IMachine machine in machines) {
            items.Add (machine);
          }
        }
      }
      data.InitValue(AbstractItem.SELECTED_ITEMS, typeof(IList<object>), items, true);
      
      data.CurrentPageName = "Page2";
      data.InitValue(TREE_DISPLAY_ORDER, typeof(int), 0, false);
      
      data.CurrentPageName = "Page3";
      data.InitValue(MACHINE_STATE_TEMPLATE, typeof(IMachineStateTemplate), null, true);
      data.InitValue(SHIFT, typeof(IShift), null, true);
      data.InitValue(HAS_SHIFT, typeof(bool), false, true);
      data.InitValue(FORCE_REBUILD, typeof(bool), false, true);
    }
    #endregion // Methods
  }
}
