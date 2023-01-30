// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardChangeProductionQuantities
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  public partial class Page1 : GenericWizardPage, IWizardPage
  {
    #region Members
    ILine m_line = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Production period selection"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose here the production period you want to change.\n\n" +
          "You can select several items but in that case take care the periods match."; } }
    
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
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_line = data.Get<ILine>(Item.LINE);
      FillAndSelectWols(data.Get<List<IWorkOrder>>(Item.WOS));
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      IList<IWorkOrder> list1 = data.Get<List<IWorkOrder>>(Item.WOS);
      IList<IWorkOrder> list2 = listProductions.SelectedValues.Cast<IWorkOrder>().ToList();
      
      if (list1.Count != list2.Count) {
        data.Store(Item.PRODUCTIONS_CHANGED, true);
      }
      else {
        foreach (IWorkOrder wo in list1) {
          if (!list2.Contains(wo)) {
            data.Store(Item.PRODUCTIONS_CHANGED, true);
            break;
          }
        }
      }
      
      data.Store(Item.WOS, list2);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<IList<IWorkOrder>>(Item.WOS).Count == 0) {
        errors.Add("at least one production period must be selected");
      }

      if (data.Get<bool>(Item.PRODUCTIONS_CHANGED)) {
        data.Get<Quantities>(Item.QUANTITIES).Init(data.Get<ILine>(Item.LINE),
                                                   data.Get<IList<IWorkOrder>>(Item.WOS));
        data.Store(Item.PRODUCTIONS_CHANGED, false);
        data.Get<Quantities>(Item.QUANTITIES).GetErrors(ref errors);
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
      return "Page2";
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
          foreach (IWorkOrder wo in data.Get<IList<IWorkOrder>>(Item.WOS)) {
            ModelDAOHelper.DAOFactory.WorkOrderDAO.Lock(wo);
            summary.Add(wo.Display);
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
    
    #region Private methods
    void FillAndSelectWols(IList<IWorkOrder> wos)
    {
      // List of productions
      listProductions.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.LineDAO.Lock(m_line);
          IList<IWorkOrderLine> wols = ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAllByLine(m_line);
          DateTime today = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetTodayBeginUtcDateTime();
          foreach (IWorkOrderLine wol in wols) {
            if (wol.WorkOrder != null && (wol.Deadline >= today || checkShowAll.Checked)) {
              string txt = wol.WorkOrder.Display;
              if (txt != "") {
                txt += ", ";
              }

              txt += wol.BeginDateTime.Value.ToLocalTime().ToShortDateString() + " → " +
                wol.Deadline.ToLocalTime().ToShortDateString() + ", target: " + wol.Quantity + " part" +
                (wol.Quantity > 1 ? "s" : "");
              listProductions.AddItem(txt, wol.WorkOrder, wol.BeginDateTime.Value);
            }
          }
        }
      }
      
      // Select the productions
      if (wos.Any()) {
        listProductions.SelectedValues = wos.Cast<object>().ToList();
      }
      else {
        listProductions.SelectedIndex = 0;
      }
    }
    #endregion // Private methods
    
    #region Event reactions
    void CheckShowAllCheckedChanged(object sender, EventArgs e)
    {
      FillAndSelectWols(listProductions.SelectedValues.Cast<IWorkOrder>().ToList());
    }
    #endregion // Event reactions
  }
}
