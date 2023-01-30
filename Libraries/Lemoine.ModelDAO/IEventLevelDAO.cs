// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEventLevel.
  /// </summary>
  public interface IEventLevelDAO: IGenericUpdateDAO<IEventLevel, int>
  {
    /// <summary>
    /// Return all IEventLevel for the Config page
    /// </summary>
    /// <returns>IList&lt;IEventLevel&gt;</returns>
    IList<IEventLevel> FindAllForConfig();
    
    /// <summary>
    /// Return all IEventLevel that match a specified priority
    /// </summary>
    /// <param name="priority"></param>
    /// <returns></returns>
    IList<IEventLevel> FindByPriority (int priority);
    
    /// <summary>
    /// Test if a EvenLevel is linked to any data
    /// </summary>
    /// <param name="eventLevel"></param>
    /// <returns>bool</returns>
    bool IsEventLevelUsed(IEventLevel eventLevel);
  }
}
