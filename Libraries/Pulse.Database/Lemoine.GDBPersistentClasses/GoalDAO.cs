// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IGoalDAO">IGoalDAO</see>
  /// </summary>
  public class GoalDAO
    : VersionableNHibernateDAO<Goal, IGoal, int>
    , IGoalDAO
  {
    /// <summary>
    /// Find all goals by type
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <returns></returns>
    public IList<IGoal> FindByType(IGoalType goalType)
    {
      if (goalType == null) {
        throw new ArgumentNullException("goalType");
      }

      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Goal> ()
        .Add (Restrictions.Eq ("Type", goalType))
        .List<IGoal>();
    }
    
    /// <summary>
    /// Find goals by its type and machine attributes (company, department, cell)
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <param name="company">may be null</param>
    /// <param name="department">may be null</param>
    /// <param name="cell">may be null</param>
    /// <returns></returns>
    public IList<IGoal> FindByTypeAndMachineAttributes(IGoalType goalType, ICompany company, IDepartment department, ICell cell)
    {
      if (goalType == null) {
        throw new ArgumentNullException("goalType");
      }

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Goal> ()
        .Add (Restrictions.Eq ("Type", goalType));
      
      // Company
      if (company == null) {
        criteria = criteria.Add(Restrictions.IsNull("Company"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("Company", company));
      }

      // Department
      if (department == null) {
        criteria = criteria.Add(Restrictions.IsNull("Department"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("Department", department));
      }

      // Cell
      if (cell == null) {
        criteria = criteria.Add(Restrictions.IsNull("Cell"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("Cell", cell));
      }

      return criteria.List<IGoal>();
    }
    
    /// <summary>
    /// Find goals by its type and machine attributes (company, category, subcategory)
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <param name="company">may be null</param>
    /// <param name="category">may be null</param>
    /// <param name="subcategory">may be null</param>
    /// <returns></returns>
    public IList<IGoal> FindByTypeAndMachineAttributes(IGoalType goalType, ICompany company, IMachineCategory category, IMachineSubCategory subcategory)
    {
      if (goalType == null) {
        throw new ArgumentNullException("goalType");
      }

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Goal> ()
        .Add (Restrictions.Eq ("Type", goalType));
      
      // Company
      if (company == null) {
        criteria = criteria.Add(Restrictions.IsNull("Company"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("Company", company));
      }

      // Category
      if (category == null) {
        criteria = criteria.Add(Restrictions.IsNull("Category"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("Category", category));
      }

      // SubCategory
      if (subcategory == null) {
        criteria = criteria.Add(Restrictions.IsNull("SubCategory"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("SubCategory", subcategory));
      }

      return criteria.List<IGoal>();
    }
    
    /// <summary>
    /// Find goals by its type and the machine
    /// </summary>
    /// <param name="goalTypeId"></param>
    /// <param name="machine">may be null</param>
    /// <returns></returns>
    public IList<IGoal> FindByTypeAndMachine(GoalTypeId goalTypeId, IMachine machine)
    {
      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Goal> ()
        .Add (Restrictions.Eq ("Type.Id", (int)goalTypeId));
      
      // Machine
      if (machine == null) {
        criteria = criteria.Add(Restrictions.IsNull("Machine"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("Machine", machine));
      }

      return criteria.List<IGoal>();
    }

    /// <summary>
    /// Find goals by its type and the machine
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <param name="machine">may be null</param>
    /// <returns></returns>
    public IList<IGoal> FindByTypeAndMachine(IGoalType goalType, IMachine machine)
    {
      if (goalType == null) {
        throw new ArgumentNullException("goalType");
      }

      var criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Goal> ()
        .Add (Restrictions.Eq ("Type", goalType));
      
      // Machine
      if (machine == null) {
        criteria = criteria.Add(Restrictions.IsNull("Machine"));
      }
      else {
        criteria = criteria.Add(Restrictions.Eq ("Machine", machine));
      }

      return criteria.List<IGoal>();
    }
    
    /// <summary>
    /// Find the goal that matches a type, a machine observation state (nullable) and a machine (not null)
    /// </summary>
    /// <returns></returns>
    /// <param name="goalTypeId"></param>
    /// <param name="machineObservationState">nullable</param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    public IGoal FindMatch (GoalTypeId goalTypeId, IMachineObservationState machineObservationState, IMachine machine)
    {
      Debug.Assert (null != machine);
      
      ICriteria criteria = NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<Goal> ()
        .Add (Restrictions.Eq ("Type.Id", (int)goalTypeId));
      if (null == machineObservationState) {
        criteria.Add (Restrictions.IsNull ("MachineObservationState"));
      }
      else {
        criteria.Add (Restrictions.Eq ("MachineObservationState", machineObservationState));
      }
      criteria.Add (Restrictions.Or (Restrictions.IsNull ("Machine"), Restrictions.Eq ("Machine", machine)));
      if (null != machine.Cell) {
        criteria.Add (Restrictions.Or (Restrictions.IsNull ("Cell"), Restrictions.Eq ("Cell", machine.Cell)));
      }
      else {
        criteria.Add (Restrictions.IsNull ("Cell"));
      }
      if (null != machine.SubCategory) {
        criteria.Add (Restrictions.Or (Restrictions.IsNull ("SubCategory"), Restrictions.Eq ("SubCategory", machine.SubCategory)));
      }
      else {
        criteria.Add (Restrictions.IsNull ("SubCategory"));
      }
      if (null != machine.Category) {
        criteria.Add (Restrictions.Or (Restrictions.IsNull ("Category"), Restrictions.Eq ("Category", machine.Category)));
      }
      else {
        criteria.Add (Restrictions.IsNull ("Category"));
      }
      if (null != machine.Department) {
        criteria.Add (Restrictions.Or (Restrictions.IsNull ("Department"), Restrictions.Eq ("Department", machine.Department)));
      }
      else {
        criteria.Add (Restrictions.IsNull ("Department"));
      }
      if (null != machine.Company) {
        criteria.Add (Restrictions.Or (Restrictions.IsNull ("Company"), Restrictions.Eq ("Company", machine.Company)));
      }
      else {
        criteria.Add (Restrictions.IsNull ("Company"));
      }
      criteria.AddOrder (Order.Asc ("Machine"))
        .AddOrder (Order.Asc ("Cell"))
        .AddOrder (Order.Asc ("SubCategory"))
        .AddOrder (Order.Asc ("Category"))
        .AddOrder (Order.Asc ("Department"))
        .AddOrder (Order.Asc ("Company"));
      criteria.SetMaxResults (1);
      return criteria.UniqueResult<IGoal> ();
    }
  }
}
