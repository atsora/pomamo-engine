// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardAddReason
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
    public string Title { get { return "Machine modes"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select here the machine modes that will be associated with the new reason.\n\n" +
          "Note: a machine mode is a state that has been recognized based on the information the machine sends."; } }

    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IMachineMode));
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
      machineModeSelection.SelectedMachineModes = data.Get<IList<IMachineMode>>(Item.MACHINE_MODES);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.MACHINE_MODES, machineModeSelection.SelectedMachineModes);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (data.Get<IList<IMachineMode>>(Item.MACHINE_MODES).Count == 0) {
        errors.Add("at least one machine mode must be selected");
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
          IList<IMachineMode> machineModes = data.Get<IList<IMachineMode>>(Item.MACHINE_MODES);
          foreach (IMachineMode machineMode in machineModes) {
            ModelDAOHelper.DAOFactory.MachineModeDAO.Lock(machineMode);
            summary.Add(machineMode.Display);
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
