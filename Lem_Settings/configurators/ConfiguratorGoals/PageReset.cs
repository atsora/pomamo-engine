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

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of PageReset.
  /// </summary>
  public partial class PageReset : GenericConfiguratorPage, IConfiguratorPage
  {
    #region Members
    IGoalType m_goalType = null;
    IList<CellResetGoal> m_cells = new List<CellResetGoal>();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Title
    /// </summary>
    public string Title { get { return "Reset goals"; } }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    public string Help { get { return "Reset all the values defined for a specific type of goals.\n\n" +
          "You must however specify a default value for each company."; } }
    
    /// <summary>
    /// Characterization of the page (see the documentation of the PageFlags)
    /// </summary>
    public override LemSettingsGlobal.PageFlag Flags {
      get {
        return LemSettingsGlobal.PageFlag.WITH_VALIDATION;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public PageReset()
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
    }
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    public void LoadPageFromData(ItemData data)
    {
      m_goalType = data.Get<IGoalType>(Item.GOAL_TYPE);
      
      // Load the default values per company
      verticalScroll.Clear();
      m_cells.Clear();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          var companies = ModelDAOHelper.DAOFactory.CompanyDAO.FindAll().OrderBy(o => o.Display);
          foreach (var company in companies) {
            double defaultValue = 50;
            var goals = ModelDAOHelper.DAOFactory.GoalDAO.FindByTypeAndMachineAttributes(
              m_goalType, company, (IDepartment)null, null);
            foreach (var goal in goals) {
              if (goal.Category == null && goal.Cell == null && goal.Department == null &&
                  goal.SubCategory == null && goal.Machine == null && goal.MachineObservationState == null) {
                defaultValue = goal.Value;
                break;
              }
            }
            
            var cell = new CellResetGoal(company, defaultValue);
            m_cells.Add(cell);
            
            cell.Dock = DockStyle.Fill;
            verticalScroll.AddControl(cell);
          }
        }
      }
    }
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    public void SavePageInData(ItemData data)
    {
      // Save the default goals
      var goals = data.Get<IDictionary<ICompany, double>>(Item.GOALS_AFTER_RESET);
      goals.Clear();
      foreach (var cell in m_cells) {
        goals[cell.Company] = cell.Value;
      }
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
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    public override void Validate(ItemData data, ref IList<string> warnings, ref int revisionId)
    {
      var goals = data.Get<IDictionary<ICompany, double>>(Item.GOALS_AFTER_RESET);
      var goalType = data.Get<IGoalType>(Item.GOAL_TYPE);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction())
        {
          ModelDAOHelper.DAOFactory.GoalTypeDAO.Lock(goalType);
          
          // Clear all goals for a specified type
          var previousGoals = ModelDAOHelper.DAOFactory.GoalDAO.FindByType(goalType);
          foreach (var goal in previousGoals) {
            ModelDAOHelper.DAOFactory.GoalDAO.MakeTransient(goal);
          }

          // Add default goals
          foreach (var company in goals.Keys) {
            var newGoal = ModelDAOHelper.ModelFactory.CreateGoal(goalType);
            newGoal.Company = company;
            newGoal.Value = goals[company];
            ModelDAOHelper.DAOFactory.GoalDAO.MakePersistent(newGoal);
          }
          
          transaction.Commit();
        }
      }
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
      return "Page1";
    }
    #endregion // Page methods
  }
}
