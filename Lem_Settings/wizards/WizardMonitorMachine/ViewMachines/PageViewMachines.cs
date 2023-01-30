// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Description of PageViewMachines.
  /// </summary>
  internal partial class PageViewMachines : GenericViewPage, IViewPage
  {
    #region Members
    readonly IDictionary<string, IList<IMachine>> m_machinesPerConfigFiles =
      new Dictionary<string, IList<IMachine>>();
    bool m_advancedMode = false;
    ItemData m_itemData = null;
    int m_machineId = -1;
    IDictionary<int, string> m_machineSuffix = new Dictionary<int, string>();
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Machines"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help
    {
      get {
        return "Display the cnc configuration of the machine " +
          "selected in the combobox. You can filter the machines per configuration file.\n\n" +
          "When you select a machine, the different modules are listed. " +
"Messages in red will be displayed if problems are detected.";
      }
    }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags
    {
      get {
        return LemSettingsGlobal.PageFlag.NONE;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageViewMachines()
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
      m_advancedMode = context.UserCategory == LemSettingsGlobal.UserCategory.SUPER_ADMIN;
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_itemData = data;
      
      // Get all machines with their respective principal cnc acquisition file
      m_machineSuffix.Clear();
      m_machinesPerConfigFiles.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          // Get all machines and machine modules
          var machines = ModelDAOHelper.DAOFactory.MachineDAO.FindAll();
          IList<IMachineModule> mamos = ModelDAOHelper.DAOFactory.MachineModuleDAO.FindAll();
          
          foreach (var machine in machines) {
            int monitoringType = 3;
            bool withConfig = false;
            string configFile = "";
            foreach (var mamo in mamos) {
              if (mamo.MonitoredMachine != null && mamo.MonitoredMachine.Id == machine.Id) {
                monitoringType = mamo.MonitoredMachine.MonitoringType == null ?
                  3 : mamo.MonitoredMachine.MonitoringType.Id;
                withConfig |= (mamo.CncAcquisition != null);
                if (mamo.MonitoredMachine != null && mamo.MonitoredMachine.MainCncAcquisition != null) {
                  configFile = mamo.MonitoredMachine.MainCncAcquisition.ConfigFile;
              }
            }
            }
            
            // Compute the name suffix
            string suffix = PageMachine.GetSuffix(monitoringType, withConfig);
            if (!string.IsNullOrEmpty (suffix)) {
              m_machineSuffix[machine.Id] = suffix;
            }
            
            // Store the machine with its configuration file
            AddMachine(string.IsNullOrEmpty(configFile) ? "-" : configFile, machine);
          }
        }
      }
      
      // Fill the different acquisition files
      comboConfFile.ClearItems ();
      foreach (string xmlFile in m_machinesPerConfigFiles.Keys) {
        if (xmlFile != "-") {
          comboConfFile.AddItem (xmlFile, xmlFile);
        }
      }

      // Add "no file" if needed and "all"
      if (m_machinesPerConfigFiles.Keys.Contains ("-")) {
        comboConfFile.InsertItem("no file", "-", 0);
      }

      comboConfFile.InsertItem("all", "", 0);

      // Select the first element
      comboConfFile.SelectedIndex = 0;
      
      // Selected machine?
      var selectedMachine = m_itemData.Get<IMachine>(ItemView.MACHINE);
      if (selectedMachine != null) {
        comboMachine.SelectedValue = selectedMachine;
    }
    }
    
    void AddMachine(string configFile, IMachine machine)
    {
      if (!m_machinesPerConfigFiles.ContainsKey (configFile)) {
        m_machinesPerConfigFiles[configFile] = new List<IMachine>();
      }

      m_machinesPerConfigFiles[configFile].Add(machine);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Nothing
    }
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    public override IList<string> GetErrorsBeforeValidation(ItemData data)
    {
      return null;
    }
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revisionId">Revision that is going to be applied when the function returns (not used for views)</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      // Nothing
    }
    
    /// <summary>
    /// Method called every second
    /// </summary>
    public override void OnTimeOut()
    {
      // Update machine state
      if (m_machineId != -1) {
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(m_machineId);
            if (machine != null) {
              // Current machine mode
              var current = ModelDAOHelper.DAOFactory.CurrentMachineModeDAO.FindWithMachineMode(machine);
              if (current != null && current.MachineMode != null) {
                labelModeCurrent.Text = MachineModeToString(current.MachineMode);
                labelCurrentDateTime.Text = current.DateTime.ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss");
                markerCurrent.Brush = new SolidBrush(MachineModeToColor(current.MachineMode));
              }
              else {
                InitCurrentMode ();
              }

              // Current fact
              var fact = ModelDAOHelper.DAOFactory.FactDAO.GetLast(machine);
              if (fact != null && fact.CncMachineMode != null) {
                labelModeFact.Text = MachineModeToString(fact.CncMachineMode);
                labelFactDateTime.Text = fact.End.ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss");
                markerFact.Brush = new SolidBrush(MachineModeToColor(fact.CncMachineMode));
              }
              else {
                InitFact ();
              }

              // Machine status
              var status = ModelDAOHelper.DAOFactory.MachineStatusDAO.FindByIdWithMachineMode(machine.Id);
              if (status != null && status.MachineMode != null) {
                labelModeAnalysed.Text = MachineModeToString(status.MachineMode);
                labelAnalysedDateTime.Text = status.ReasonSlotEnd.ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss");
                markerAnalysis.Brush = new SolidBrush(MachineModeToColor(status.MachineMode));
              }
              else {
                InitAnalysedMode ();
              }
            }
            else {
              InitCurrentMode();
              InitFact();
              InitAnalysedMode();
            }
          }
        }
      }
      else {
        InitCurrentMode();
        InitAnalysedMode();
      }
    }
    #endregion // Page methods
    
    #region Event reactions
    void ComboConfFileItemChanged(string arg1, object arg2)
    {
      comboMachine.ClearItems();
      string fileName = arg2 as string;
      
      if (string.IsNullOrEmpty(fileName)) {
        // Display all machines
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            foreach (var machines in m_machinesPerConfigFiles.Values) {
              foreach (var machine in machines) {
                AddMachineInCombobox (machine);
              }
            }
          }
        }
      } else if (m_machinesPerConfigFiles.ContainsKey(fileName)) {
        // Display machine for a specific xml file
        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
          using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
            foreach (IMachine machine in m_machinesPerConfigFiles[fileName]) {
              AddMachineInCombobox(machine);
          }
        }
      }
      }
      
      if (comboMachine.Count > 0) {
        comboMachine.SelectedIndex = 0;
      }
      else {
        verticalScroll.Visible = false;
    }
    }
    
    void ComboMachineItemChanged(string arg1, object arg2)
    {
      // Display the cnc configuration of a machine
      verticalScroll.Clear();
      bool hasMachineModule = false;
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var machine = arg2 as IMonitoredMachine;
          if (machine != null) {
            AddCell(machine);
            hasMachineModule = true;
            m_machineId = machine.Id;
            OnTimeOut();
          }
          else {
            m_machineId = -1;
            if (m_itemData != null) {
              m_itemData.Store(ItemView.MACHINE, null);
          }
        }
      }
      }
      
      verticalScroll.Visible = hasMachineModule;
    }
    #endregion // Event reactions
    
    #region Private methods
    void AddMachineInCombobox(IMachine machine)
    {
      if (m_machineSuffix.Keys.Contains(machine.Id)) {
        string display = String.Format("{0} (id {1}){2}", machine.Name, machine.Id, m_machineSuffix[machine.Id]);
        comboMachine.AddItem(display, machine, display, SystemColors.ControlDarkDark, true, false);
      }
      else {
        string display = String.Format("{0} (id {1})", machine.Name, machine.Id);
        comboMachine.AddItem(display, machine, display);
      }
    }
    
    string MachineModeToString(IMachineMode machineMode)
    {
      string ret = "-";
      
      if (machineMode != null) {
        if (machineMode.Running.HasValue) {
          ret = machineMode.Running.Value ? "running" : "idle";
        }
        else {
          ret = "other";
      }
      }
      
      ret += " - " + machineMode.NameOrTranslation;
      
      return ret;
    }
    
    Color MachineModeToColor(IMachineMode machineMode)
    {
      Color colorRet = SystemColors.Control;
      try {
        colorRet = ColorTranslator.FromHtml(machineMode.Color);
      }
      catch {
      }
      return colorRet;
    }
    
    void InitCurrentMode()
    {
      labelModeCurrent.Text = "";
      labelCurrentDateTime.Text = "";
      markerCurrent.Brush = new SolidBrush(SystemColors.Control);
    }
    
    void InitFact()
    {
      labelModeFact.Text = "";
      labelFactDateTime.Text = "";
      markerFact.Brush = new SolidBrush(SystemColors.Control);
    }
    
    void InitAnalysedMode()
    {
      labelModeAnalysed.Text = "-";
      labelAnalysedDateTime.Text = "";
      markerAnalysis.Brush = new SolidBrush(SystemColors.Control);
    }
    
    void AddCell(IMonitoredMachine machine)
    {
      ModelDAOHelper.DAOFactory.MonitoredMachineDAO.Lock(machine);
      if (m_itemData != null) {
        m_itemData.Store(ItemView.MACHINE, machine);
      }

      foreach (var mamo in machine.MachineModules) {
        // Header
        var header = new Label();
        header.Text = String.Format("Module \"{0}\" (id {1})",
                                    String.IsNullOrEmpty(mamo.Name) ? "-" : mamo.Name,
                                    mamo.Id);
        header.Font = new Font(header.Font, FontStyle.Bold);
        header.Dock = DockStyle.Fill;
        header.Margin = new Padding(0);
        header.TextAlign = ContentAlignment.MiddleCenter;
        header.BackColor = LemSettingsGlobal.COLOR_SUBCATEGORY;
        header.ForeColor = Color.White;
        
        // Central cell
        Control center;
        string errors = "";
        if (mamo.CncAcquisition == null) {
          var label = new Label();
          label.Font = header.Font;
          label.ForeColor = SystemColors.ControlDark;
          label.Text = "No acquisition";
          label.TextAlign = ContentAlignment.MiddleCenter;
          center = label;
        }
        else {
          var cell = new MachineModuleCell(mamo, m_advancedMode);
          errors = cell.Errors;
          center = cell;
        }
        center.Dock = DockStyle.Fill;
        center.Margin = new Padding(0);
        
        verticalScroll.AddControl(header);
        verticalScroll.AddControl(center);
        
        // Footer
        if (!string.IsNullOrEmpty(errors)) {
          var footer = new Label();
          footer.Text = errors;
          footer.Height = (footer.Text.Split('\n').Length) * 25;
          footer.Font = new Font(center.Font, FontStyle.Bold);
          footer.Dock = DockStyle.Fill;
          footer.Margin = new Padding(3);
          footer.TextAlign = ContentAlignment.MiddleLeft;
          footer.ForeColor = LemSettingsGlobal.COLOR_ERROR;
          verticalScroll.AddControl(footer);
        }
      }
      
      if (machine.MachineModules.Count == 0) {
        var label = new Label();
        label.Font = new Font(label.Font, FontStyle.Bold);
        label.ForeColor = SystemColors.ControlDark;
        label.Text = "No modules";
        label.TextAlign = ContentAlignment.MiddleCenter;
        label.Dock = DockStyle.Fill;
        label.Margin = new Padding(0);
        verticalScroll.AddControl(label);
      }
    }
    #endregion // Private methods
  }
}
