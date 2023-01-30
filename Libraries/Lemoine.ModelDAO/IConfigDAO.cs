// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IConfig.
  /// </summary>
  public interface IConfigDAO : IGenericUpdateDAO<IConfig, int>
  {
    /// <summary>
    /// Get the config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetConfig (string key);

    /// <summary>
    /// Get the analysis config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetAnalysisConfig (AnalysisConfigKey key);

    /// <summary>
    /// Get the global config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetCalendarConfig (CalendarConfigKey key);

    /// <summary>
    /// Get the CNC config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetCncConfig (CncConfigKey key);

    /// <summary>
    /// Get the operation explorer value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    object GetOperationExplorerConfigValue (OperationExplorerConfigKey key);

    /// <summary>
    /// Get the operation explorer config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetOperationExplorerConfig (OperationExplorerConfigKey key);

    /// <summary>
    /// Get the analysis config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    object GetAnalysisConfigValue (AnalysisConfigKey key);

    /// <summary>
    /// Get the CNC config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    object GetCncConfigValue (CncConfigKey key);

    /// <summary>
    /// Get the datastructure config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    object GetDataStructureConfigValue (DataStructureConfigKey key);

    /// <summary>
    /// Get the datastructure config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetDataStructureConfig (DataStructureConfigKey key);

    /// <summary>
    /// Get the Webservice config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    object GetWebServiceConfigValue (WebServiceConfigKey key);

    /// <summary>
    /// Get the Webservice config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetWebServiceConfig (WebServiceConfigKey key);

    /// <summary>
    /// Get the Dbm config value for the specified key.
    /// 
    /// If the key is not found, an exception is raised.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    object GetDbmConfigValue (DbmConfigKey key);

    /// <summary>
    /// Get the Dbm config for the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    IConfig GetDbmConfig (DbmConfigKey key);

    /// <summary>
    /// Force a configuration, without first checking the previous configuration value
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="activated"></param>
    void SetConfig (string key, object v, bool activated);

    /// <summary>
    /// Find the config where the key is like a given filter
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    IList<IConfig> FindLike (string filter);
  }
}
