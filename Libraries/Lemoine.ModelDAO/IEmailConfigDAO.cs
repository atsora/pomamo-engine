// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IEmailConfig.
  /// </summary>
  public interface IEmailConfigDAO: IGenericUpdateDAO<IEmailConfig, int>
  {
    /// <summary>
    /// FindAll implementation with an eager fetch of the different foreign keys
    /// for the configuration
    /// 
    /// The result is sorted by name
    /// </summary>
    /// <returns></returns>
    IList<IEmailConfig> FindAllForConfig(); 

    /// <summary>
    /// Find all active configurations (cacheable)
    /// </summary>
    /// <returns></returns>
    IList<IEmailConfig> FindActive ();
    
    /// <summary>
    /// Find all configurations that match a specified data type and machine
    /// </summary>
    /// <param name="dataType"></param>
    /// <param name="machine"></param>
    /// <returns></returns>
    IList<IEmailConfig> FindByDataTypeMachine (string dataType, IMachine machine);
  }
}
