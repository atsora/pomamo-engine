// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of Page1.
  /// </summary>
  internal partial class Page1 : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    IGoalType m_goalType = null;
    bool m_withDepartment = false;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Type of goal"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help {
      get {
        return "Choose here the type of goals and the kind of action you want. " +
          "By clicking on \"Edit\", the page \"goal assignment\" or \"goal definition\" " +
          "will be displayed depending on if the categorization of the machines is " +
          "defined or not.\n\n" +
          "By clicking on \"Reset\", the page \"Reset Goals\" will appear. " +
          "All goals of a specific type will be removed (along with the categorization " +
          "of the machines regarding this type of goal), except for the default values " +
          "per company.";
      }
    }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public override IList<Type> EditableTypes {
      get {
        IList<Type> types = new List<Type>();
        types.Add(typeof(IGoalType));
        return types;
      }
    }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get { return LemSettingsGlobal.PageFlag.DONT_SHOW_SUCCESS_INFORMATION; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Page1()
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
      // "Edit" and "reset" buttons for all goal types
      verticalScroll.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          foreach (IGoalType goalType in ModelDAOHelper.DAOFactory.GoalTypeDAO.FindAll()) {
            var cell = new CellGoalType(goalType, goalType.Display);
            cell.EditClicked += ButtonEditClicked;
            cell.ResetClicked += ButtonResetClicked;
            cell.Dock = DockStyle.Fill;
            cell.Margin = new Padding(0);
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
      // Nothing to load
      EmitProtectAgainstQuit(false);
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      data.Store(Item.GOAL_TYPE, m_goalType);
      data.Store(Item.ASSIGNMENT_BY_DEPARTMENT, m_withDepartment);
    }
    #endregion // Page methods
    
    #region Private methods
    bool IsAssignmentDefined()
    {
      bool defined = false;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          ModelDAOHelper.DAOFactory.GoalTypeDAO.Lock(m_goalType);
          IList<IGoal> goals = ModelDAOHelper.DAOFactory.GoalDAO.FindByType(m_goalType);
          
          bool withDepartmentOrCell = false;
          bool withCatOrSubCat = false;
          foreach (IGoal goal in goals) {
            if (goal.Department != null || goal.Cell != null) {
              withDepartmentOrCell = true;
            }

            if (goal.Category != null || goal.SubCategory != null) {
              withCatOrSubCat = true;
            }
          }
          
          defined = (withDepartmentOrCell && !withCatOrSubCat) ||
            (!withDepartmentOrCell && withCatOrSubCat);
          m_withDepartment = withDepartmentOrCell;
        }
      }
      
      return defined;
    }
    #endregion // Private methods
    
    #region Event reactions
    void ButtonEditClicked(IGoalType goalType)
    {
      m_goalType = goalType;
      if (IsAssignmentDefined()) {
        EmitDisplayPageEvent ("Page3", null);
      }
      else {
        EmitDisplayPageEvent ("Page2", null);
      }
    }
    
    void ButtonResetClicked(IGoalType goalType)
    {
      m_goalType = goalType;
      EmitDisplayPageEvent("PageReset", null);
    }
    #endregion // Event reactions
  }
}
