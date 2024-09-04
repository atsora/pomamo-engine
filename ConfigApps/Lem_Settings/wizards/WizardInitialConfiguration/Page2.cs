// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.ModelDAO;
using Lemoine.Settings;

namespace WizardInitialConfiguration
{
  /// <summary>
  /// Description of Page2.
  /// </summary>
  internal partial class Page2 : GenericWizardPage, IWizardPage
  {
    #region Members
    IDictionary<string, DisplayCell> m_cells = new Dictionary<string, DisplayCell>();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Item display"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Pattern can be modified here, to specify how each item should be written " +
          "in the reports and interfaces.\n\n" +
          "Each pattern is made of at least one field from the related item, included in \"<%\" and \"%>\". " +
          "For instance for the company:\n<%Code%>:<%Name%>"; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page2()
    {
      InitializeComponent();
      
      m_cells[Item.DISPLAY_COMPANY] = new DisplayCell("Company", "Name", "Code");
      m_cells[Item.DISPLAY_COMPANY_DEPARTMENT] = new DisplayCell("Company department", "Name", "Code");
      m_cells[Item.DISPLAY_MACHINE_CATEGORY] = new DisplayCell("Machine category", "Name", "Code");
      m_cells[Item.DISPLAY_MACHINE_SUB_CATEGORY] = new DisplayCell("Machine subcategory", "Name", "Code");
      m_cells[Item.DISPLAY_MACHINE] = new DisplayCell("Machine", "Name", "Code", "Company.Display", "Department.Display", "Category.Display", "SubCategory.Display");
      m_cells[Item.DISPLAY_MACHINE_MODULE] = new DisplayCell("Machine module", "Name", "Code");

      m_cells[Item.DISPLAY_WO] = new DisplayCell("Work order", "Name", "Code");
      m_cells[Item.DISPLAY_WO_OPERATION_SLOT] = new DisplayCell("WO in operation slot", "Display");
      m_cells[Item.DISPLAY_PROJECT_JOB] = new DisplayCell("Project / Job", "Name", "Code");
      m_cells[Item.DISPLAY_COMPONENT_PART] = new DisplayCell("Component / Part", "Name", "Code");
      m_cells[Item.DISPLAY_COMPONENT_PART_OPERATION_SLOT] = new DisplayCell("Component in operation slot", "Display", "Project.Display");
      m_cells[Item.DISPLAY_IWP] = new DisplayCell("Intermediate work piece", "Name", "Code", "Component.Display", "Components");
      m_cells[Item.DISPLAY_OPERATION] = new DisplayCell("Operation", "Name", "Code", "Component.Display", "Components");
      m_cells[Item.DISPLAY_OPERATION_WITH_COMPONENT] = new DisplayCell("Operation with component", "Name", "Code", "Component.Display", "Components");
      m_cells[Item.DISPLAY_OPERATION_OPERATION_SLOT] = new DisplayCell("Operation in OperationSlot", "Display");
      m_cells[Item.DISPLAY_OPERATION_SLOT] = new DisplayCell("Operation slot", "Operation.Display_OperationSlot", "Component.Display_OperationSlot", "WorkOrder.Display_OperationSlot", "ciwporder", "ciwpcode");

      m_cells[Item.DISPLAY_TOOL] = new DisplayCell("Tool", "Name", "Code", "Display_Size");
      m_cells[Item.DISPLAY_SEQUENCE] = new DisplayCell("Sequence", "Name", "Tool.Display", "Order", "Description");
      m_cells[Item.DISPLAY_SEQUENCE_SEQUENCE_SLOT] = new DisplayCell("Sequence in SequenceSlot", "Name", "Tool.Display", "Tool.Display_Size", "Order", "Description");

      m_cells[Item.DISPLAY_COMPUTER] = new DisplayCell("Computer", "Name", "Address");
      m_cells[Item.DISPLAY_LINE] = new DisplayCell("Line", "Name", "Code");

      m_cells[Item.DISPLAY_REASON_GROUP] = new DisplayCell("Reason group", "Name", "NameOrTranslation");
      m_cells[Item.DISPLAY_REASON] = new DisplayCell("Reason", "Name", "Code", "NameOrTranslation", "ReasonGroup.Display");

      m_cells[Item.DISPLAY_SHIFT] = new DisplayCell("Shift", "Name", "Code");
      m_cells[Item.DISPLAY_USER] = new DisplayCell("User", "Name", "Code", "Login", "MobileNumber", "EMail");
      
      foreach (string text in m_cells.Keys) {
        verticalScrollLayout.AddControl(m_cells[text]);
      }
    }
    #endregion // Constructors

