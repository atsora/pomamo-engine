// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace ConfiguratorGoals
{
  /// <summary>
  /// Description of GoalManager.
  /// </summary>
  public class GoalManager
  {
    #region Members
    IGoalType m_goalType;
    IDictionary<GoalFilter, NullableDictionary<IMachineObservationState, double>> m_goals = new Dictionary<GoalFilter, NullableDictionary<IMachineObservationState, double>>();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (GoalManager).FullName);

    #region Getters / Setters
    /// <summary>
    /// Unit of the goal type
    /// </summary>
    public string UnitTxt { get; private set; }
    
    /// <summary>
    /// Name of the goal
    /// </summary>
    public string GoalName { get; private set; }
    
    /// <summary>
    /// Current goal filter
    /// </summary>
    public GoalFilter CurrentGoalFilter { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public GoalManager() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Load configured goals of a specified type
    /// Reset all previous modified goals
    /// </summary>
    public void Load(IGoalType goalType)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          m_goalType = goalType;
          
          // Goals
          m_goals.Clear();
          if (m_goalType != null) {
            ModelDAOHelper.DAOFactory.GoalTypeDAO.Lock(m_goalType);
            IList<IGoal> goals = ModelDAOHelper.DAOFactory.GoalDAO.FindByType(m_goalType);
            foreach (IGoal goal in goals) {
              var filter = new GoalFilter(goal);
              if (!m_goals.ContainsKey(filter)) {
                m_goals[filter] = new NullableDictionary<IMachineObservationState, double>();
              }

              if (goal.MachineObservationState == null) {
                m_goals[filter][null] = goal.Value;
              }
              else {
                m_goals[filter][goal.MachineObservationState] = goal.Value;
              }
            }
            
            // Unit
            IUnit unit = m_goalType.Unit;
            if (unit == null) {
              UnitTxt = "-";
            }
            else {
              ModelDAOHelper.DAOFactory.UnitDAO.Lock(unit);
              UnitTxt = unit.Display;
            }
            
            // Type
            GoalName = m_goalType.Display;
          } else {
            UnitTxt = "-";
          }
        }
      }
    }
    
    /// <summary>
    /// Get the goals matching a filter
    /// </summary>
    /// <param name="goalFilter"></param>
    /// <returns></returns>
    public NullableDictionary<IMachineObservationState, double> GetGoals(GoalFilter goalFilter)
    {
      return goalFilter.Valid && m_goals.ContainsKey(goalFilter) ? m_goals[goalFilter] :
        new NullableDictionary<IMachineObservationState, double>();
    }
    
    /// <summary>
    /// Save all modifications in the database
    /// </summary>
    /// <param name="previousGoals"></param>
    public void Save(IList<IGoal> previousGoals)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction()) {
          if (ModelDAOHelper.DAOFactory.GoalDAO.FindByType(m_goalType).Count != previousGoals.Count) {
            throw new Lemoine.Settings.StaleException("Goals have been deleted or removed");
          }
        }
      }
      
      var defaultGoalFilter = new GoalFilter();
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession()) {
        using (IDAOTransaction transaction = session.BeginTransaction()) {
          ModelDAOHelper.DAOFactory.GoalTypeDAO.Lock(m_goalType);
          
          // List of default filters that have been enabled
          var defaultGoalFiltersEnabled = new NullableDictionary<IMachineObservationState, double>();
          if (m_goals.ContainsKey(defaultGoalFilter)) {
            foreach (var elt in m_goals[defaultGoalFilter]) {
              defaultGoalFiltersEnabled[elt.Key] = 0.0; // The value is not important
            }
          }
          
          // Existing goals are updated or deleted
          foreach (IGoal goal in previousGoals) {
            ModelDAOHelper.DAOFactory.GoalDAO.Lock(goal);
            var goalFilter = new GoalFilter(goal);
            IMachineObservationState machineObservationState = goal.MachineObservationState;
            if (m_goals.ContainsKey(goalFilter)) {
              if (m_goals[goalFilter].ContainsKey(machineObservationState) &&
                  defaultGoalFiltersEnabled.ContainsKey(machineObservationState)) {
                // Update
                goal.Value = m_goals[goalFilter][machineObservationState];
                m_goals[goalFilter].Remove(machineObservationState);
              } else // Deletion, no default filter for a machine state
{
                ModelDAOHelper.DAOFactory.GoalDAO.MakeTransient(goal);
              }
            } else // No goals in the same goal filter or no default filter enabled for a specific MOS
{
              ModelDAOHelper.DAOFactory.GoalDAO.MakeTransient(goal);
            }
          }
          
          // New goals
          foreach (GoalFilter goalFilter in m_goals.Keys) {
            foreach (IMachineObservationState machineObservationState in m_goals[goalFilter].Keys) {
              
              if (defaultGoalFiltersEnabled.ContainsKey(machineObservationState)) {
                // Creation of a new goal
                IGoal newGoal = ModelDAOHelper.ModelFactory.CreateGoal(m_goalType);
                ModelDAOHelper.DAOFactory.GoalDAO.MakePersistent(newGoal);
                
                // Filters
                goalFilter.PrepareGoal(newGoal);
                if (machineObservationState != null) {
                  newGoal.MachineObservationState = machineObservationState;
                }
                
                // Value
                newGoal.Value = m_goals[goalFilter][machineObservationState];
              }
            }
          }
          
          transaction.Commit();
        }
      }
    }
    
    /// <summary>
    /// Modify a value
    /// </summary>
    /// <param name="machineObservationState"></param>
    /// <param name="isSet"></param>
    /// <param name="value"></param>
    public void Modify(IMachineObservationState machineObservationState, bool isSet, double value)
    {
      if (!m_goals.ContainsKey(CurrentGoalFilter)) {
        m_goals[CurrentGoalFilter] = new NullableDictionary<IMachineObservationState, double>();
      }

      if (isSet) {
        m_goals[CurrentGoalFilter][machineObservationState] = value;
      }
      else {
        m_goals[CurrentGoalFilter].Remove(machineObservationState);
      }
    }
    
    /// <summary>
    /// Modify a default value
    /// </summary>
    /// <param name="machineObservationState"></param>
    /// <param name="isSet"></param>
    /// <param name="value"></param>
    public void ModifyDefault(IMachineObservationState machineObservationState, bool isSet, double value)
    {
      var defaultFilter = new GoalFilter();
      if (!m_goals.ContainsKey(defaultFilter)) {
        m_goals[defaultFilter] = new NullableDictionary<IMachineObservationState, double>();
      }

      if (isSet) {
        m_goals[defaultFilter][machineObservationState] = value;
      }
      else {
        m_goals[defaultFilter].Remove(machineObservationState);
      }
    }
    
    /// <summary>
    /// Textual description of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return base.ToString();
    }
    #endregion // Methods
  }
}
