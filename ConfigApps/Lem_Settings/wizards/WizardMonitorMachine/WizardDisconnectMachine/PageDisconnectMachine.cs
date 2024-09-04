// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Select an existing machine
  /// </summary>
  internal partial class PageDisconnectMachine : GenericWizardPage, IWizardPage
  {
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Machine"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help {
      get {
        return "Select a monitored machine that will be flagged as obsolete.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageDisconnectMachine()
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
      list.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Machines
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          IList<IMachineModule> mamos = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindAll();
          foreach (var machine in machines) {
            string name = machine.Name + (String.IsNullOrEmpty(machine.Code) ? "" : (
              " (" + machine.Code + ")"));
            
            bool isMonitored = false;
            foreach (var mamo in mamos) {
              if (mamo.MonitoredMachine != null && mamo.MonitoredMachine.Id == machine.Id &&
                  mamo.CncAcquisition != null) {
                isMonitored = true;
                break;
              }
            }
            
            if (isMonitored) {
              list.AddItem(name, machine, name);
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
      list.SelectedValue = data.Get<IMachine>(ItemDisconnect.MACHINE);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(ItemDisconnect.MACHINE, list.SelectedValue);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      var errors = new List<string>();
      
      if (data.Get<IMachine>(ItemDisconnect.MACHINE) == null) {
        errors.Add("the machine has not been selected");
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
      
      if (data.Get<IMachine>(Item.MACHINE) == null) {
        summary.Add("none");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            var machine = data.Get<IMachine>(Item.MACHINE);
            ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
            summary.Add("\"" + machine.Display + "\"");
          }
        }
      }
      
      return summary;
    }
    #endregion // Page methods
  }
}
