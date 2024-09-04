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
  /// Description of Page0.
  /// </summary>
  internal partial class Page0 : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Line selection"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select the line in which you want to change the targets " +
          "of one or more production period(s)."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(ILine));
        return types;
      }
    }
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
    public void Initialize(ItemContext context)
    {
      // Line list
      listLine.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<ILine> lines = ModelDAOHelper.DAOFactory.LineDAO.FindAll();
          foreach (ILine line in lines) {
            listLine.AddItem(line.Display, line);
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
      // Select the line
      if (data.Get<ILine>(Item.LINE) != null) {
        listLine.SelectedValue = data.Get<ILine>(Item.LINE);
      }
      else {
        listLine.SelectedIndex = 0;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      if (listLine.SelectedValue != null) {
        ILine line = (ILine)listLine.SelectedValue;
        if (!Object.Equals(data.Get<ILine>(Item.LINE), line)) {
          data.Store(Item.WOS, new List<IWorkOrder>());
          data.Store(Item.PRODUCTIONS_CHANGED, true);
        }
        
        data.Store(Item.LINE, line);
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
      
      if (listLine.SelectedValue == null) {
        errors.Add("a line must be selected");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            ILine line = data.Get<ILine>(Item.LINE);
            ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
            if (ModelDAOHelper.DAOFactory.WorkOrderLineDAO.FindAllByLine(line).Count == 0) {
              errors.Add("the selected line must comprise at least one production period");
            }

            if (line.Components.Count == 0) {
              errors.Add("the selected line must produce at least one part");
            }
            else {
              int iwpCount = 0;
              foreach (IComponent component in line.Components) {
                iwpCount += component.ComponentIntermediateWorkPieces.Count;
              }

              if (iwpCount == 0) {
                errors.Add("the selected line must comprise at least one operation");
              }
            }
          }
        }
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
      return "Page1";
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
          ILine line = data.Get<ILine>(Item.LINE);
          ModelDAOHelper.DAOFactory.LineDAO.Lock(line);
          summary.Add(line.Display);
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
