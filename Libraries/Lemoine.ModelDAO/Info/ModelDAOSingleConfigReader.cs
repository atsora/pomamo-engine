// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.ModelDAO.Info
{
  /// <summary>
  /// IGenericConfigReader to read the configuration in the database in the table configuration
  /// one data at a time
  /// 
  /// Not thread safe
  /// </summary>
  public class ModelDAOSingleConfigReader : Lemoine.Info.ConfigReader.IGenericConfigReader
  {
    #region Members
    volatile bool m_activeRequest = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ModelDAOSingleConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ModelDAOSingleConfigReader ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IGenericConfigReader implementation
    /// <summary>
    /// Implementation of <see cref="Lemoine.Info.ConfigReader.IGenericConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      if (m_activeRequest) { // To avoid any recursion
        log.DebugFormat ("Get: " +
                         "a request is already active",
                         key);
        throw new Lemoine.Info.ConfigKeyNotFoundException (key, "a request is already active");
      }

      try {
        m_activeRequest = true;

        using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var config = ModelDAOHelper.DAOFactory.ConfigDAO.GetConfig (key);
          if (null == config) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: key {key} does not exist in database");
            }
            throw new Lemoine.Info.ConfigKeyNotFoundException (key, "key does not exist in database");
          }
          else if (null == config.Value) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: key {key} exists in database, but no value is set");
            }
            throw new Lemoine.Info.ConfigKeyNotFoundException (key, "key with no value in database");
          }
          else if (!config.Active) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: key {config.Key} is not active");
            }
            throw new Lemoine.Info.ConfigKeyNotFoundException (key, "key is not active");
          }
          else if (config.Value.ToString ().Equals (NotSetConfigValue.String, StringComparison.InvariantCultureIgnoreCase)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"Get: key {config.Key} exists in database, but its value is not set");
            }
            throw new Lemoine.Info.ConfigKeyNotFoundException (key, "value is not set");
          }
          else {
            return (T)config.Value;
          }
        }
      }
      finally {
        m_activeRequest = false;
      }
    }
    #endregion
  }
}
