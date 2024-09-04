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
  /// Select an existing machine
  /// </summary>
  internal partial class PageMachine : GenericWizardPage, IWizardPage
  {
    #region Members
    bool m_modified = true;
    #endregion // Members
    
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
        return "Select an existing machine which will be modified.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageMachine()
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
          // Existing machines
          var machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          foreach (var machine in machines) {
            if (machine.IsObsolete()) {
              list.AddItem(machine.Display + " (obsolete)", machine, machine.DisplayPriority.HasValue ?
                           machine.DisplayPriority.Value : -1, SystemColors.ControlDarkDark, true, false);
            }
            else {
              list.AddItem(machine.Display, machine, machine.DisplayPriority.HasValue ?
                           machine.DisplayPriority.Value : -1);
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
      m_modified = false;
      list.SelectedValue = data.Get<IMachine>(AbstractItem.MACHINE);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(AbstractItem.MACHINE, list.SelectedValue);
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
      
      if (data.Get<IMachine>(AbstractItem.MACHINE) == null) {
        errors.Add("a machine must be selected");
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
      if (m_modified || String.IsNullOrEmpty(data.Get<string>(AbstractItem.MACHINE_CODE))) {
        // Load the configuration of the machine
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            var machine = data.Get<IMachine>(AbstractItem.MACHINE);
            ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
            data.Store(AbstractItem.MACHINE_CODE, machine.Code);
            data.Store(AbstractItem.MACHINE_NAME, machine.Name);
            data.Store(AbstractItem.CATEGORY, machine.Category);
            data.Store(AbstractItem.SUBCATEGORY, machine.SubCategory);
            data.Store(AbstractItem.DEPARTMENT, machine.Department);
            data.Store(AbstractItem.COMPANY, machine.Company);
            data.Store(AbstractItem.CELL, machine.Cell);
            data.Store(AbstractItem.MACHINE_PRIORITY, machine.DisplayPriority.HasValue ?
                       (double)machine.DisplayPriority.Value + .5 : -1.0);
          }
        }
      }
      
      return "PageIdentification";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<IMachine>(AbstractItem.MACHINE) == null) {
        summary.Add("none");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            var machine = data.Get<IMachine>(AbstractItem.MACHINE);
            ModelDAOHelper.DAOFactory.MachineDAO.Lock(machine);
            summary.Add("\"" + machine.Display + "\"");
          }
        }
      }
      
      return summary;
    }
    
    void ListItemChanged(string arg1, object arg2)
    {
      m_modified = true;
    }
    #endregion // Page methods
  }
}
