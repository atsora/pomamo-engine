// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardReasonSelection
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  internal partial class Page3 : GenericWizardPage, IWizardPage
  {
    #region Members
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Machine observation states"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select here the machine observation states that will be associated with new reasons.\n\n" +
          "Note: a machine observation state is a state that is planned and expected."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IMachineObservationState));
        return types;
      }
    }
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
    public void Initialize(ItemContext context)
    {
      // List of all machine observation states
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachineObservationState> moss = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindAll();
          foreach (IMachineObservationState mos in moss) {
            listMoss.AddItem(mos.Display, mos);
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
      listMoss.SelectedValues = data.Get<IList<IMachineObservationState>>(Item.MOSS).Cast<object>().ToList();
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.MOSS, listMoss.SelectedValues.Cast<IMachineObservationState>().ToList());
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<IList<IMachineObservationState>>(Item.MOSS).Count == 0) {
        errors.Add("at least one machine observation state must be selected");
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
      
      int count = 0;
      int countDefault = 0;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachineMode> mamos = new List<IMachineMode>();
          mamos.Add(Item.GetAndLockMachineMode(data));
          IList<IMachineObservationState> moss = Item.GetAndLockMachineObservationStates(data);
          count = ModelDAOHelper.DAOFactory.ReasonSelectionDAO
            .FindWithForConfig (mamos, moss).Count;
          countDefault = ModelDAOHelper.DAOFactory.MachineModeDefaultReasonDAO
            .FindWithForConfig (mamos, moss).Count;
        }
      }
      
      if (count > 0 || countDefault > 0) {
        warnings.Add(String.Format("{2} default reason{3} and {0} selected reason{1} will be deleted before adding new ones.",
                                   count, count > 1 ? "s" : "", countDefault, countDefault > 1 ? "s" : ""));
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
      return "PageDefaultReason";
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
          IList<IMachineObservationState> moss = data.Get<IList<IMachineObservationState>>(Item.MOSS);
          foreach (IMachineObservationState mos in moss) {
            ModelDAOHelper.DAOFactory.MachineObservationStateDAO.Lock(mos);
            summary.Add(mos.Display);
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
