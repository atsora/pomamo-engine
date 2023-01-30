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
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericWizardPage, IWizardPage
  {
    #region Members
    ILine m_line = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Production period"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose the production period you want to rename."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IWorkOrderLine));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
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
      m_line = data.Get<ILine>(Item.LINE);
      FillAndSelectWols(data.Get<IWorkOrderLine>(Item.WOL));
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      if (listProductions.SelectedValue == null) {
        data.Store(Item.WOL, null);
      }
      else {
        data.Store(Item.WOL, listProductions.SelectedValue);
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
      
      if (data.Get<IWorkOrderLine>(Item.WOL) == null) {
        errors.Add("a production period must be selected");
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
      return "Page3";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IWorkOrderLine wol = data.Get<IWorkOrderLine>(Item.WOL);
          ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
          string txt = String.Format("{0} ({1} \u2192 {2})",
                                     wol.WorkOrder.Display,
                                     wol.BeginDateTime.Value.ToLocalTime().ToString("d"),
                                     wol.Deadline.ToLocalTime().ToString("d"));
          summary.Add(txt);
        }
      }
      
      return summary;
    }
    
    #endregion // Page methods
    
    #region Private methods
    void FillAndSelectWols(IWorkOrderLine selectedWol)
    {
      // List of productions
      listProductions.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Get all wols
          ModelDAOHelper.DAOFactory.LineDAO.Lock(m_line);
          IList<IWorkOrderLine> wols = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAllByLine(m_line);
          DateTime today = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetTodayBeginUtcDateTime();
          
          // For all wol or for all wol having not begun yet
          foreach (IWorkOrderLine wol in wols) {
            if (wol.WorkOrder != null && (wol.Deadline >= today || checkShowAll.Checked)) {
              string txt = String.Format("{4}{0} \u2192 {1}, target: {2} part{3}",
                                         wol.BeginDateTime.Value.ToLocalTime().ToString("d"),
                                         wol.Deadline.ToLocalTime().ToString("d"),
                                         wol.Quantity,
                                         (wol.Quantity > 1 ? "s" : ""),
                                         string.IsNullOrEmpty(wol.WorkOrder.Display) ? "" : wol.WorkOrder.Display + ", ");
              listProductions.AddItem(txt, wol, wol.BeginDateTime.Value);
            }
          }
        }
      }
      
      // Select productions
      listProductions.SelectedValue = selectedWol;
      if (listProductions.SelectedValue == null) {
        listProductions.SelectedIndex = 0;
      }
    }
    #endregion // Private methods
    
    #region Event reactions
    void CheckShowAllCheckedChanged(object sender, EventArgs e)
    {
      FillAndSelectWols(listProductions.SelectedValue as IWorkOrderLine);
    }
    #endregion // Event reactions
  }
}
