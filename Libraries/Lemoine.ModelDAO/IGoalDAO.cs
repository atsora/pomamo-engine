// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IGoal.
  /// </summary>
  public interface IGoalDAO: IGenericDAO<IGoal, int>
  {
    /// <summary>
    /// Find all goals by type
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <returns></returns>
    IList<IGoal> FindByType(IGoalType goalType);
    
    /// <summary>
    /// Find goals by its type and machine attributes (company, department, cell)
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <param name="company">may be null</param>
    /// <param name="department">may be null</param>
    /// <param name="cell">may be null</param>
    /// <returns></returns>
    IList<IGoal> FindByTypeAndMachineAttributes(IGoalType goalType, ICompany company, IDepartment department, ICell cell);
    
    /// <summary>
    /// Find goals by its type and machine attributes (company, category, subcategory)
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <param name="company">may be null</param>
    /// <param name="category">may be null</param>
    /// <param name="subcategory">may be null</param>
    /// <returns></returns>
    IList<IGoal> FindByTypeAndMachineAttributes(IGoalType goalType, ICompany company, IMachineCategory category, IMachineSubCategory subcategory);
    
    /// <summary>
    /// Find goals by its type and the machine
    /// </summary>
    /// <param name="goalTypeId"></param>
    /// <param name="machine">may be null</param>
    /// <returns></returns>
    IList<IGoal> FindByTypeAndMachine(GoalTypeId goalTypeId, IMachine machine);

    /// <summary>
    /// Find goals by its type and the machine
    /// </summary>
    /// <param name="goalType">not null</param>
    /// <param name="machine">may be null</param>
    /// <returns></returns>
    IList<IGoal> FindByTypeAndMachine(IGoalType goalType, IMachine machine);
    
    /// <summary>
    /// Find the goal that matches a type, a machine observation state (nullable) and a machine (not null)
    /// </summary>
    /// <returns></returns>
    /// <param name="goalTypeId"></param>
    /// <param name="machineObservationState">nullable</param>
    /// <param name="machine">not null</param>
    /// <returns></returns>
    IGoal FindMatch (GoalTypeId goalTypeId, IMachineObservationState machineObservationState, IMachine machine);
  }
}
