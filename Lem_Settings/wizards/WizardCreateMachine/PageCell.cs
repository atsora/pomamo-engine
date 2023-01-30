// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardCreateMachine
{
  /// <summary>
  /// Description of PageCell.
  /// </summary>
  internal partial class PageCell : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Cell"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "The machine can by placed in an existing cell (not mandatory)."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(ICell));
        return types;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageCell()
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
      listCell.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<ICell> cells = ModelDAOHelper.DAOFactory.CellDAO.FindAll();
          foreach (ICell cell in cells) {
            string display = cell.Display;
            listCell.AddItem(display, cell, cell.DisplayPriority);
          }
        }
      }
      listCell.InsertItem("none", null, 0, SystemColors.ControlText, true, false);
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      listCell.SelectedValue = data.Get<ICell>(AbstractItem.CELL);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(AbstractItem.CELL, listCell.SelectedValue);
    }
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName(ItemData data)
    {
      bool firstMachine = false;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        firstMachine = (ModelDAOHelper.DAOFactory.MachineDAO.FindAll().Count == 0);
      }
      
      if (firstMachine) {
        return null; // no order
      }
      else {
        return "PageOrder";
      }
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<ICell>(AbstractItem.CELL) == null) {
        summary.Add("none");
      }
      else
      {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            ICell cell = data.Get<ICell>(AbstractItem.CELL);
            ModelDAOHelper.DAOFactory.CellDAO.Lock(cell);
            summary.Add("\"" + cell.Display+ "\"");
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
