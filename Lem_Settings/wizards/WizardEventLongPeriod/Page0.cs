// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.ModelDAO;

namespace WizardEventLongPeriod
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page0 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Modification method"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Two ways for editing the long period events are possible:\n\n" +
          " - by only adding new long period events (and possibly overwriting existing ones),\n\n" +
          " - or by clearing all existing long period events and then adding new ones."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page0()
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
      if (data.Get<bool>(Item.CHOICE_DONE)) {
        if (data.Get<bool>(Item.CLEAR_FIRST)) {
          radioAddOnly.Checked = false;
          radioDeleteAndAdd.Checked = true;
        } else {
          radioDeleteAndAdd.Checked = false;
          radioAddOnly.Checked = true;
        }
      } else {
        radioAddOnly.AutoCheck = false;
        radioDeleteAndAdd.AutoCheck = false;
        radioAddOnly.Checked = false;
        radioDeleteAndAdd.Checked = false;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      if (radioAddOnly.Checked) {
        data.Store(Item.CHOICE_DONE, true);
        data.Store(Item.CLEAR_FIRST, false);
      } else if (radioDeleteAndAdd.Checked) {
        data.Store(Item.CHOICE_DONE, true);
        data.Store(Item.CLEAR_FIRST, true);
      } else {
        data.Store(Item.CHOICE_DONE, false);
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
      
      if (!data.Get<bool>(Item.CHOICE_DONE)) {
        errors.Add("a method has to be chosen");
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
      
      if (data.Get<bool>(Item.CLEAR_FIRST)) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            int count = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.FindAll().Count;
            if (count > 0) {
              warnings.Add(String.Format("{0} existing long period event{1} will be deleted.",
                                         count, count > 1 ? "s" : ""));
            }
          }
        }
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
      return "Page1";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<bool>(Item.CLEAR_FIRST)) {
        summary.Add("First clear all and then add long period events.");
      }
      else {
        summary.Add("Add long period events, overwrite if needed.");
      }

      return summary;
    }
    #endregion // Page methods

    #region Event reactions
    void RadioAddOnlyMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      radioAddOnly.AutoCheck = true;
      radioDeleteAndAdd.AutoCheck = true;
      radioAddOnly.Checked = true;
    }
    
    void RadioDeleteAndAddMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      radioAddOnly.AutoCheck = true;
      radioDeleteAndAdd.AutoCheck = true;
      radioDeleteAndAdd.Checked = true;
    }
    #endregion // Event reactions
  }
}
