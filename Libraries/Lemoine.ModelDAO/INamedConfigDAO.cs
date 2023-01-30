// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for INamedConfig.
  /// </summary>
  public interface INamedConfigDAO: IGenericUpdateDAO<INamedConfig, int>
  {
    /// <summary>
    /// Get the list of names for the given key pattern (with the SQL like syntax)
    /// </summary>
    /// <param name="keyPattern"></param>
    /// <returns></returns>
    IList<string> GetNames (string keyPattern);
    
    /// <summary>
    /// Get the config for the specified key and name
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    INamedConfig GetConfig (string name, string key);

    /// <summary>
    /// Get a set of configs for the specified named and key pattern (with the SQL like syntax)
    /// </summary>
    /// <param name="name"></param>
    /// <param name="keyPattern"></param>
    /// <returns></returns>
    IList<INamedConfig> GetConfigs (string name, string keyPattern);

    /// <summary>
    /// Force a configuration, without first checking the previous configuration value
    /// </summary>
    /// <param name="name"></param>
    /// <param name="key"></param>
    /// <param name="v"></param>
    void SetConfig (string name, string key, object v);
  }
}
