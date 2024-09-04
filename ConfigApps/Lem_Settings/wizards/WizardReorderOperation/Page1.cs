// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardReorderOperation
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
    public string Title { get { return "Part"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Choose the part whose operations have to be reordered."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags { get { return LemSettingsGlobal.PageFlag.NONE; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IPart));
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
      // Load the different parts
      listParts.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        IList<IPart> parts = ModelDAOHelper.DAOFactory.PartDAO.FindAll();
        foreach (IPart part in parts) {
          string txt = part.Display + " (";
          if (part.ComponentIntermediateWorkPieces == null || part.ComponentIntermediateWorkPieces.Count == 0) {
            txt += "no operations)";
          }
          else if (part.ComponentIntermediateWorkPieces.Count == 1) {
            txt += "only one operation)";
          }
          else {
            txt += part.ComponentIntermediateWorkPieces.Count + " operations)";
          }

          listParts.AddItem(txt, part);
        }
      }
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // Select the part
      listParts.SelectedValue = data.Get<IPart>(Item.PART);
      if (listParts.SelectedIndex == -1) {
        listParts.SelectedIndex = 0;
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.PART, listParts.SelectedValue);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<IPart>(Item.PART) == null) {
        errors.Add("a part must be selected");
      }
      else {
        data.Get<Order>(Item.ORDER).Load(data.Get<IPart>(Item.PART));
        data.Get<Order>(Item.ORDER).GetErrors(ref errors);
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
      data.Get<Order>(Item.ORDER).GetWarnings(ref warnings);
      return warnings;
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
          IPart part = data.Get<IPart>(Item.PART);
          ModelDAOHelper.DAOFactory.PartDAO.Lock(part);
          summary.Add("\"" + part.Display + "\"");
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
