// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardEventLongPeriod
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
    public string Help { get { return "Select here the machine modes associated " +
          "with the long period events you want to create. Each machine mode is flagged " +
          "as an idle or running period.\n\n" +
          "It is possible to include all items with the \"All\" check-box."; } }

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
      // List of all machine modes
      verticalScroll.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var machineModes = ModelDAOHelper.DAOFactory.MachineModeDAO.FindAll();
          (machineModes as List<IMachineMode>).Sort((x,y) => {
                                                      if (object.Equals (x.Running, y.Running)) {
              return x.Display.CompareTo(y.Display);
            }
            else {
                                                        if (!x.Running.HasValue) {
                                                          return -1;
                                                        }
                                                        if (!y.Running.HasValue) {
                                                          return 1;
                                                        }
                                                        return x.Running.Value.CompareTo(y.Running.Value);
                                                      }
                                                    });
          foreach (IMachineMode machineMode in machineModes) {
            var cell = new MachineModeCell(machineMode);
            cell.Dock = DockStyle.Fill;
            verticalScroll.AddControl(cell);
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
      IList<IMachineMode> machineModes = data.Get<IList<IMachineMode>>(Item.MACHINE_MODES);
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as MachineModeCell;
        cell.Checked = machineModes.Contains(cell.MachineMode);
      }
      checkAll.Checked = data.Get<bool>(Item.ALL_MACHINE_MODES);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      IList<IMachineMode> machineModes = new List<IMachineMode>();
      foreach (Control c in verticalScroll.ControlsInLayout) {
        var cell = c as MachineModeCell;
        if (cell.Checked) {
          machineModes.Add(cell.MachineMode);
        }
      }
      data.Store(Item.MACHINE_MODES, machineModes);
      data.Store(Item.ALL_MACHINE_MODES, checkAll.Checked);
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      if (!data.Get<bool>(Item.ALL_MACHINE_MODES) &&
          data.Get<IList<IMachineMode>>(Item.MACHINE_MODES).Count == 0) {
        errors.Add("at least one machine mode must be selected");
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
      return "Page3";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      if (data.Get<bool>(Item.ALL_MACHINE_MODES)) {
        summary.Add("All");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            IList<IMachineMode> machineModes = data.Get<IList<IMachineMode>>(Item.MACHINE_MODES);
            foreach (IMachineMode machineMode in machineModes) {
              ModelDAOHelper.DAOFactory.MachineModeDAO.Lock(machineMode);
              summary.Add(machineMode.Display);
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
      verticalScroll.Enabled = !checkAll.Checked;
    }
    #endregion // Event reactions
  }
}
