// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IXxx.
  /// </summary>
  public interface IToolDAO: IGenericUpdateDAO<ITool, int>
  {   
    /// <summary>
    /// Find tool by code (unique)
    /// </summary>
    /// <param name="toolCode"></param>
    /// <returns></returns>
    ITool FindByCode(string toolCode);
    
    /// <summary>
    /// Find tool by name (non-unique)
    /// </summary>
    /// <param name="toolName"></param>
    /// <returns></returns>
    IList<ITool> FindByName(string toolName);
  }
}
