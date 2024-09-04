// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Settings;
using Lemoine.ModelDAO;

namespace WizardEventToolLife
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
    public string Help { get { return "Three ways for editing the tool life events are possible:\n\n" +
          " - by only adding new tool life events (and possibly overwriting existing ones),\n\n" +
          " - by clearing all existing tool life events and then adding new ones,\n\n" +
          " - or by just clearing everything."; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page0()
    {
      InitializeComponent();
      dataGrid.RowHeadersVisible = false;
      dataGrid.ColumnCount = 4;
      dataGrid.Columns[0].Name = "Type";
      dataGrid.Columns[1].Name = "Machine filter";
      dataGrid.Columns[2].Name = "Machine state";
      dataGrid.Columns[3].Name = "Event level";
      dataGrid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGrid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGrid.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
      dataGrid.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
      // Recall old events
      dataGrid.Rows.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var configs = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAll();
          foreach (var config in configs) {
            dataGrid.Rows.Add(
              config.Type.Name(),
              config.MachineFilter != null ? config.MachineFilter.Name : "-",
              config.MachineObservationState != null ? config.MachineObservationState.Display : "-",
              config.Level != null ? config.Level.Display : "-");
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
      switch (data.Get<int>(Item.CHOICE)) {
        case 1:
          radioAddOnly.Checked = true;
          radioDeleteAndAdd.Checked = false;
          radioDeleteOnly.Checked = false;
          break;
        case 2:
          radioAddOnly.Checked = false;
          radioDeleteAndAdd.Checked = true;
          radioDeleteOnly.Checked = false;
          break;
        case 3:
          radioAddOnly.Checked = false;
          radioDeleteAndAdd.Checked = false;
          radioDeleteOnly.Checked = true;
          break;
        default:
          radioAddOnly.Checked = false;
          radioDeleteAndAdd.Checked = false;
          radioDeleteOnly.Checked = false;
          break;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      if (radioAddOnly.Checked) {
        data.Store(Item.CHOICE, 1);
      }
      else if (radioDeleteAndAdd.Checked) {
        data.Store(Item.CHOICE, 2);
      }
      else if (radioDeleteOnly.Checked) {
        data.Store(Item.CHOICE, 3);
      }
      else {
        data.Store(Item.CHOICE, 0);
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
      
      if (data.Get<int>(Item.CHOICE) == 0) {
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
      
      if (data.Get<int>(Item.CHOICE) == 2 || data.Get<int>(Item.CHOICE) == 3) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            int count = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAll().Count;
            if (count > 0) {
              warnings.Add(String.Format("{0} existing tool life event{1} will be deleted.",
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
      return data.Get<int>(Item.CHOICE) == 3 ? "" : "Page1";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      switch (data.Get<int>(Item.CHOICE)) {
        case 1:
          summary.Add("Add tool life events, overwrite if needed.");
          break;
        case 2:
          summary.Add("First clear all and then add tool life events.");
          break;
        case 3:
          summary.Add("Clear all tool life events.");
          break;
      } 
      
      return summary;
    }
    #endregion // Page methods

    #region Event reactions
    void RadioAddOnlyMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      radioAddOnly.Checked = true;
      radioDeleteOnly.Checked = false;
      radioDeleteAndAdd.Checked = false;
    }
    
    void RadioDeleteAndAddMouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
    {
      radioDeleteAndAdd.Checked = true;
      radioDeleteOnly.Checked = false;
      radioAddOnly.Checked = false;
    }
    
    void RadioDeleteOnlyClick(object sender, EventArgs e)
    {
      radioDeleteOnly.Checked = true;
      radioDeleteAndAdd.Checked = false;
      radioAddOnly.Checked = false;
    }
    #endregion // Event reactions
  }
}
