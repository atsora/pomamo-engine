// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardEventLongPeriod
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
    public string Title { get { return "Monitored machines"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Select here the monitored machines associated " +
          "with the long period events you want to create.\n\n" +
          "It is possible to include all items with the \"All\" check-box."; } }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IMachine));
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
      // List of all monitored machines
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMonitoredMachine> machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindAll();
          foreach (IMonitoredMachine machine in machines) {
            if (!machine.IsObsolete()) {
              listMachines.AddItem(machine.Display, machine);
            }
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
      listMachines.SelectedValues = data.Get<IList<IMonitoredMachine>>(Item.MONITORED_MACHINES).Cast<object>().ToList();
      checkAll.Checked = data.Get<bool>(Item.ALL_MONITORED_MACHINES);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.MONITORED_MACHINES, listMachines.SelectedValues.Cast<IMonitoredMachine>().ToList());
      data.Store(Item.ALL_MONITORED_MACHINES, checkAll.Checked);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (!data.Get<bool>(Item.ALL_MONITORED_MACHINES) &&
          data.Get<IList<IMonitoredMachine>>(Item.MONITORED_MACHINES).Count == 0) {
        errors.Add("at least one monitored machine must be selected");
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
      
      if (data.Get<bool>(Item.ALL_MONITORED_MACHINES)) {
        summary.Add("All");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IList<IMonitoredMachine> machines = data.Get<IList<IMonitoredMachine>>(Item.MONITORED_MACHINES);
            foreach (IMonitoredMachine machine in machines) {
              ModelDAOHelper.DAOFactory.MonitoredMachineDAO.Lock(machine);
              summary.Add(machine.Display);
            }
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
    
    #region Event reactions
    void CheckAllCheckedChanged(object sender, EventArgs e)
    {
      listMachines.Enabled = !checkAll.Checked;
    }
    #endregion // Event reactions
  }
}
