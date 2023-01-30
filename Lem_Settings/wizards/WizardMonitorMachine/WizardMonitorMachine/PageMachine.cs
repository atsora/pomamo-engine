// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Lemoine.DataRepository;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Select an existing machine
  /// </summary>
  internal partial class PageMachine : GenericWizardPage, IWizardPage
  {
    #region Members
    bool m_modification = false;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Machine"; } }

    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Select an existing machine whose acquisition will be parametered.";
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageMachine ()
    {
      InitializeComponent ();
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize (ItemContext context)
    {
      list.ClearItems ();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          // Get all machines and machine modules
          IList<IMachine> machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll ();
          IList<IMachineModule> mamos = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindAll ();

          foreach (var machine in machines) {
            string name = machine.Name + (String.IsNullOrEmpty (machine.Code) ? "" : (
              " (" + machine.Code + ")"));

            int monitoringType = 3;
            bool withConfig = false;
            foreach (var mamo in mamos) {
              if (mamo.MonitoredMachine != null && mamo.MonitoredMachine.Id == machine.Id) {
                monitoringType = mamo.MonitoredMachine.MonitoringType == null ?
                  3 : mamo.MonitoredMachine.MonitoringType.Id;
                withConfig |= (mamo.CncAcquisition != null);
              }
            }

            // Compute the name suffix
            string suffix = GetSuffix (monitoringType, withConfig);

            // Add the machine in the list
            if (monitoringType != 2) {
              // Grey if not in monitored state
              list.AddItem (name + suffix, machine, name + suffix,
                           SystemColors.ControlDarkDark, true, false);
            }
            else {
              if (withConfig) {
                list.AddItem (name, machine, name);
              }
              else {
                // Red if monitored with no config
                list.AddItem (name + suffix, machine, name + suffix,
                             LemSettingsGlobal.COLOR_ERROR, false, true);
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Compute a machine suffix for a display
    /// </summary>
    /// <param name="monitoringType"></param>
    /// <param name="withConfig"></param>
    /// <returns></returns>
    public static string GetSuffix (int monitoringType, bool withConfig)
    {
      string suffix = "";
      switch (monitoringType) {
      case 1:
        suffix = " - undefined";
        break;
      case 3:
      case -1:
        suffix = " - not monitored";
        break;
      case 4:
        suffix = " - out source";
        break;
      case 5:
        suffix = " - obsolete";
        break;
      default:
        suffix = "";
        break;
      }

      if (withConfig) {
        if (!string.IsNullOrEmpty (suffix)) {
          suffix += " with config";
        }
      }
      else {
        if (string.IsNullOrEmpty (suffix)) {
          suffix = " - monitored with no config";
      }
      }

      return suffix;
    }

    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData (ItemData data)
    {
      m_modification = data.Get<bool> (Item.LOAD_PARAMETERS);
      list.SelectedValue = data.Get<IMachine> (Item.MACHINE);
    }

    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData (ItemData data)
    {
      data.Store (Item.MACHINE, list.SelectedValue);
    }

    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext (ItemData data)
    {
      var errors = new List<string> ();

      var machine = data.Get<IMachine> (Item.MACHINE);
      if (machine == null) {
        errors.Add ("the machine has not been selected");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            ModelDAOHelper.DAOFactory.MachineDAO.Lock (machine);

            // Find the corresponding monitored machine, if any
            var moma = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByMachine (machine);

            if (moma != null) {
              int acquisitionId = (moma.MainCncAcquisition == null) ? -1 : moma.MainCncAcquisition.Id;
              if (moma.MachineModules != null && moma.MachineModules.Count > 0) {
                foreach (var mamo in moma.MachineModules) {
                  if (mamo.CncAcquisition != null) {
                    if (acquisitionId == -1) {
                      acquisitionId = mamo.CncAcquisition.Id;
                    }
                    else if (mamo.CncAcquisition.Id != acquisitionId) {
                      errors.Add ("this wizard is not compatible with machines being monitored by more than 1 acquisition");
                      break;
                    }
                  }
                }
              }
            }
          }
        }
      }

      return errors;
    }

    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of warnings, can be null</returns>
    public override IList<string> GetWarnings (ItemData data)
    {
      // Reset "LOAD_PARAMETERS"
      data.Store (Item.LOAD_PARAMETERS, false);

      // Reset the old configuration
      data.Store (Item.OLD_XML_DATA_FOR_PARAM, null);
      data.Store (Item.OLD_XML_DATA_FOR_MODULE, null);

      // Machine already connected?
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
          var machine = data.Get<IMachine> (Item.MACHINE);
          ModelDAOHelper.DAOFactory.MachineDAO.Lock (machine);
          var moma = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindByMachine (machine);

          if (moma != null) {
            MemorizeOldConfiguration (data, moma);
          }

          if (m_modification) {
            if (moma != null) {
              LoadConfiguration (data, moma);
              data.Store (Item.LOAD_PARAMETERS, true);
            }
            else {
              RestoreConfiguration (data);
            }
          }
        }
      }

      return null;
    }

    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <returns>the class name of the next page</returns>
    public string GetNextPageName (ItemData data)
    {
      return "PageModule";
    }

    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary (ItemData data)
    {
      IList<string> summary = new List<string> ();

      if (data.Get<IMachine> (Item.MACHINE) == null) {
        summary.Add ("none");
      }
      else {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ()) {
            var machine = data.Get<IMachine> (Item.MACHINE);
            ModelDAOHelper.DAOFactory.MachineDAO.Lock (machine);
            summary.Add ("\"" + machine.Display + "\"");
          }
        }
      }

      return summary;
    }
    #endregion // Page methods

    #region Private methods
    void MemorizeOldConfiguration (ItemData data, IMonitoredMachine moma)
    {
      // Cnc acquisition
      ICncAcquisition acquisition = moma.MainCncAcquisition;
      if (acquisition != null) {
        ModelDAOHelper.DAOFactory.CncAcquisitionDAO.Lock (acquisition);
        var factory = new FileRepoFactory ("cncconfigs", acquisition.ConfigFile);

        try {
          var cncDoc = new CncDocument (acquisition.ConfigFile, factory.GetData (cancellationToken: System.Threading.CancellationToken.None));

          // Load the configuration from the database and keep it only if it is valid
          CncDocument.LoadResult val = cncDoc.Load (moma);
          if ((val & CncDocument.LoadResult.DIFFERENT_XML) == 0 &&
              (val & CncDocument.LoadResult.TOO_MANY_PARAMETERS) == 0 &&
              (val & CncDocument.LoadResult.WRONG_PARAMETERS) == 0) {
            data.Store (Item.OLD_XML_DATA_FOR_PARAM, cncDoc);
          }

          if ((val & CncDocument.LoadResult.TOO_FEW_MACHINE_MODULES) == 0 &&
              (val & CncDocument.LoadResult.TOO_MANY_MACHINE_MODULES) == 0 &&
              (val & CncDocument.LoadResult.DIFFERENT_MACHINE_MODULES) == 0) {
            data.Store (Item.OLD_XML_DATA_FOR_MODULE, cncDoc);
          }
        }
        catch (Exception) {
          // Cannot find the file
        }
      }
    }

    void LoadConfiguration (ItemData data, IMonitoredMachine moma)
    {
      if (moma.MachineModules.Count > 0) {
        ICncAcquisition acquisition = null;
        foreach (var mamo in moma.MachineModules) {
          if (mamo.CncAcquisition != null) {
            acquisition = mamo.CncAcquisition;
          }
        }

        if (acquisition != null) {
          ModelDAOHelper.DAOFactory.CncAcquisitionDAO.Lock (acquisition);
          data.Store (Item.CONFIG_FILE, acquisition.ConfigFile);
          data.Store (Item.COMPUTER, acquisition.Computer);
        }
        else {
          data.Store (Item.CONFIG_FILE, "");
          data.Store (Item.COMPUTER, null);
        }

        data.Store (Item.OPERATION_BAR, moma.OperationBar == OperationBar.Operation);
        data.Store (Item.FIELD, moma.PerformanceField);
      }
    }

    void RestoreConfiguration (ItemData data)
    {
      data.Store (Item.CONFIG_FILE, "");
      data.Store (Item.COMPUTER, null);
      data.Store (Item.OPERATION_BAR, true);
      data.Store (Item.FIELD, null);
    }
    #endregion // Private methods

    #region Event reactions
    void ListItemChanged (string arg1, object arg2)
    {
      // Parameters will be reloaded before going to the next page
      m_modification = true;
    }
    #endregion // Event reactions
  }
}
