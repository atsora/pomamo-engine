// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Collections;

namespace Lemoine.Model
{
  /// <summary>
  /// List of possible goal types
  /// </summary>
  public enum GoalTypeId {
    /// <summary>
    /// Utilization %
    /// </summary>
    UtilizationPercentage = 1,
    /// <summary>
    /// Expected quantity in % VS the cycle duration during a production
    /// </summary>
    QuantityVsProductionCycleDuration = 2,
  }
  
  /// <summary>
  /// Description of IGoalType.
  /// </summary>
  public interface IGoalType: IDataWithIdentifiers, IDataWithVersion, IDataWithId<int>
  {
    /// <summary>
    /// Name
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// TranslationKey
    /// </summary>
    string TranslationKey { get; set; }

    /// <summary>
    /// Display name that is deduced from the translation table
    /// </summary>
    string Display { get; }
    
    /// <summary>
    /// Unit
    /// </summary>
    IUnit Unit { get; set; }
  }
}
