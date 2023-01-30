// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardDeleteProduction
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Production periods"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose the production period you want to delete. Several items may be selected.\n\n" +
          "Please note that a past period or a period in progress cannot be deleted. These periods are not be listed here."; } }
    
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
    public void Initialize(ItemContext context)
    {
      
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // List of productions
      listProductions.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Get all wols
          ILine line = data.Get<ILine>(Item.LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          IList<IWorkOrderLine> wols = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAllByLine(line);

          // For all wol having not begun yet
          DateTime today = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetTodayEndUtcDateTime();
          foreach (IWorkOrderLine wol in wols) {
            if (wol.WorkOrder != null && wol.BeginDateTime.Value >= today) {
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
      listProductions.SelectedValues = data.Get<IList<IWorkOrderLine>>(Item.WOLS).Cast<object>().ToList();
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.WOLS, listProductions.SelectedValues.Cast<IWorkOrderLine>().ToList());
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<IList<IWorkOrderLine>>(Item.WOLS).Count == 0) {
        errors.Add("at least one production period must be selected");
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
      return null;
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      IList<IWorkOrderLine> wols = data.Get<IList<IWorkOrderLine>>(Item.WOLS);
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IWorkOrderLine wol in wols) {
            ModelDAOHelper.DAOFactory.WorkOrderLineDAO.Lock(wol);
            string txt = String.Format("{0} ({1} \u2192 {2})",
                                     wol.WorkOrder.Display,
                                     wol.BeginDateTime.Value.ToLocalTime().ToString("d"),
                                     wol.Deadline.ToLocalTime().ToString("d"));
            summary.Add(txt);
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
