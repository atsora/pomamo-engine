// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace WizardCreateMachine
{
  /// <summary>
  /// First item that creates a machine and configures it
  /// </summary>
  internal class Item : AbstractItem, IWizard
  {
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Create a machine"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Create a new machine that can then be monitored.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "machines" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "new_machine"; } }
    
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
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
      }
    }
    
    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new PageIdentification());
        pages.Add(new PageCompany());
        pages.Add(new PageDepartment());
        pages.Add(new PageCategory());
        pages.Add(new PageSubcategory());
        pages.Add(new PageCell());
        pages.Add(new PageOrder());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Wizard methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherItemData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize(ItemData otherItemData)
    {
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      
      data.InitValue(MACHINE_NAME, typeof(string), "", true);
      data.InitValue(MACHINE_CODE, typeof(string), "", true);
      data.InitValue(COMPANY, typeof(ICompany), null, true);
      data.InitValue(DEPARTMENT, typeof(IDepartment), null, true);
      data.InitValue(CATEGORY, typeof(IMachineCategory), null, true);
      data.InitValue(SUBCATEGORY, typeof(IMachineSubCategory), null, true);
      data.InitValue(CELL, typeof(ICell), null, true);
      data.InitValue(MACHINE_PRIORITY, typeof(double), -1.0, true);
      data.InitValue(MACHINE, typeof(IMachine), null, false);
      data.InitValue(MODIFICATION, typeof(bool), false, false);
      
      return data;
    }
    
    /// <summary>
    /// Get a list of items for proposing the next possible steps to the user
    /// once the current wizard is successfully finished
    /// The value of the dictionary to return is an infinive text such as "monitor the machine"
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <returns>See the description above, can be null</returns>
    public override IDictionary<string, string> GetPossibleNextItems(ItemData data)
    {
      var nextItems = new Dictionary<string, string>();
      nextItems["WizardMonitorMachine.Item"] = "monitor the machine";
      return nextItems;
    }
    #endregion // Wizard methods
  }
}
