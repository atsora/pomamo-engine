// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEventCncValueConfig.
  /// </summary>
  public interface IEventCncValueConfigDAO: IGenericUpdateDAO<IEventCncValueConfig, int>
  {
    /// <summary>
    /// Find by name
    /// 
    /// Fetch early its properties
    /// 
    /// null if not found
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IEventCncValueConfig FindByName (string name);
    
    /// <summary>
    /// Find All for Config with FetchEager
    /// </summary>
    /// <returns></returns>
    IList<IEventCncValueConfig> FindAllForConfig();    
    
    /// <summary>
    /// Get the used IEventLevel in EventCncValueConfig
    /// </summary>
    /// <returns></returns>
    IList<IEventLevel> GetLevels ();  
  }
}
