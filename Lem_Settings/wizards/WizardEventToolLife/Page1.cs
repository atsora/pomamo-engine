// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardEventToolLife
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Machine filters"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select here the machine filters associated " +
          "with the tool life events you want to create. The more filters you select, " +
          "the more machines will be impacted.\n\n" +
          "It is possible to ignore the effect of filters and include all machines " +
          "by clicking on \"None\"."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IMachineFilter));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
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
      // List of all machine filters
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachineFilter> mafis = ModelDAOHelper.DAOFactory.MachineFilterDAO.FindAll();
          foreach (IMachineFilter mafi in mafis) {
            listMachineFilters.AddItem(mafi.SelectionText + " (" + mafi.Name + ")", mafi);
          }
        }
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      listMachineFilters.SelectedValues = data.Get<IList<IMachineFilter>>(Item.MACHINE_FILTERS).Cast<object>().ToList();
      checkAll.Checked = data.Get<bool>(Item.NO_MACHINE_FILTERS);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.MACHINE_FILTERS, listMachineFilters.SelectedValues.Cast<IMachineFilter>().ToList());
      data.Store(Item.NO_MACHINE_FILTERS, checkAll.Checked);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (!data.Get<bool>(Item.NO_MACHINE_FILTERS) &&
          data.Get<IList<IMachineFilter>>(Item.MACHINE_FILTERS).Count == 0) {
        errors.Add("at least one machine filter must be selected if you don't check \"None\"");
      }

      return errors;
    }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings(ItemData data)
    {
      IList<string> warnings = new List<string>();
      
      return warnings;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "Page3";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<bool>(Item.NO_MACHINE_FILTERS)) {
        summary.Add("None");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IList<IMachineFilter> mafis = data.Get<IList<IMachineFilter>>(Item.MACHINE_FILTERS);
            foreach (IMachineFilter mafi in mafis) {
              ModelDAOHelper.DAOFactory.MachineFilterDAO.Lock(mafi);
              summary.Add(mafi.SelectionText + " (" + mafi.Name + ")");
            }
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
    
    #region Event reactions
    void CheckAllCheckedChanged(object sender, EventArgs e)
    {
      listMachineFilters.Enabled = !checkAll.Checked;
    }
    #endregion // Event reactions
  }
}
