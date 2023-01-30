// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Info.ConfigReader;
using Lemoine.Core.Log;

namespace Lemoine.ModelDAO.Info
{
  /// <summary>
  /// IGenericConfigReader to read the configuration in the database in the table applicationstate
  /// 
  /// Not thread safe
  /// </summary>
  public class ApplicationStateConfigReader : IGenericConfigReader
  {
    #region Members
    readonly string m_prefix;
    volatile bool m_activeRequest = false;
    volatile bool m_initialized = false;
    readonly MemoryConfigReader m_storage = new MemoryConfigReader ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (ApplicationStateConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="initialize">Initialize immediately</param>
    public ApplicationStateConfigReader (string prefix = "", bool initialize = true)
    {
      m_prefix = prefix;
      if (initialize) {
        Initialize ();
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize the data reading the config values in database
    /// </summary>
    public void Initialize ()
    {
      if (!m_initialized && !m_activeRequest) {
        m_activeRequest = true;
        try {
          using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
            var applicationStates = ModelDAOHelper.DAOFactory.ApplicationStateDAO
              .FindAll ();
            foreach (var applicationState in applicationStates) {
              if (null == applicationState.Value) {
                if (log.IsDebugEnabled) {
                  log.Debug ($"Initialize: key {applicationState.Key} exists in database, but no value is set");
                }
                continue;
              }
              else if (applicationState.Key.StartsWith (m_prefix)) {
                m_storage.Add (applicationState.Key, applicationState.Value);
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
    public T Get<T> (string key)
    {
      Initialize ();
      return m_storage.Get<T> (key);
    }
    #endregion
  }
}
