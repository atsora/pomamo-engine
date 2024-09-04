// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Settings;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class ItemView : GenericItem, IView
  {
    internal const string MACHINE = "machine";
    
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "View machines"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Display all machines configured with the configuration " +
          "of their cnc acquisition. Checks are made per machine to see if everything is right.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines, configurations, acquisition, cnc" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "view_machine"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Configuration"; } }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags => LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IMachine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMonitoredMachine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IMachineModule)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(ICncAcquisition)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof(IComputer)] = LemSettingsGlobal.InteractionType.SECONDARY;
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
        pages.Add(new PageViewMachines());
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
      
      // The machine may come from a previous data
      IMachine machine = null;
      if (otherData != null) {
        machine = otherData.IsStored<IMachine>(MACHINE) ?
          otherData.Get<IMachine>(MACHINE) : null;
      }

      // Common data
      data.CurrentPageName = "";
      data.InitValue(MACHINE, typeof(IMachine), machine, true);
      
      return data;
    }
    #endregion // View methods
  }
}