    #region Page methods
    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    /// <param name="context">Context of the item</param>
    public void Initialize(ItemContext context) {}
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_cells[Item.DISPLAY_COMPANY].Pattern = data.Get<string>(Item.DISPLAY_COMPANY);
      m_cells[Item.DISPLAY_COMPANY_DEPARTMENT].Pattern = data.Get<string>(Item.DISPLAY_COMPANY_DEPARTMENT);
      m_cells[Item.DISPLAY_MACHINE_CATEGORY].Pattern = data.Get<string>(Item.DISPLAY_MACHINE_CATEGORY);
      m_cells[Item.DISPLAY_MACHINE_SUB_CATEGORY].Pattern = data.Get<string>(Item.DISPLAY_MACHINE_SUB_CATEGORY);
      m_cells[Item.DISPLAY_MACHINE].Pattern = data.Get<string>(Item.DISPLAY_MACHINE);
      m_cells[Item.DISPLAY_MACHINE_MODULE].Pattern = data.Get<string>(Item.DISPLAY_MACHINE_MODULE);

      m_cells[Item.DISPLAY_WO].Pattern = data.Get<string>(Item.DISPLAY_WO);
      m_cells[Item.DISPLAY_WO_OPERATION_SLOT].Pattern = data.Get<string>(Item.DISPLAY_WO_OPERATION_SLOT);
      m_cells[Item.DISPLAY_PROJECT_JOB].Pattern = data.Get<string>(Item.DISPLAY_PROJECT_JOB);
      m_cells[Item.DISPLAY_COMPONENT_PART].Pattern = data.Get<string>(Item.DISPLAY_COMPONENT_PART);
      m_cells[Item.DISPLAY_COMPONENT_PART_OPERATION_SLOT].Pattern = data.Get<string>(Item.DISPLAY_COMPONENT_PART_OPERATION_SLOT);
      m_cells[Item.DISPLAY_IWP].Pattern = data.Get<string>(Item.DISPLAY_IWP);
      m_cells[Item.DISPLAY_OPERATION].Pattern = data.Get<string>(Item.DISPLAY_OPERATION);
      m_cells[Item.DISPLAY_OPERATION_WITH_COMPONENT].Pattern = data.Get<string>(Item.DISPLAY_OPERATION_WITH_COMPONENT);
      m_cells[Item.DISPLAY_OPERATION_OPERATION_SLOT].Pattern = data.Get<string>(Item.DISPLAY_OPERATION_OPERATION_SLOT);
      m_cells[Item.DISPLAY_OPERATION_SLOT].Pattern = data.Get<string>(Item.DISPLAY_OPERATION_SLOT);

      m_cells[Item.DISPLAY_TOOL].Pattern = data.Get<string>(Item.DISPLAY_TOOL);
      m_cells[Item.DISPLAY_SEQUENCE].Pattern = data.Get<string>(Item.DISPLAY_SEQUENCE);
      m_cells[Item.DISPLAY_SEQUENCE_SEQUENCE_SLOT].Pattern = data.Get<string>(Item.DISPLAY_SEQUENCE_SEQUENCE_SLOT);

      m_cells[Item.DISPLAY_COMPUTER].Pattern = data.Get<string>(Item.DISPLAY_COMPUTER);
      m_cells[Item.DISPLAY_LINE].Pattern = data.Get<string>(Item.DISPLAY_LINE);
      
      m_cells[Item.DISPLAY_REASON_GROUP].Pattern = data.Get<string>(Item.DISPLAY_REASON_GROUP);
      m_cells[Item.DISPLAY_REASON].Pattern = data.Get<string>(Item.DISPLAY_REASON);
      
      m_cells[Item.DISPLAY_SHIFT].Pattern = data.Get<string>(Item.DISPLAY_SHIFT);
      m_cells[Item.DISPLAY_USER].Pattern = data.Get<string>(Item.DISPLAY_USER);
      
      m_cells[Item.DISPLAY_IWP].Visible = !data.Get<bool>(Item.STRUCTURE_IWP_IS_OP);
      m_cells[Item.DISPLAY_PROJECT_JOB].Visible = !data.Get<bool>(Item.STRUCTURE_P_C_IS_PART);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.DISPLAY_COMPANY, m_cells[Item.DISPLAY_COMPANY].Pattern);
      data.Store(Item.DISPLAY_COMPANY_DEPARTMENT, m_cells[Item.DISPLAY_COMPANY_DEPARTMENT].Pattern);
      data.Store(Item.DISPLAY_MACHINE_CATEGORY, m_cells[Item.DISPLAY_MACHINE_CATEGORY].Pattern);
      data.Store(Item.DISPLAY_MACHINE, m_cells[Item.DISPLAY_MACHINE].Pattern);
      data.Store(Item.DISPLAY_MACHINE_SUB_CATEGORY, m_cells[Item.DISPLAY_MACHINE_SUB_CATEGORY].Pattern);
      data.Store(Item.DISPLAY_MACHINE_MODULE, m_cells[Item.DISPLAY_MACHINE_MODULE].Pattern);
      
