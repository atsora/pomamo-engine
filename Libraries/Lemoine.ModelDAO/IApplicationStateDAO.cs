// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IApplicationState.
  /// </summary>
  public interface IApplicationStateDAO: IGenericUpdateDAO<IApplicationState, int>
  {
    /// <summary>
    /// Get the application state for the specified key
    /// 
    /// Note: this request may be cached
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IApplicationState GetApplicationState (string key);
    
    /// <summary>
    /// Get the applicationState for the specified key without any cache
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IApplicationState GetNoCache (string key);

    /// <summary>
    /// Create or update directly an application state value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    void Update (string key, object v);
  }
}
