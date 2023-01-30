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
  /// Second item that configures a machine already created
  /// </summary>
  internal class Item2 : AbstractItem, IWizard
  {
    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Edit a machine"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "Change the attributes of a machine.";
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "edit_machine"; } }
    
    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new PageMachine());
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
      
      // The machine may come from a previous data
      IMachine machine = null;
      if (otherItemData != null) {
        machine = otherItemData.IsStored<IMachine>(MACHINE) ?
          otherItemData.Get<IMachine>(MACHINE) : null;
      }

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
      data.InitValue(MACHINE, typeof(IMachine), machine, true);
      data.InitValue(MODIFICATION, typeof(bool), true, false);
      
      return data;
    }
    #endregion // Wizard methods
  }
}