      data.Store(Item.DISPLAY_WO, m_cells[Item.DISPLAY_WO].Pattern);
      data.Store(Item.DISPLAY_WO_OPERATION_SLOT, m_cells[Item.DISPLAY_WO_OPERATION_SLOT].Pattern);
      // Project like component / part?
      if (data.Get<bool>(Item.STRUCTURE_P_C_IS_PART)) {
        data.Store(Item.DISPLAY_PROJECT_JOB, m_cells[Item.DISPLAY_COMPONENT_PART].Pattern);
      }
      else {
        data.Store(Item.DISPLAY_PROJECT_JOB, m_cells[Item.DISPLAY_PROJECT_JOB].Pattern);
      }

      data.Store(Item.DISPLAY_COMPONENT_PART, m_cells[Item.DISPLAY_COMPONENT_PART].Pattern);
      data.Store(Item.DISPLAY_COMPONENT_PART_OPERATION_SLOT, m_cells[Item.DISPLAY_COMPONENT_PART_OPERATION_SLOT].Pattern);
      data.Store(Item.DISPLAY_OPERATION, m_cells[Item.DISPLAY_OPERATION].Pattern);
      data.Store(Item.DISPLAY_OPERATION_WITH_COMPONENT, m_cells[Item.DISPLAY_OPERATION_WITH_COMPONENT].Pattern);
      data.Store(Item.DISPLAY_OPERATION_OPERATION_SLOT, m_cells[Item.DISPLAY_OPERATION_OPERATION_SLOT].Pattern);
      data.Store(Item.DISPLAY_OPERATION_SLOT, m_cells[Item.DISPLAY_OPERATION_SLOT].Pattern);

      data.Store(Item.DISPLAY_TOOL, m_cells[Item.DISPLAY_TOOL].Pattern);
      data.Store(Item.DISPLAY_SEQUENCE, m_cells[Item.DISPLAY_SEQUENCE].Pattern);
      data.Store(Item.DISPLAY_SEQUENCE_SEQUENCE_SLOT, m_cells[Item.DISPLAY_SEQUENCE_SEQUENCE_SLOT].Pattern);

      data.Store(Item.DISPLAY_COMPUTER, m_cells[Item.DISPLAY_COMPUTER].Pattern);
      data.Store(Item.DISPLAY_LINE, m_cells[Item.DISPLAY_LINE].Pattern);

      data.Store(Item.DISPLAY_REASON_GROUP, m_cells[Item.DISPLAY_REASON_GROUP].Pattern);
      data.Store(Item.DISPLAY_REASON, m_cells[Item.DISPLAY_REASON].Pattern);

      data.Store(Item.DISPLAY_SHIFT, m_cells[Item.DISPLAY_SHIFT].Pattern);
      data.Store(Item.DISPLAY_USER, m_cells[Item.DISPLAY_USER].Pattern);
      
