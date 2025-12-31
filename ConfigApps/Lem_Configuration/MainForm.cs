// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.FileRepository;
using Lemoine.I18N;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using Lemoine.ModelDAO.Interfaces;
using Lemoine.Info.ConfigReader.TargetSpecific;
using System.Threading.Tasks;

namespace Lem_Configuration
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    readonly bool m_advancedMode = false;
    bool m_availableOptionsLoaded = false;
    readonly IDictionary<CheckBox, string> availabilityCheckBoxes =
      new Dictionary<CheckBox, string> ();

    static readonly ILog log = LogManager.GetLogger (typeof (MainForm).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MainForm (bool advancedMode = false)
    {
      log.Debug ("MainForm");

      m_advancedMode = advancedMode;

      InitializeComponent ();
    }

    void MainFormLoad (object sender, EventArgs e)
    {
      // - i18n
      generalTabPage.Text = PulseCatalog.GetString ("GeneralConfig");
      availableOptionsTabPage.Text = PulseCatalog.GetString ("Lem_ConfigurationAvailableOptions");
      dataStructureTabPage.Text = PulseCatalog.GetString ("DataStructureConfig");
      displayTabPage.Text = PulseCatalog.GetString ("DisplayConfig");
      analysisTabPage.Text = PulseCatalog.GetString ("AnalysisConfig");
      shiftTabPage.Text = PulseCatalog.GetString ("Shift");
      computerTabPage.Text = PulseCatalog.GetString ("Computer");
      dayTemplateTabPage.Text = PulseCatalog.GetString ("Day templates");
      shiftTemplateTabPage.Text = PulseCatalog.GetString ("Shift templates");

      machineConfigsTabPage.Text = PulseCatalog.GetString ("Machine");
      companyTabPage.Text = PulseCatalog.GetString ("Company");
      departmentTabPage.Text = PulseCatalog.GetString ("Department");
      cellTabPage.Text = PulseCatalog.GetString ("Cell");
      machineCategoryTabPage.Text = PulseCatalog.GetString ("MachineCategory");
      machineSubCategoryTabPage.Text = PulseCatalog.GetString ("MachineSubCategory");
      machineTabPage.Text = PulseCatalog.GetString ("Machine");
      machineFilterTabPage.Text = PulseCatalog.GetString ("MachineFilter");
      machineStateTemplateTab.Text = PulseCatalog.GetString ("MachineStateTemplate");
      machineStateTemplateRightTab.Text = PulseCatalog.GetString ("MachineStateTemplateRight");

      acquisitionTabPage.Text = PulseCatalog.GetString ("Acquisition");
      machineModuleTabPage.Text = PulseCatalog.GetString ("MachineModule");
      monitoredMachineTabPage.Text = PulseCatalog.GetString ("MonitoredMachine");
      cncAcquisitionTabPage.Text = PulseCatalog.GetString ("CncAcquisition");
      cncConfigTabPage.Text = PulseCatalog.GetString ("CncConfig");

      machineStatusTabPage.Text = PulseCatalog.GetString ("MachineStatusConfig");
      machineModeTabPage.Text = PulseCatalog.GetString ("MachineModeConfig");
      machineObservationStateTabPage.Text = PulseCatalog.GetString ("MachineObservationStateConfig");
      reasonGroupTabPage.Text = PulseCatalog.GetString ("ReasonGroup");
      reasonTabPage.Text = PulseCatalog.GetString ("Reason");
      reasonSelectionTabPage.Text = PulseCatalog.GetString ("ReasonSelection");
      defaultReasonTabPage.Text = PulseCatalog.GetString ("DefaultReason");
      autoMachineStateTemplateTabPage.Text = PulseCatalog.GetString ("AutoMachineStateTemplate");
      productionStateTabPage.Text = PulseCatalog.GetString ("ProductionState");

      interfacesTabPage.Text = PulseCatalog.GetString ("ConfigInterfaces");
      fieldLegendTabPage.Text = PulseCatalog.GetString ("FieldLegend");
      guiConfigTabPage.Text = PulseCatalog.GetString ("GuiConfig");
      machineStateTemplateFlowTabPage.Text = PulseCatalog.GetString ("MachineStateTemplateFlow");

      eventTabPage.Text = PulseCatalog.GetString ("Event");
      eventLongPeriodConfigTabPage.Text = PulseCatalog.GetString ("EventLongPeriodConfig");
      eventCnCValueConfigTabPage.Text = PulseCatalog.GetString ("EventCnCValueConfig");
      eventToolLifeConfigTabPage.Text = PulseCatalog.GetString ("EventToolLifeConfig");
      eventSMTPTabPage.Text = PulseCatalog.GetString ("EventSMTPConfig");
      eventEmailConfigTabPage.Text = PulseCatalog.GetString ("EventEmailConfig");

      qualityTabPage.Text = PulseCatalog.GetString ("Quality");
      nonConformanceReasonTabPage.Text = PulseCatalog.GetString ("NonConformanceReason");

      mainUserTabPage.Text = PulseCatalog.GetString ("User");
      userTabPage.Text = PulseCatalog.GetString ("User");

      // - tabPage display
      if (!m_advancedMode) {
        CheckTabPageVisibility (this);
      }

      if (availableOptionsTabPage.Visible) {
        FillAvailableOptions ();
      }

      // - Observer links between controls
      companyConfig1.AddObserver (machineConfig1);
      companyConfig1.AddObserver (machineFilterConfig1);
      departmentConfig1.AddObserver (machineConfig1);
      departmentConfig1.AddObserver (machineFilterConfig1);
      cellConfig1.AddObserver (machineConfig1);
      cellConfig1.AddObserver (machineFilterConfig1);
      machineCategoryConfig1.AddObserver (machineConfig1);
      machineCategoryConfig1.AddObserver (machineFilterConfig1);
      machineSubCategoryConfig1.AddObserver (machineConfig1);
      machineSubCategoryConfig1.AddObserver (machineFilterConfig1);
      reasonGroupConfig1.AddObserver (reasonConfig1);
      reasonConfig1.AddObserver (machineModeDefaultReasonConfig1);
      reasonConfig1.AddObserver (reasonSelectionConfig1);
      machineModeConfig1.AddObserver (machineModeDefaultReasonConfig1);
      machineModeConfig1.AddObserver (reasonSelectionConfig1);
      machineModeConfig1.AddObserver (eventLongPeriodConfigConfig1);
      machineObservationStateConfig1.AddObserver (machineModeDefaultReasonConfig1);
      machineObservationStateConfig1.AddObserver (reasonSelectionConfig1);
      machineObservationStateConfig1.AddObserver (eventLongPeriodConfigConfig1);
      machineFilterConfig1.AddObserver (machineModeDefaultReasonConfig1);
      machineFilterConfig1.AddObserver (reasonSelectionConfig1);
      machineFilterConfig1.AddObserver (emailConfigConfig);
      machineModuleConfig1.AddObserver (monitoredMachineConfig1);
      monitoredMachineConfig1.AddObserver (cncAcquisitionConfig1);
      monitoredMachineConfig1.AddObserver (machineModuleConfig1);
      monitoredMachineConfig1.AddObserver (eventLongPeriodConfigConfig1);
      cncAcquisitionConfig1.AddObserver (machineModuleConfig1);
      fieldConfig1.AddObserver (monitoredMachineConfig1);
      unitConfig1.AddObserver (fieldConfig1);
      monitoredMachineConfig1.AddObserver (machineConfig1);
      fieldConfig1.AddObserver (fieldLegendConfig1);
      smtpConfig.AddObserver (smtpStatusStrip);
      machineStateTemplateConfig1.AddObserver (machineStateTemplateRightConfig1);
      eventLevelConfig.AddObserver (eventLongPeriodConfigConfig1);
      eventLevelConfig.AddObserver (eventCncValueConfigConfig);
      eventLevelConfig.AddObserver (eventToolLifeConfigConfig);
      eventLevelConfig.AddObserver (emailConfigConfig);
      machineStateTemplateConfig1.AddObserver (machineStateTemplateFlowConfig);
    }

    void CheckTabPageVisibility (Control control)
    {
      if (control is TabControl) {
        TabControl tabControl = control as TabControl;
        foreach (TabPage tabPage in tabControl.TabPages) {
          log.Info ("tabControl.tabPage is " + tabPage.Name);
          log.Info ("tabControl.tabPage config key is " + GetConfigKey (tabPage.Name));
        }

        List<TabPage> toRemoveList = new List<TabPage> ();

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          foreach (TabPage tabPage in tabControl.TabPages) {
            var config = ModelDAOHelper.DAOFactory.ConfigDAO
              .GetConfig (GetConfigKey (tabPage.Name));
            if ((null == config) || !((bool)config.Value)) {
              toRemoveList.Add (tabPage);
            }
          }
        }

        foreach (TabPage tabPage in toRemoveList) {
          tabControl.TabPages.Remove (tabPage);
        }
      }
      foreach (Control subControl in control.Controls) {
        CheckTabPageVisibility (subControl);
      }
    }

    void FillAvailableOptions ()
    {
      if (!m_availableOptionsLoaded) {
        m_availableOptionsLoaded = true;
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          AddTabPageOption (this);
        }
      }
    }

    void AddTabPageOption (Control control)
    {
      if (control is TabControl) {
        TabControl tabControl = control as TabControl;
        foreach (TabPage tabPage in tabControl.TabPages) {
          CheckBox checkBox = new CheckBox ();
          availabilityCheckBoxes[checkBox] = tabPage.Name;
          checkBox.Text = tabPage.Text;
          checkBox.Width = 200;
          checkBox.Parent = availableOptionsFlowLayoutPanel;
          checkBox.CheckedChanged += new EventHandler (checkBox_CheckedChanged);
          var config = ModelDAOHelper.DAOFactory.ConfigDAO
            .GetConfig (GetConfigKey (tabPage.Name));
          if ((null != config) && (bool)config.Value) {
            checkBox.Checked = true;
          }
          availableOptionsTabPage.Refresh ();
        }
      }
      foreach (Control subControl in control.Controls) {
        AddTabPageOption (subControl);
      }
    }

    string GetConfigKey (string tabPageName)
    {
      return string.Format ("Lem_Configuration.{0}.Availability", tabPageName);
    }

    async void checkBox_CheckedChanged (object sender, EventArgs e)
    {
      CheckBox checkBox = sender as CheckBox;
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        var config = ModelDAOHelper.DAOFactory.ConfigDAO
          .GetConfig (GetConfigKey (availabilityCheckBoxes[checkBox]));
        if (null == config) {
          if (checkBox.Checked) {
            config = ModelDAOHelper.ModelFactory.CreateConfig (GetConfigKey (availabilityCheckBoxes[checkBox]));
            config.Value = true;
            await ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistentAsync (config);
          }
        }
        else {
          if (checkBox.Checked) {
            config.Value = true;
          }
          else {
            await ModelDAOHelper.DAOFactory.ConfigDAO.MakeTransientAsync (config);
          }
        }
        transaction.Commit ();
      }
    }

    private void MainForm_FormClosed (object sender, FormClosedEventArgs e)
    {
      Application.Exit ();
    }
  }
}
