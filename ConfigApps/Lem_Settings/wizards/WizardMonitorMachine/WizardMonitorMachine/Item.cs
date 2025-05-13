// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.ModelDAO;
using Lemoine.Settings;
using Lemoine.Model;
using System.Linq;

namespace WizardMonitorMachine
{
  /// <summary>
  /// Add or change the way a machine is monitored
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    /// <summary>
    /// Activate the new configuration key params
    /// </summary>
    static readonly string KEY_PARAMS_KEY = "Config.CncAcquisition.KeyParams";
    static readonly bool KEY_PARAMS_DEFAULT = true;

    internal const string MACHINE = "machine";
    internal const string CONFIG_FILE = "xml_file";
    internal const string LOAD_PARAMETERS = "load_parameters";
    internal const string XML_DATA = "xml_data";
    internal const string OLD_XML_DATA_FOR_PARAM = "old_xml_data_for_param";
    internal const string OLD_XML_DATA_FOR_MODULE = "old_xml_data_for_module";
    internal const string OPERATION_BAR = "operation_bar";
    internal const string FIELD = "field";
    internal const string COMPUTER = "computer";

    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Monitor machine"; } }

    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description
    {
      get {
        return "With this item you can connect or change the connection configuration " +
          "of a machine. This is usually used after you create a new machine.";
      }
    }

    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords
    {
      get {
        return new String[] { "machines", "monitored", "monitoring", "stamping" };
      }
    }

    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "monitor_machine"; } }

    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Machines"; } }

    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Configuration"; } }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags
    {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
      }
    }

    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types
    {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType> ();
        dic[typeof (IMachine)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof (ICncAcquisition)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof (IMachineModule)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        dic[typeof (IComputer)] = LemSettingsGlobal.InteractionType.SECONDARY;
        return dic;
      }
    }

    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages
    {
      get {
        IList<IWizardPage> pages = new List<IWizardPage> ();
        pages.Add (new PageMachine ());
        pages.Add (new PageModule ());
        pages.Add (new PageConfModule ());
        pages.Add (new PageStamping ());
        pages.Add (new PageField ());
        pages.Add (new PageOperationBar ());
        pages.Add (new PageComputer ());
        return pages;
      }
    }
    #endregion // Getters / Setters

    #region Wizard methods
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    public ItemData Initialize (ItemData otherData)
    {
      var data = new ItemData ();

      // The machine may come from a previous data
      IMachine machine = null;
      if (otherData != null) {
        machine = otherData.IsStored<IMachine> (MACHINE) ?
          otherData.Get<IMachine> (MACHINE) : null;
      }

      // Common data
      data.CurrentPageName = "";
      data.InitValue (MACHINE, typeof (IMachine), machine, true);
      data.InitValue (CONFIG_FILE, typeof (string), "", true);
      data.InitValue (LOAD_PARAMETERS, typeof (bool), true, false);
      data.InitValue (XML_DATA, typeof (Dictionary<string, CncDocument>), new Dictionary<string, CncDocument> (), false);
      data.InitValue (OLD_XML_DATA_FOR_PARAM, typeof (CncDocument), null, false);
      data.InitValue (OLD_XML_DATA_FOR_MODULE, typeof (CncDocument), null, false);
      data.InitValue (COMPUTER, typeof (IComputer), null, true);
      data.InitValue (OPERATION_BAR, typeof (bool), true, true);
      data.InitValue (FIELD, typeof (IField), null, true);

      return data;
    }

    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public void Finalize (ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          // Get the machine
          var machine = data.Get<IMachine> (Item.MACHINE);

          if (machine == null) {
            throw new Exception ("Machine cannot be null");
          }

          // Prepare a monitored machine
          var moma = PrepareMonitoredMachine (machine, data);

          // Prepare a cnc acquisition
          var cncAcquisition = PrepareCncAcquisition (moma, data);

          // Prepare the machine modules
          PrepareMachineModules (moma, cncAcquisition, data);

          transaction.Commit ();
        }
      }

      // Clear all controls in CncDocument
      IDictionary<string, CncDocument> xmlData = data.Get<Dictionary<string, CncDocument>> (XML_DATA);
      foreach (string key in xmlData.Keys) {
        CncDocument cncDoc = xmlData[key];
        cncDoc.DisposeAllControls ();
      }
    }

    IMonitoredMachine PrepareMonitoredMachine (IMachine machine, ItemData data)
    {
      var moma = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (machine.Id);
      if (moma == null) {
        // Make a monitored machine and delete the previous machine
        moma = ModelDAOHelper.ModelFactory.CreateMonitoredMachine (machine);
        ModelDAOHelper.DAOFactory.MachineDAO.MakeTransient (machine);
      }

      // Attributes of the monitored machine
      moma.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById (2); // Monitored
      moma.OperationBar = data.Get<bool> (OPERATION_BAR) ? OperationBar.Operation : OperationBar.None;
      if (data.Get<IField> (FIELD) != null) {
        IField field = data.Get<IField> (FIELD);
        if (moma.PerformanceField == null || moma.PerformanceField.Id != field.Id) {
          ModelDAOHelper.DAOFactory.FieldDAO.Lock (field);
          moma.PerformanceField = field;
        }
      }
      else {
        moma.PerformanceField = null;
      }

      moma.OperationFromCnc = true;

      ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (moma);
      return moma;
    }

    ICncAcquisition PrepareCncAcquisition (IMonitoredMachine moma, ItemData data)
    {
      ICncAcquisition cncAcquisition = null;

      // A monitored machine can only have 1 cnc acquisition
      foreach (var mamo in moma.MachineModules) {
        if (mamo.CncAcquisition != null) {
          cncAcquisition = mamo.CncAcquisition;
          break;
        }
      }
      if (cncAcquisition == null) {
        cncAcquisition = moma.MainCncAcquisition;
      }

      if (cncAcquisition == null) {
        cncAcquisition = ModelDAOHelper.ModelFactory.CreateCncAcquisition ();
      }

      // Attributes
      cncAcquisition.Name = moma.Name;
      if (data.Get<IComputer> (COMPUTER) != null) {
        IComputer computer = data.Get<IComputer> (COMPUTER);
        if (cncAcquisition.Computer == null || cncAcquisition.Computer.Id != computer.Id) {
          ModelDAOHelper.DAOFactory.ComputerDAO.Lock (computer);
          cncAcquisition.Computer = computer;
        }
      }

      cncAcquisition.ConfigFile = data.Get<string> (CONFIG_FILE);
      CncDocument cncDoc = data.Get<Dictionary<string, CncDocument>> (XML_DATA)[cncAcquisition.ConfigFile];
      if (Lemoine.Info.ConfigSet.LoadAndGet (KEY_PARAMS_KEY, KEY_PARAMS_DEFAULT) || cncDoc.OldSingleParameterConfiguration) {
        cncAcquisition.ConfigParameters = cncDoc.Parameters.FirstOrDefault (x => x.Name.Equals ("Parameter"))?.GetValue () ?? "";
        cncAcquisition.ConfigKeyParams = cncDoc.Parameters.ToDictionary (p => p.Name, p => p.GetValue ());
      }
      else {
        cncAcquisition.ConfigParameters = cncDoc.GetConfigParameters ();
        cncAcquisition.ConfigKeyParams = cncDoc.Parameters.Where (x => !x.IsParamX ())
          .ToDictionary (p => p.Name, p => p.GetValue ());
      }
      if (!cncAcquisition.UseProcess) {
        cncAcquisition.UseProcess = cncDoc.UseProcess ();
      }

      if (!cncAcquisition.StaThread) {
        cncAcquisition.StaThread = cncDoc.UseStaThread ();
      }

      if (cncDoc.AcquisitionEvery > 0) {
        cncAcquisition.Every = TimeSpan.FromSeconds (cncDoc.AcquisitionEvery);
      }

      if (cncDoc.NotRespondingTimeout > 0) {
        cncAcquisition.NotRespondingTimeout = TimeSpan.FromSeconds (cncDoc.NotRespondingTimeout);
      }

      if (cncDoc.SleepBeforeRestart > 0) {
        cncAcquisition.SleepBeforeRestart = TimeSpan.FromSeconds (cncDoc.SleepBeforeRestart);
      }

      ModelDAOHelper.DAOFactory.CncAcquisitionDAO.MakePersistent (cncAcquisition);
      moma.MainCncAcquisition = cncAcquisition;
      return cncAcquisition;
    }

    void PrepareMachineModules (IMonitoredMachine monitoredMachine, ICncAcquisition cncAcquisition, ItemData data)
    {
      // Existing machine modules
      var existingMachineModules = monitoredMachine.MachineModules.ToList ();

      var existingUsed = new List<bool> ();
      for (int i = 0; i < existingMachineModules.Count; i++) {
        existingUsed.Add (false);
      }

      // New configuration
      var newModules = data.Get<Dictionary<string, CncDocument>> (XML_DATA)
        [cncAcquisition.ConfigFile].Modules;
      var newUsed = new List<bool> ();
      for (int i = 0; i < newModules.Count; i++) {
        newUsed.Add (false);
      }

      // First make modules whose prefix match
      for (int newIndex = 0; newIndex < newModules.Count; newIndex++) {
        string prefix = newModules[newIndex].m_identifier;
        if (!String.IsNullOrEmpty (prefix)) {
          prefix += "-";
        }

        for (int oldIndex = 0; oldIndex < existingMachineModules.Count; oldIndex++) {
          if (!existingUsed[oldIndex]) {
            var existingMachineModule = existingMachineModules[oldIndex];
            if (existingMachineModule.ConfigPrefix == prefix) {
              ConfigureMachineModule (monitoredMachine, cncAcquisition,
                                     existingMachineModule, newModules[newIndex]);
              existingUsed[oldIndex] = true;
              newUsed[newIndex] = true;
            }
          }
        }
      }

      // Then reuse old modules
      for (int newIndex = 0; newIndex < newModules.Count; newIndex++) {
        if (!newUsed[newIndex]) {
          // Try to find an existing position
          int oldIndex = 0;
          while (oldIndex < existingMachineModules.Count && existingUsed[oldIndex]) {
            oldIndex++;
          }

          if (oldIndex < existingMachineModules.Count) {
            // A position has been found
            ConfigureMachineModule (monitoredMachine, cncAcquisition,
                                   existingMachineModules[oldIndex], newModules[newIndex]);
            existingUsed[oldIndex] = true;
            newUsed[newIndex] = true;
          }
        }
      }

      // Finally create new modules
      for (int newIndex = 0; newIndex < newModules.Count; newIndex++) {
        if (!newUsed[newIndex]) {
          // Create a new module
          var module = newModules[newIndex];
          var machineModule = ModelDAOHelper.ModelFactory
            .CreateMachineModuleFromName (monitoredMachine, GetMachineModuleName (monitoredMachine, module));
          ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent (machineModule);
          ConfigureMachineModule (monitoredMachine, cncAcquisition, machineModule, module);
          newUsed[newIndex] = true;
        }
      }

      // Or disconnect remaining modules
      for (int oldIndex = 0; oldIndex < existingMachineModules.Count; oldIndex++) {
        if (!existingUsed[oldIndex]) {
          // Disconnect an old machine module
          DisconnectMachineModule (existingMachineModules[oldIndex]);
          existingUsed[oldIndex] = true;
        }
      }
    }

    string GetMachineModuleName (IMonitoredMachine monitoredMachine, CncDocument.Module module)
    {
      if (string.IsNullOrEmpty (module.m_identifier)) {
        return monitoredMachine.Name;
      }
      else {
        return monitoredMachine.Name + "-" + module.m_identifier;
      }
    }

    void ConfigureMachineModule (IMonitoredMachine moma, ICncAcquisition cncAcquisition,
                                IMachineModule machineModule, CncDocument.Module module)
    {
      if (string.IsNullOrEmpty (module.m_identifier)) {
        machineModule.ConfigPrefix = "";
        machineModule.Code = moma.Code;
        moma.MainMachineModule = machineModule;
      }
      else {
        machineModule.ConfigPrefix = module.m_identifier + "-";
        machineModule.Code = "";
      }
      machineModule.CncAcquisition = cncAcquisition;
      if (module.m_autoSequenceActivity != "") {
        machineModule.AutoSequenceActivity = (module.m_autoSequenceActivity.ToLower () == "machine") ?
          MachineModuleAutoSequenceActivity.Machine : MachineModuleAutoSequenceActivity.MachineModule;
      }

      // Variables
      machineModule.StartCycleVariable = module.m_startCycleVariableValue;
      machineModule.CycleVariable = module.m_cycleVariableValue;
      machineModule.SequenceVariable = module.m_sequenceVariableValue;
      machineModule.DetectionMethodVariable = module.m_detectionMethodVariableValue;

      // Detection methods
      if (module.m_startCycleDetectionMethod > 0) {
        machineModule.StartCycleDetectionMethod = (StartCycleDetectionMethod)module.m_startCycleDetectionMethod;
      }

      if (module.m_cycleDetectionMethod > 0) {
        machineModule.CycleDetectionMethod = (CycleDetectionMethod)module.m_cycleDetectionMethod;
      }

      if (module.m_sequenceDetectionMethod > 0) {
        machineModule.SequenceDetectionMethod = (SequenceDetectionMethod)module.m_sequenceDetectionMethod;
      }

      ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent (machineModule);
    }

    void DisconnectMachineModule (IMachineModule machineModule)
    {
      machineModule.CncAcquisition = null;
      ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent (machineModule);
    }

    /// <summary>
    /// Get a list of items for proposing the next possible steps to the user
    /// once the current wizard is successfully finished
    /// The value of the dictionary to return is an infinive text such as "monitor the machine"
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <returns>See the description above, can be null</returns>
    public override IDictionary<string, string> GetPossibleNextItems (ItemData data)
    {
      var nextItems = new Dictionary<string, string> ();

      // add selected_items
      nextItems["ConfiguratorSlots.MachineState.MachineStateItem"] = "select a machine state template to plan machine activity";
      return nextItems;
    }
    #endregion // Wizard methods
  }
}