      // Iwp like operation?
      if (data.Get<bool>(Item.STRUCTURE_IWP_IS_OP)) {
        data.Store(Item.DISPLAY_IWP, m_cells[Item.DISPLAY_OPERATION].Pattern);
      }
      else {
        data.Store(Item.DISPLAY_IWP, m_cells[Item.DISPLAY_IWP].Pattern);
      }
    }
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public override IList<string> GetErrorsToGoNext(ItemData data)
    {
      IList<string> errors = new List<string>();
      
      bool ok = true;
      foreach (Control c in verticalScrollLayout.ControlsInLayout) {
        if (c is DisplayCell) {
          ok &= (c as DisplayCell).IsValid();
        }
      }

      if (!ok) {
        errors.Add("all patterns are not valid");
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
      // Test if default events are already configured
      bool defaultToolLife = false;
      bool defaultLongPeriod = false;
      bool defaultLevels = false;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var type1 = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAllByType(
            Lemoine.Model.EventToolLifeType.CurrentLifeDecreased);
          var type2 = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAllByType(
            Lemoine.Model.EventToolLifeType.RestLifeIncreased);
          var type3 = ModelDAOHelper.DAOFactory.EventToolLifeConfigDAO.FindAllByType(
            Lemoine.Model.EventToolLifeType.TotalLifeIncreased);
          
          defaultToolLife = (type1.Count + type2.Count + type3.Count > 0);
          
          var configs = ModelDAOHelper.DAOFactory.EventLongPeriodConfigDAO.FindAll();
          var modeInactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById(1);
          if (modeInactive != null) {
            foreach (var config in configs) {
              if (object.Equals(config.MachineMode, modeInactive) && (
                ((int)config.TriggerDuration.TotalMinutes == 5 && config.TriggerDuration.Seconds == 0 ||
                 (int)config.TriggerDuration.TotalMinutes == 10 && config.TriggerDuration.Seconds == 0))) {
                defaultLongPeriod = true;
                break;
              }
            }
          }
          
          var warnLevels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindByPriority(Item.WARN_LEVEL);
          var errLevels = ModelDAOHelper.DAOFactory.EventLevelDAO.FindByPriority(Item.ERR_LEVEL);
          defaultLevels = warnLevels.Count > 0 || errLevels.Count > 0;
        }
      }
      
      return defaultToolLife || defaultLongPeriod || defaultLevels ? "" : "Page3";
    }
    
    /// <summary>
    /// Get a summary of the user inputs
    /// </summary>
    /// <returns></returns>
    public IList<string> GetSummary(ItemData data)
    {
      IList<string> summary = new List<string>();
      
      summary.Add("Company displayed as: " + data.Get<string>(Item.DISPLAY_COMPANY));
      summary.Add("Company department displayed as: " + data.Get<string>(Item.DISPLAY_COMPANY_DEPARTMENT));
      summary.Add("Machine category displayed as: " + data.Get<string>(Item.DISPLAY_MACHINE_CATEGORY));
      summary.Add("Machine sub-category displayed as: " + data.Get<string>(Item.DISPLAY_MACHINE_SUB_CATEGORY));
      summary.Add("Machine displayed as: " + data.Get<string>(Item.DISPLAY_MACHINE));
      summary.Add("Machine module displayed as: " + data.Get<string>(Item.DISPLAY_MACHINE_MODULE));
      
      summary.Add("Work order displayed as: " + data.Get<string>(Item.DISPLAY_WO));
      summary.Add("Work order in operation slot displayed as: " + data.Get<string>(Item.DISPLAY_WO_OPERATION_SLOT));
      summary.Add("Project or job displayed as: " + data.Get<string>(Item.DISPLAY_PROJECT_JOB));
      summary.Add("Component or part displayed as: " + data.Get<string>(Item.DISPLAY_COMPONENT_PART));
      summary.Add("Component or part in operation slot displayed as: " + data.Get<string>(Item.DISPLAY_COMPONENT_PART_OPERATION_SLOT));
      summary.Add("Intermediate work piece displayed as: " + data.Get<string>(Item.DISPLAY_IWP));
      summary.Add("Operation displayed as: " + data.Get<string>(Item.DISPLAY_OPERATION));
      summary.Add("Operation with component displayed as: " + data.Get<string>(Item.DISPLAY_OPERATION_WITH_COMPONENT));
      summary.Add("Operation in operation slot displayed as: " + data.Get<string>(Item.DISPLAY_OPERATION_OPERATION_SLOT));
      summary.Add("Operation slot displayed as: " + data.Get<string>(Item.DISPLAY_OPERATION_SLOT));
      
      summary.Add("Tool displayed as: " + data.Get<string>(Item.DISPLAY_TOOL));
      summary.Add("Sequence displayed as: " + data.Get<string>(Item.DISPLAY_SEQUENCE));
      summary.Add("Sequence in sequence slot displayed as: " + data.Get<string>(Item.DISPLAY_SEQUENCE_SEQUENCE_SLOT));
      
      summary.Add("Computer displayed as: " + data.Get<string>(Item.DISPLAY_COMPUTER));
      summary.Add("Line displayed as: " + data.Get<string>(Item.DISPLAY_LINE));
      
      summary.Add("Reason group displayed as: " + data.Get<string>(Item.DISPLAY_REASON_GROUP));
      summary.Add("Reason displayed as: " + data.Get<string>(Item.DISPLAY_REASON));
      
      summary.Add("Shift displayed as: " + data.Get<string>(Item.DISPLAY_SHIFT));
      summary.Add("User displayed as: " + data.Get<string>(Item.DISPLAY_USER));
      
      return summary;
    }
    #endregion // Page methods
  }
}
