// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace WizardInitialConfiguration
{
  /// <summary>
  /// Description of Item.
  /// </summary>
  internal class Item : GenericItem, IWizard
  {
    internal const string STRUCTURE_UNIQUE_WO = "DataStructure.UniqueWorkOrderFromProjectOrComponent";
    internal const string STRUCTURE_UNIQUE_COMP_FROM_OP = "DataStructure.UniqueComponentFromOperation";
    internal const string STRUCTURE_SINGLE_PATH = "DataStructure.SinglePath";
    internal const string STRUCTURE_WO_IS_JOB = "DataStructure.WorkOrderProjectIsJob";
    internal const string STRUCTURE_WO_FROM_COMP = "DataStructure.WorkOrderFromComponentOnly";
    internal const string STRUCTURE_P_C_IS_PART = "DataStructure.ProjectComponentIsPart";
    internal const string STRUCTURE_IWP_IS_OP = "DataStructure.IntermediateWorkPieceOperationIsSimpleOperation";
    internal const string STRUCTURE_UNIQUE_PART_FROM_WO = "DataStructure.UniqueProjectOrPartFromWorkOrder";
    internal const string STRUCTURE_UNIQUE_COMP_FROM_LINE = "DataStructure.UniqueComponentFromLine";
    internal const string STRUCTURE_COMP_FROM_OP = "DataStructure.ComponentFromOperationOnly";
    internal const string OPERATION_SLOT_SPLIT_OPTION = "Analysis.OperationSlotSplitOption";
    internal const string OPERATION_EXPLORER_PART_AT_THE_TOP = "Gui.OperationExplorer.PartAtTheTop";

    internal const string DISPLAY_COMPANY = "DisplayCompany";
    internal const string DISPLAY_COMPANY_DEPARTMENT = "DisplayCompanyDepartment";
    internal const string DISPLAY_MACHINE_CATEGORY = "DisplayMachineCategory";
    internal const string DISPLAY_MACHINE_SUB_CATEGORY = "DisplayMachineSubCategory";
    internal const string DISPLAY_MACHINE = "DisplayMachineAndMonitoredMachine";
    internal const string DISPLAY_MACHINE_MODULE = "DisplayMachineModule";

    internal const string DISPLAY_WO = "DisplayWorkOrder";
    internal const string DISPLAY_WO_OPERATION_SLOT = "DisplayWorkOrderOperationSlot";
    internal const string DISPLAY_PROJECT_JOB = "DisplayProjectOrJob";
    internal const string DISPLAY_COMPONENT_PART = "DisplayComponentOrPart";
    internal const string DISPLAY_COMPONENT_PART_OPERATION_SLOT = "DisplayComponentOrPartOperationSlot";
    internal const string DISPLAY_IWP = "DisplayIntermediateWorkPiece";
    internal const string DISPLAY_OPERATION = "DisplayOperationAndSimpleOperation";
    internal const string DISPLAY_OPERATION_WITH_COMPONENT = "DisplayOperationWithComponent";
    internal const string DISPLAY_OPERATION_OPERATION_SLOT = "DisplayOperationAndSimpleOperationOperationSlot";
    internal const string DISPLAY_OPERATION_SLOT = "DisplayOperationSlot";

    internal const string DISPLAY_SEQUENCE = "DisplaySequence";
    internal const string DISPLAY_SEQUENCE_SEQUENCE_SLOT = "DisplaySequenceSequenceSlot";
    internal const string DISPLAY_TOOL = "DisplayTool";

    internal const string DISPLAY_COMPUTER = "DisplayComputer";
    internal const string DISPLAY_LINE = "DisplayLine";

    internal const string DISPLAY_REASON_GROUP = "DisplayReasonGroup";
    internal const string DISPLAY_REASON = "DisplayReason";

    internal const string DISPLAY_SHIFT = "DisplayShift";
    internal const string DISPLAY_USER = "DisplayUser";
    
    internal const string TOOL_LIFE_EVENTS = "ToolLifeEvents";
    internal const string LONG_PERIOD_EVENTS = "LongPeriodEvents";
    
    internal const int WARN_LEVEL = 401;
    internal const int ERR_LEVEL = 301;

    #region Getters / Setters
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    public override string Title { get { return "Initial configuration"; } }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    public override string Description {
      get {
        return "This initial wizard will configure the newly installed system.";
      }
    }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    public override ICollection<string> Keywords {
      get {
        return new String [] { "setup", "primary", "first", "installation", "initial", "configuration" };
      }
    }
    
    /// <summary>
    /// Name of the icon, coming from a resx associated to the item
    /// </summary>
    protected override string IconName { get { return "wizard"; } }
    
    /// <summary>
    /// Default category
    /// </summary>
    public override string Category { get { return "Global configurations"; } }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    public override string Subcategory { get { return "Installation"; } }

    /// <summary>
    /// Flags that characterize the item
    /// </summary>
    public override LemSettingsGlobal.ItemFlag ItemFlags {
      get {
        return LemSettingsGlobal.ItemFlag.ONLY_ADMIN_AND_SUPER_ADMIN;
      }
    }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    public IDictionary<Type, LemSettingsGlobal.InteractionType> Types {
      get {
        var dic = new Dictionary<Type, LemSettingsGlobal.InteractionType>();
        dic[typeof(IConfig)] = LemSettingsGlobal.InteractionType.PRINCIPAL;
        return dic;
      }
    }
    
    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    public IList<IWizardPage> Pages {
      get {
        IList<IWizardPage> pages = new List<IWizardPage>();
        pages.Add(new Page1());
        pages.Add(new Page2());
        pages.Add(new Page3());
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
    public ItemData Initialize(ItemData otherData)
    {
      var data = new ItemData();
      
      // Common data
      data.CurrentPageName = "";
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {

          // Initialization of options

          InitStructure(data, DataStructureConfigKey.UniqueWorkOrderFromProjectOrComponent, STRUCTURE_UNIQUE_WO);
          InitStructure(data, DataStructureConfigKey.UniqueComponentFromOperation, STRUCTURE_UNIQUE_COMP_FROM_OP);
          InitStructure(data, DataStructureConfigKey.SinglePath, STRUCTURE_SINGLE_PATH);
          InitStructure(data, DataStructureConfigKey.WorkOrderProjectIsJob, STRUCTURE_WO_IS_JOB);
          InitStructure(data, DataStructureConfigKey.WorkOrderFromComponentOnly, STRUCTURE_WO_FROM_COMP);
          InitStructure(data, DataStructureConfigKey.ProjectComponentIsPart, STRUCTURE_P_C_IS_PART);
          InitStructure(data, DataStructureConfigKey.IntermediateWorkPieceOperationIsSimpleOperation, STRUCTURE_IWP_IS_OP);
          InitStructure(data, DataStructureConfigKey.UniqueProjectOrPartFromWorkOrder, STRUCTURE_UNIQUE_PART_FROM_WO);
          InitStructure(data, DataStructureConfigKey.UniqueComponentFromLine, STRUCTURE_UNIQUE_COMP_FROM_LINE);
          InitStructure(data, DataStructureConfigKey.ComponentFromOperationOnly, STRUCTURE_COMP_FROM_OP);
          
          object obj = ModelDAOHelper.DAOFactory.ConfigDAO.GetAnalysisConfigValue(AnalysisConfigKey.OperationSlotSplitOption);
          data.InitValue(OPERATION_SLOT_SPLIT_OPTION, typeof(int), (obj is int) ? (int)obj : 0, true);

          obj = ModelDAOHelper.DAOFactory.ConfigDAO.GetOperationExplorerConfigValue (OperationExplorerConfigKey.PartAtTheTop);
          data.InitValue (OPERATION_EXPLORER_PART_AT_THE_TOP, typeof (bool), (obj is bool) ? (bool)obj : false, true);


          // Initialization of display

          InitDisplay (data, "Company",  DISPLAY_COMPANY);
          InitDisplay(data, "Department", DISPLAY_COMPANY_DEPARTMENT);
          InitDisplay(data, "MachineCategory", DISPLAY_MACHINE_CATEGORY);
          InitDisplay(data, "MachineSubCategory", DISPLAY_MACHINE_SUB_CATEGORY);
          InitDisplay(data, "Machine", DISPLAY_MACHINE);
          InitDisplay(data, "MachineModule", DISPLAY_MACHINE_MODULE);

          InitDisplay(data, "WorkOrder", DISPLAY_WO);
          if (!data.Get<bool>(Item.STRUCTURE_WO_IS_JOB)) {
            data.InitValue(DISPLAY_WO_OPERATION_SLOT, typeof(string), "", true);
          }
          else {
            InitDisplay(data, "WorkOrder.OperationSlot", DISPLAY_WO_OPERATION_SLOT);
          }
          InitDisplay(data, "Project", DISPLAY_PROJECT_JOB);
          InitDisplay(data, "Component", DISPLAY_COMPONENT_PART);
          if (!data.Get<bool>(Item.STRUCTURE_P_C_IS_PART)) {
            data.InitValue(DISPLAY_COMPONENT_PART_OPERATION_SLOT, typeof(string), "<%Project.Display%>/<%Display%>", true);
          }
          else {
            InitDisplay(data, "Component.OperationSlot", DISPLAY_COMPONENT_PART_OPERATION_SLOT);
          }
          InitDisplay(data, "IntermediateWorkPiece", DISPLAY_IWP);
          InitDisplay(data, "Operation", DISPLAY_OPERATION);
          InitDisplay(data, "Operation.Long", DISPLAY_OPERATION_WITH_COMPONENT);
          InitDisplay(data, "Operation.OperationSlot", DISPLAY_OPERATION_OPERATION_SLOT);
          InitDisplay(data, "OperationSlot", DISPLAY_OPERATION_SLOT);

          InitDisplay(data, "Sequence", DISPLAY_SEQUENCE);
          InitDisplay(data, "Sequence.SequenceSlot", DISPLAY_SEQUENCE_SEQUENCE_SLOT);
          InitDisplay(data, "Tool", DISPLAY_TOOL);

          InitDisplay(data, "Computer", DISPLAY_COMPUTER);
          InitDisplay(data, "Line", DISPLAY_LINE);

          InitDisplay(data, "ReasonGroup", DISPLAY_REASON_GROUP);
          InitDisplay(data, "Reason", DISPLAY_REASON);

          InitDisplay(data, "Shift", DISPLAY_SHIFT);
          InitDisplay(data, "User", DISPLAY_USER);
        }
      }
      
      data.InitValue(TOOL_LIFE_EVENTS, typeof(bool), true, true);
      data.InitValue(LONG_PERIOD_EVENTS, typeof(bool), true, true);
      
      return data;
    }
    
    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public void Finalize(ItemData data, ref IList<string> warnings, ref IRevision revision)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          IDAOFactory factoryDAO = ModelDAOHelper.DAOFactory;

          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_UNIQUE_WO, data.Get<bool>(STRUCTURE_UNIQUE_WO), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_UNIQUE_COMP_FROM_OP, data.Get<bool>(STRUCTURE_UNIQUE_COMP_FROM_OP), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_SINGLE_PATH, data.Get<bool>(STRUCTURE_SINGLE_PATH), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_WO_IS_JOB, data.Get<bool>(STRUCTURE_WO_IS_JOB), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_WO_FROM_COMP, data.Get<bool>(STRUCTURE_WO_FROM_COMP), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_P_C_IS_PART, data.Get<bool>(STRUCTURE_P_C_IS_PART), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_IWP_IS_OP, data.Get<bool>(STRUCTURE_IWP_IS_OP), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_UNIQUE_PART_FROM_WO, data.Get<bool>(STRUCTURE_UNIQUE_PART_FROM_WO), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_UNIQUE_COMP_FROM_LINE, data.Get<bool>(STRUCTURE_UNIQUE_COMP_FROM_LINE), true);
          factoryDAO.ConfigDAO.SetConfig(STRUCTURE_COMP_FROM_OP, data.Get<bool>(STRUCTURE_COMP_FROM_OP), true);
          factoryDAO.ConfigDAO.SetConfig(OPERATION_SLOT_SPLIT_OPTION, data.Get<int>(OPERATION_SLOT_SPLIT_OPTION), true);
          factoryDAO.ConfigDAO.SetConfig (OPERATION_EXPLORER_PART_AT_THE_TOP, data.Get<bool> (OPERATION_EXPLORER_PART_AT_THE_TOP), true);
          
          if (data.Get<bool>(TOOL_LIFE_EVENTS)) {
            AddToolLifeEvents ();
          }

          if (data.Get<bool>(LONG_PERIOD_EVENTS)) {
            AddLongPeriodEvents ();
          }

          transaction.Commit();
        }
      }
      
      UpdateDisplay("Company", data.Get<string>(DISPLAY_COMPANY));
      UpdateDisplay("Department", data.Get<string>(DISPLAY_COMPANY_DEPARTMENT));
      UpdateDisplay("MachineCategory", data.Get<string>(DISPLAY_MACHINE_CATEGORY));
      UpdateDisplay("MachineSubCategory", data.Get<string>(DISPLAY_MACHINE_SUB_CATEGORY));
      UpdateDisplay("Machine", data.Get<string>(DISPLAY_MACHINE));
      UpdateDisplay("MachineModule", data.Get<string>(DISPLAY_MACHINE_MODULE));

      UpdateDisplay("WorkOrder", data.Get<string>(DISPLAY_WO));
      UpdateDisplay("WorkOrder.OperationSlot", data.Get<string>(DISPLAY_WO_OPERATION_SLOT));
      UpdateDisplay("Project", data.Get<string>(DISPLAY_PROJECT_JOB));
      UpdateDisplay("Job", "<%Project.Display%>");
      UpdateDisplay("Component", data.Get<string>(DISPLAY_COMPONENT_PART));
      UpdateDisplay("Component.OperationSlot", data.Get<string>(DISPLAY_COMPONENT_PART_OPERATION_SLOT));
      UpdateDisplay("Part", "<%Component.Display%>");
      UpdateDisplay("IntermediateWorkPiece", data.Get<string>(DISPLAY_IWP));
      UpdateDisplay("Operation", data.Get<string>(DISPLAY_OPERATION));
      UpdateDisplay("SimpleOperation", "<%Operation.Display%>");
      UpdateDisplay("Operation.Long", data.Get<string>(DISPLAY_OPERATION_WITH_COMPONENT));
      UpdateDisplay("Operation.Short", "<%Code%>");
      UpdateDisplay("Operation.OperationSlot", data.Get<string>(DISPLAY_OPERATION_OPERATION_SLOT));
      UpdateDisplay("OperationSlot", data.Get<string>(DISPLAY_OPERATION_SLOT));

      UpdateDisplay("Tool", data.Get<string>(DISPLAY_TOOL));
      UpdateDisplay("Sequence", data.Get<string>(DISPLAY_SEQUENCE));
      UpdateDisplay("Sequence.SequenceSlot", data.Get<string>(DISPLAY_SEQUENCE_SEQUENCE_SLOT));

      UpdateDisplay("Computer", data.Get<string>(DISPLAY_COMPUTER));
      UpdateDisplay("Line", data.Get<string>(DISPLAY_LINE));

      UpdateDisplay("ReasonGroup", data.Get<string>(DISPLAY_REASON_GROUP));
      UpdateDisplay("Reason", data.Get<string>(DISPLAY_REASON));

      UpdateDisplay("Shift", data.Get<string>(DISPLAY_SHIFT));
      UpdateDisplay("User", data.Get<string>(DISPLAY_USER));
    }
    #endregion // Wizard methods
    
    #region Private methods
    void InitStructure(ItemData data, DataStructureConfigKey configKey, string dataKey)
    {
      object obj = ModelDAOHelper.DAOFactory.ConfigDAO.GetDataStructureConfigValue(configKey);
      data.InitValue(dataKey, typeof(bool), (obj is bool) ? (bool)obj : false, true);
    }
    
    void InitDisplay(ItemData data, string tableName, string dataKey)
    {
      string val = "";
      try {
        string[] split = tableName.Split('.');
        if (split.Length == 1) {
          val = ModelDAOHelper.DAOFactory.DisplayDAO.GetPattern(split[0]);
        }
        else if (split.Length == 2) {
          val = ModelDAOHelper.DAOFactory.DisplayDAO.GetPatternWithDefault(split[0], split[1]);
        }
      } catch (Exception) {} // To avoid problems from a corrupted database
      data.InitValue(dataKey, typeof(string), val, true);
    }
    
    void UpdateDisplay(string tableName, string newDisplay)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          IDAOFactory factoryDAO = ModelDAOHelper.DAOFactory;
          
          // Find a corresponding display
          IDisplay display = null;
          string[] split = tableName.Split('.');
          if (split.Length == 1) {
            display = ModelDAOHelper.DAOFactory.DisplayDAO.FindWithTable(split[0]);
          }
          else if (split.Length == 2) {
            display = ModelDAOHelper.DAOFactory.DisplayDAO.FindWithTableVariant(split[0], split[1]);
          }

          if (display == null) {
            display = ModelDAOHelper.ModelFactory.CreateDisplay(split[0]);
            if (split.Length == 2) {
              display.Variant = split[1];
            }

            ModelDAOHelper.DAOFactory.DisplayDAO.MakePersistent(display);
          }
          display.Pattern = newDisplay;
          
          transaction.Commit();
        }
      }
    }
    
    void AddToolLifeEvents()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          var errorLevel = ModelDAOHelper.DAOFactory.EventLevelDAO.FindByPriority(300);
          
          if (errorLevel.Count > 0) {
            // Current tool life decreases
            if (ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAllByType(
              EventToolLifeType.CurrentLifeDecreased).Count == 0) {
              var eventConfig = ModelDAOHelper.ModelFactory.CreateEventToolLifeConfig(
                EventToolLifeType.CurrentLifeDecreased, errorLevel[0]);
              ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.MakePersistent(eventConfig);
            }
            
            // Rest tool life increases
            if (ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAllByType(
              EventToolLifeType.RestLifeIncreased).Count == 0) {
              var eventConfig = ModelDAOHelper.ModelFactory.CreateEventToolLifeConfig(
                EventToolLifeType.RestLifeIncreased, errorLevel[0]);
              ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.MakePersistent(eventConfig);
            }
            
            // Total tool life decreases
            if (ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAllByType(
              EventToolLifeType.CurrentLifeDecreased).Count == 0) {
              var eventConfig = ModelDAOHelper.ModelFactory.CreateEventToolLifeConfig(
                EventToolLifeType.TotalLifeDecreased, errorLevel[0]);
              ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.MakePersistent(eventConfig);
            }
          }
          
          transaction.Commit();
        }
      }
    }
    
    void AddLongPeriodEvents()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          var existingEventLevels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindAll();
          
          // level WARN > 10 min
          IEventLevel warnLevel = null;
          foreach (var existingEventLevel in existingEventLevels) {
            if (existingEventLevel.Name == "WARN > 10 min") {
              warnLevel = existingEventLevel;
            }
          }
          if (warnLevel == null) {
            warnLevel = ModelDAOHelper.ModelFactory.CreateEventLevelFromName (WARN_LEVEL, "WARN > 10 min");
            ModelDAOHelper.DAOFactory.EventLevelDAO.MakePersistent(warnLevel);
          }
          
          // level ERR > 20 min
          IEventLevel errLevel = null;
          foreach (var existingEventLevel in existingEventLevels) {
            if (existingEventLevel.Name == "ERR > 20 min") {
              errLevel = existingEventLevel;
            }
          }
          if (errLevel == null) {
            errLevel = ModelDAOHelper.ModelFactory.CreateEventLevelFromName (ERR_LEVEL, "ERR > 20 min");
            ModelDAOHelper.DAOFactory.EventLevelDAO.MakePersistent(errLevel);
          }
          
          var modeInactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById(1);
          if (modeInactive != null) {
            var levels = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.GetLevels();
            if (!levels.Contains(warnLevel)) {
              var config10min = ModelDAOHelper.ModelFactory.CreateEventLongPeriodConfig(
                new TimeSpan(0, 10, 0), warnLevel);
              config10min.MachineMode = modeInactive;
              ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.MakePersistent(config10min);
            }
            
            if (!levels.Contains(errLevel)) {
              var config20min = ModelDAOHelper.ModelFactory.CreateEventLongPeriodConfig(
                new TimeSpan(0, 20, 0), errLevel);
              config20min.MachineMode = modeInactive;
              ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.MakePersistent(config20min);
            }
          }
          
          transaction.Commit();
        }
      }
    }
    #endregion // Private methods
  }
}
