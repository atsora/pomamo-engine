// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardRenameProduction
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  internal partial class Page3 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "New name for the production period"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose the new name of the production period."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3()
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
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Previous name
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IWorkOrderLine wol = data.Get<IWorkOrderLine>(Item.WOL);
          ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
          labelPreviousName.Text = wol.WorkOrder.Name;
        }
      }
      
      // New name
      if (data.Get<string>(Item.WOL_NAME) == "") {
        textName.Text = labelPreviousName.Text;
      }
      else {
        textName.Text = data.Get<string>(Item.WOL_NAME);
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.WOL_NAME, textName.Text);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<string>(Item.WOL_NAME) == "") {
        errors.Add("the name cannot be empty");
      }

      return errors;
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      return "";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      summary.Add(data.Get<string>(Item.WOL_NAME));
      
      return summary;
    }
    #endregion // Page methods
  }
}
