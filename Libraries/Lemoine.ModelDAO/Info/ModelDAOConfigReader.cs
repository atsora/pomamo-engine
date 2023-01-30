// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Info.ConfigReader;
using Lemoine.Core.Log;
using System.Threading;

namespace Lemoine.ModelDAO.Info
{
  /// <summary>
  /// IGenericConfigReader to read the configuration in the database in the table configuration
  /// 
  /// Not thread safe
  /// </summary>
  public class ModelDAOConfigReader: IGenericConfigReader
  {
    #region Members
    volatile bool m_activeRequest = false;
    volatile bool m_initialized = false;
    readonly MemoryConfigReader m_storage = new MemoryConfigReader ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ModelDAOConfigReader).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ModelDAOConfigReader ()
    {
    }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="initialize">Initialize immediately</param>
    public ModelDAOConfigReader (bool initialize)
    {
      if (initialize) {
        Initialize ();
      }
    }

    #region Methods
    /// <summary>
    /// Initialize the data reading the config values in database
    /// </summary>
    public void Initialize (CancellationToken cancellationToken = default)
    {
      if (!m_initialized && !m_activeRequest) {
        m_activeRequest = true;
        try {
          if (log.IsDebugEnabled) {
            log.Debug ($"Initialize");
          }
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
          {
            var configs = ModelDAOHelper.DAOFactory.ConfigDAO
              .FindAll ();
            foreach (var config in configs) {
              if (cancellationToken.IsCancellationRequested) {
                log.Info ($"Initialize: cancellation requested");
                return;
              }
              if (null == config.Value) {
                log.Debug ($"Initialize: key {config.Key} exists in database, but no value is set");
                continue;
              }
              else if (!config.Active) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Initialize: key {config.Key} is not active");
                }
                continue;
              }
              else if (config.Value.ToString ().Equals (NotSetConfigValue.String, StringComparison.InvariantCultureIgnoreCase)) {
                log.Debug ($"Initialize: key {config.Key} exists in database, but its value is not set");
                continue;
              }
              else {
                m_storage.Add (config.Key, config.Value);
              }
            }
          }
          
        }
        finally {
          m_activeRequest = false;
        }

        m_initialized = true;
      }
    }
    #endregion // Methods

    #region IGenericConfigReader implementation
    /// <summary>
    /// Implementation of <see cref="Lemoine.Info.ConfigReader.IGenericConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(string key)
    {
      Initialize ();
      return m_storage.Get<T> (key);
    }
    #endregion
  }
}
