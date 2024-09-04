// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardAddReason
{
  /// <summary>
  /// Description of PageSelectableReason.
  /// </summary>
  internal partial class PageSelectableReason : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Selectable reason properties"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "When applied to a set of machine modes and machine observation states, " +
          "the new reason may need additional details by the user. In that case, check the corresponding box.\n\n" +
          "The new reason may also appear only for a couple a machines. In that case, select the appropriate " +
          "machine filter."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageSelectableReason()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context)
    {
      // Machine filter list
      listMachineFilters.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachineFilter> mfs = ModelDAOHelper.DAOFactory.MachineFilterDAO.FindAll();
          foreach (var mf in mfs) {
            listMachineFilters.AddItem(mf.Name, mf);
          }
        }
      }
      listMachineFilters.InsertItem("none", null, 0, listMachineFilters.ForeColor, true, false);
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      checkDetails.Checked = data.Get<bool>(Item.DETAILS_REQUIRED);
      listMachineFilters.SelectedValue = data.Get<IMachineFilter>(Item.MACHINE_FILTER);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.DETAILS_REQUIRED, checkDetails.Checked);
      data.Store(Item.MACHINE_FILTER, listMachineFilters.SelectedValue);
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<bool>(Item.DETAILS_REQUIRED)) {
        summary.Add("details required");
      }
      else {
        summary.Add("details not required");
      }

      IMachineFilter mf = data.Get<IMachineFilter>(Item.MACHINE_FILTER);
      if (mf == null) {
        summary.Add("no machine filter");
      }
      else {
        summary.Add(String.Format("machine filter: \"{0}\"", mf.Name));
      }

      return summary;
    }
    #endregion // Page methods
  }
}
