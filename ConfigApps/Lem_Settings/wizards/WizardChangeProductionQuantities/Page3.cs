// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardChangeProductionQuantities
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
    public string Title { get { return "Global quantity"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "You can change here the new global quantity of parts to produce " +
          "for the current production periods.\n\n" +
          "The \"auto\" value is computed based on the previous number of parts per operation."; } }
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
      // Previous target(s)
      IList<int> previousQtts = GetPreviousTargets(data.Get<IList<IWorkOrder>>(Item.WOS),
                                                   data.Get<ILine>(Item.LINE));
      string str = "Previous quantit" + ((previousQtts.Count > 1) ? "ies" : "y") + ": ";
      str += previousQtts[0].ToString();
      for (int i = 1; i < previousQtts.Count; i++) {
        str += ", " + previousQtts[i];
      }

      labelPreviousTarget.Text = str;
      
      // Auto sum
      int? autoSum = data.Get<Quantities>(Item.QUANTITIES).GetSumPerOperation();
      if (data.Get<int>(Item.GLOBAL_QUANTITY) == -1) {
        if (autoSum.HasValue) {
          numericQtt.Value = autoSum.Value;
        }
        else {
          numericQtt.Text = "";
        }
      } else {
        numericQtt.Value = data.Get<int>(Item.GLOBAL_QUANTITY);
      }

      // Select the radiobutton
      radioAuto.Enabled = autoSum.HasValue;
      if (autoSum.HasValue) {
        radioAuto.Text = "Auto (" + autoSum.Value + " part" + ((autoSum.Value > 1) ? "s" : "") + ")";
      }
      else {
        radioAuto.Text = "Auto";
      }

      if (data.Get<bool>(Item.CHANGE_GLOBAL)) {
        if (data.Get<bool>(Item.GLOBAL_AUTO) && autoSum.HasValue) {
          radioAuto.Checked = true;
        }
        else {
          radioYes.Checked = true;
        }
      } else {
        radioNo.Checked = true;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.CHANGE_GLOBAL, radioYes.Checked || radioAuto.Checked);
      data.Store(Item.GLOBAL_AUTO, radioAuto.Checked);
      if (numericQtt.Text == "") {
        data.Store(Item.GLOBAL_QUANTITY, -1);
      }
      else {
        data.Store(Item.GLOBAL_QUANTITY, (int)numericQtt.Value);
      }
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<bool>(Item.CHANGE_GLOBAL) && data.Get<int>(Item.GLOBAL_QUANTITY) == -1) {
        errors.Add("the quantity must be provided");
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
      
      // Possibly adjust the number of parts
      if (data.Get<bool>(Item.CHANGE_GLOBAL) && data.Get<bool>(Item.GLOBAL_AUTO)) {
        int? sum = data.Get<Quantities>(Item.QUANTITIES).GetSumPerOperation();
        data.Store(Item.GLOBAL_QUANTITY, sum.Value);
      }
      
      return warnings;
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
      
      if (data.Get<bool>(Item.CHANGE_GLOBAL)) {
        string plural = (data.Get<int>(Item.GLOBAL_QUANTITY) > 1) ? "s" : "";
        summary.Add("new quantity: " + data.Get<int>(Item.GLOBAL_QUANTITY) + " part" + plural);
      } else {
        summary.Add("no changes");
      }

      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    IList<int> GetPreviousTargets(IList<IWorkOrder> wos, ILine line)
    {
      List<int> result = new List<int>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          foreach (IWorkOrder wo in wos) {
            ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock(wo);
            IWorkOrderLine wol = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindByLineAndWorkOrder(line, wo);
            if (wol != null && !result.Contains(wol.Quantity)) {
              result.Add(wol.Quantity);
            }
          }
        }
      }
      
      result.Sort();
      return result;
    }
    #endregion // Private methods
    
    #region Event reactions
    void RadioAutoCheckedChanged(object sender, EventArgs e)
    {
      if (radioAuto.Checked) {
        numericQtt.Enabled = false;
      }
    }
    
    void RadioYesCheckedChanged(object sender, System.EventArgs e)
    {
      if (radioYes.Checked) {
        numericQtt.Enabled = true;
      }
    }
    
    void RadioNoCheckedChanged(object sender, System.EventArgs e)
    {
      if (radioNo.Checked) {
        numericQtt.Enabled = false;
      }
    }
    #endregion // Event reactions
  }
}
