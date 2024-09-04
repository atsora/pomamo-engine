// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Collections;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of Page3.
  /// </summary>
  public partial class Page3 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    GoalManager m_goals = null;
    bool m_byDepartments = false;
    bool m_preparation = true;
    IMachineObservationState m_currentMOS = null;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Goal definition"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "For each machine state, you can choose if you want to apply goals with a default value.\n\n" +
          "When it's done, you can then assign goals to company / categories / subcategories or company / department / cell " +
          "(according to the categorization you chose), to overwrite the default value. " +
          "For example, assigning a goal directly to a machine prevails on all other goals that could have included the machine. " +
          "Goals are automatically propagated until the next overwrite.\n\n" +
          "Elements in blue in the tree are elements linked to a rule."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    
    GoalFilter CurrentGoalFilter {
      get { return m_goals != null ? m_goals.CurrentGoalFilter : null; }
      set {
        if (m_goals != null) {
          m_goals.CurrentGoalFilter = value;
        }
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page3()
    {
      InitializeComponent();
      treeMachines.AddDetermineColorFunction(GetColorNode);
    }
    
    Color GetColorNode(object obj)
    {
      var tne = obj as TreeNodeElement;
      return (tne != null && tne.IsBlue) ? Color.Blue : SystemColors.ControlText;
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
      // Load machines in the tree
      treeMachines.ClearElements();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMonitoredMachine> machines = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindAll();
          var treeElements = new List<TreeElement>();
          foreach (IMonitoredMachine machine in machines) {
            if (!machine.IsObsolete()) {
              treeElements.Add(new TreeElement(machine));
            }
          }

          treeElements.Sort();
          foreach (var treeElement in treeElements) {
            treeMachines.AddElement(treeElement);
          }
        }
      }
      
      // Load the different machine observation states
      comboboxState.ClearItems();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          IList<IMachineObservationState> moss = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindAll();
          foreach (IMachineObservationState mos in moss) {
            comboboxState.AddItem(mos.Display, mos, mos.Display);
          }
        }
      }
      
      // Add "default" in machine observation states
      comboboxState.InsertItem("Default (24x7)", null, 0);
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      // List of all initial goals. A check is made during the validation for stale exceptions.
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          data.Store(Item.INITIAL_GOALS, ModelDAOHelper.DAOFactory.GoalDAO.FindByType(
            data.Get<IGoalType>(Item.GOAL_TYPE)));
        }
      }
      
      m_goals = data.Get<GoalManager>(Item.GOALS);
      m_goals.Load(data.Get<IGoalType>(Item.GOAL_TYPE));
      m_byDepartments = data.Get<bool>(Item.ASSIGNMENT_BY_DEPARTMENT);
      
      // Default goal filter
      CurrentGoalFilter = new GoalFilter();
      
      // Name and unit
      EmitSetTitle(Title + " (\"" + m_goals.GoalName + "\")");
      labelUnit.Text = "Unit: " + m_goals.UnitTxt;
      TreeNodeElement.Unit = m_goals.UnitTxt;
      
      // Select default machine observation state
      m_preparation = true;
      comboboxState.SelectedIndex = 0;
      m_currentMOS = null;
      m_preparation = false;
      
      // Order the machines
      treeMachines.ClearOrders();
      treeMachines.SelectedOrder = m_byDepartments ?
        treeMachines.AddOrder("", new [] {"Company", "Department", "Cell"}) :
        treeMachines.AddOrder("", new [] {"Company", "Category", "SubCategory"});
      
      ComboboxStateItemChanged("", null);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Automatically saved in GoalManager
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
    /// <param name="revisionId">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      data.Get<GoalManager>(Item.GOALS).Save(data.Get<IList<IGoal>>(Item.INITIAL_GOALS));
    }
    
    /// <summary>
    /// If the validation step is enabled, method called after the validation and after the possible progress
    /// bar linked to a revision (the user or the timeout could have canceled the progress bar but in that
    /// case a warning is displayed).
    /// Don't forget to emit "DataChangedEvent" if data changed
    /// </summary>
    /// <param name="data">data that can be processed before the page changes</param>
    public override void ProcessAfterValidation(ItemData data)
    {
      EmitDataChangedEvent(null);
    }
    
    /// <summary>
    /// If the validation step is enabled, class name of the page to go after having validated
    /// The name must be a previous page otherwise this will have no effects
    /// A null or an empty value keeps the normal behavious (the previous page is displayed after validating)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public override string GetPageAfterValidation(ItemData data)
    {
      // Page2 is skipped
      return "Page1";
    }
    #endregion // Page methods
    
    #region Private methods
    GoalFilter GetGoalFilter(IList<IDisplayable> path)
    {
      // If nothing is selected are an element "unknown"
      if (path.Count == 0 || (path[path.Count - 1] as TreeNodeElement).Displayable == null) {
        return new GoalFilter((IGoal)null);
      }

      var company = (path[0] as TreeNodeElement).Displayable as ICompany;
      if (m_byDepartments) {
        IDepartment department = (path.Count > 1 ? (path[1] as TreeNodeElement).Displayable as IDepartment : null);
        ICell cell = (path.Count > 2 ? (path[2] as TreeNodeElement).Displayable as ICell : null);
        return new GoalFilter(company, department, cell);
      } else {
        IMachineCategory category = (path.Count > 1 ? (path[1] as TreeNodeElement).Displayable as IMachineCategory : null);
        IMachineSubCategory subCategory = (path.Count > 2 ? (path[2] as TreeNodeElement).Displayable as IMachineSubCategory : null);
        return new GoalFilter(company, category, subCategory);
      }
    }
    
    void DisplayGoal(TreeNodeElement element)
    {
      m_preparation = true;
      
      if (element != null) {
        if (element.Displayable == null) {
          // Unknown element
          checkOverwrite.Checked = false;
          checkOverwrite.Enabled = numericGoal.Enabled = false;
        } else {
          // Fill with the current value
          checkOverwrite.Enabled = true;
          var goals = m_goals.GetGoals(CurrentGoalFilter);
          if (goals.ContainsKey(m_currentMOS)) {
            checkOverwrite.Checked = true;
            numericGoal.Value = (decimal)goals[m_currentMOS];
            numericGoal.Enabled = true;
          } else {
            checkOverwrite.Checked = false;
            numericGoal.Value = (decimal)(element.Value.HasValue ? element.Value.Value : 0);
            numericGoal.Enabled = false;
          }
        }
      } else {
        // Reset the controls
        checkOverwrite.Checked = false;
        numericGoal.Value = element == null ? 0 : (decimal)(element.Value.HasValue ? element.Value.Value : 0);
        checkOverwrite.Enabled = numericGoal.Enabled = false;
      }
      
      m_preparation = false;
    }
    #endregion // Private methods
    
    #region Event reactions
    void TreeSelectionChanged()
    {
      if (m_preparation) {
        return;
      }

      IList<IList<IDisplayable>> paths = treeMachines.SelectedPaths;
      IList<IDisplayable> elements = treeMachines.SelectedElements;
      
      // Create the filter
      TreeNodeElement tne = null;
      if (elements.Count == 1) {
        // A machine has been selected
        tne = elements[0] as TreeNodeElement;
        CurrentGoalFilter = new GoalFilter(tne.Displayable as IMachine);
      } else if (paths.Count == 1) {
        // A node has been selected
        CurrentGoalFilter = GetGoalFilter(paths[0]);
        if (paths[0].Count > 0) {
          tne = paths[0][paths[0].Count - 1] as TreeNodeElement;
          if (tne.Displayable == null) {
            CurrentGoalFilter = new GoalFilter((IGoal)null);
          }
        }
      } else {
        CurrentGoalFilter = new GoalFilter((IGoal)null);
      }

      // Display the goals
      DisplayGoal (tne);
    }
    
    void CheckOverwriteCheckedChanged(object sender, EventArgs e)
    {
      if (!m_preparation) {
        m_goals.Modify(m_currentMOS, checkOverwrite.Checked, (double)numericGoal.Value);
        numericGoal.Enabled = checkOverwrite.Checked;
        ComboboxStateItemChanged("", m_currentMOS);
      }
    }
    
    void NumericGoalValueChanged(object sender, EventArgs e)
    {
      if (!m_preparation) {
        m_goals.Modify(m_currentMOS, checkOverwrite.Checked, (double)numericGoal.Value);
        ComboboxStateItemChanged("", m_currentMOS);
      }
    }
    
    void ComboboxStateItemChanged(string arg1, object arg2)
    {
      bool oldPrepState = m_preparation;
      m_preparation = true;
      m_currentMOS = (IMachineObservationState)arg2;
      
      // Default value (level 0)
      double? defaultValue = null;
      var goals = m_goals.GetGoals(new GoalFilter());
      if (goals.ContainsKey(m_currentMOS)) {
        defaultValue = goals[m_currentMOS];
        numericDefault.Value = (decimal)defaultValue;
        stackedWidget.SelectedIndex = 0;
        checkEnable.Checked = true;
      } else {
        stackedWidget.SelectedIndex = 1;
        checkEnable.Checked = false;
      }
      
      // Scan all items in the tree
      // Level 1: companies
      foreach (TreeNode nodeCompany in treeMachines.TreeView.Nodes) {
        var company = nodeCompany.Tag as TreeNodeElement;
        goals = m_goals.GetGoals(new GoalFilter(company.Displayable as ICompany,
                                                (IDepartment)null, null));
        double? currentDefaultValue1 = defaultValue;
        if (goals.ContainsKey(m_currentMOS)) {
          currentDefaultValue1 = goals[m_currentMOS];
          company.IsBlue = true;
        } else {
          company.IsBlue = false;
        }

        company.Value = currentDefaultValue1;
        
        // Level 2: departments or category
        foreach (TreeNode nodeLevel2 in nodeCompany.Nodes) {
          double? currentDefaultValue2 = currentDefaultValue1;
          var depOrCat = nodeLevel2.Tag as TreeNodeElement;
          goals = depOrCat.Displayable == null ?
            new NullableDictionary<IMachineObservationState, double>() :
            (m_byDepartments ?
             m_goals.GetGoals(new GoalFilter(company.Displayable as ICompany,
                                             depOrCat.Displayable as IDepartment, null)) :
             m_goals.GetGoals(new GoalFilter(company.Displayable as ICompany,
                                             depOrCat.Displayable as IMachineCategory, null)));
          if (goals.ContainsKey(m_currentMOS)) {
            currentDefaultValue2 = goals[m_currentMOS];
            string s1 = (company.Displayable as ICompany).Display;
            string s2 = (depOrCat.Displayable as IDepartment).Display;
            depOrCat.IsBlue = true;
          } else {
            depOrCat.IsBlue = false;
          }

          depOrCat.Value = currentDefaultValue2;
          
          // Level 3: cell or subcategory
          foreach (TreeNode nodeLevel3 in nodeLevel2.Nodes) {
            double? currentDefaultValue3 = currentDefaultValue2;
            var cellOrSubCat = nodeLevel3.Tag as TreeNodeElement;
            goals = depOrCat.Displayable == null || cellOrSubCat.Displayable == null ?
              new NullableDictionary<IMachineObservationState, double>() :
              m_byDepartments ?
              m_goals.GetGoals(new GoalFilter(company.Displayable as ICompany,
                                              depOrCat.Displayable as IDepartment,
                                              cellOrSubCat.Displayable as ICell)) :
              m_goals.GetGoals(new GoalFilter(company.Displayable as ICompany,
                                              depOrCat.Displayable as IMachineCategory,
                                              cellOrSubCat.Displayable as IMachineSubCategory));
            if (goals.ContainsKey(m_currentMOS)) {
              currentDefaultValue3 = goals[m_currentMOS];
              cellOrSubCat.IsBlue = true;
            } else {
              cellOrSubCat.IsBlue = false;
            }

            cellOrSubCat.Value = currentDefaultValue3;
            
            // Level 4: machines
            foreach (TreeNode nodeMachine in nodeLevel3.Nodes) {
              double? currentDefaultValue4 = currentDefaultValue3;
              var treeElement = nodeMachine.Tag as TreeElement;
              goals = m_goals.GetGoals(new GoalFilter(treeElement.Displayable as IMachine));
              if (goals.ContainsKey(m_currentMOS)) {
                currentDefaultValue4 = goals[m_currentMOS];
                treeElement.IsBlue = true;
              } else {
                treeElement.IsBlue = false;
              }
              treeElement.Value = currentDefaultValue4;
            }
          }
        }
      }
      
      m_preparation = oldPrepState;
      using (new SuspendDrawing(treeMachines)) {
        treeMachines.RefreshTreeview();
      }
    }
    
    void CheckEnableCheckedChanged(object sender, EventArgs e)
    {
      if (!m_preparation) {
        m_goals.ModifyDefault(m_currentMOS, checkEnable.Checked, (double)numericDefault.Value);
        ComboboxStateItemChanged("", m_currentMOS);
      }
    }
    
    void NumericDefaultValueChanged(object sender, EventArgs e)
    {
      if (!m_preparation) {
        m_goals.ModifyDefault(m_currentMOS, checkEnable.Checked, (double)numericDefault.Value);
        ComboboxStateItemChanged("", m_currentMOS);
      }
    }
    #endregion // Event reactions
  }
}
