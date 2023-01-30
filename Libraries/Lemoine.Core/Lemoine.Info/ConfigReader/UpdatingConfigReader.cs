// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Config reader that can be udpated at any time
  /// 
  /// Thread safe
  /// </summary>
  public sealed class UpdatingConfigReader : IGenericConfigReader, IPersistentConfigWriter
  {
    #region Members
    IGenericConfigReader m_configReader;
    IPersistentConfigWriter m_configWriter;
    readonly object m_configReaderLock = new object ();
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (UpdatingConfigReader).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public UpdatingConfigReader ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="configReader"></param>
    /// <param name="configWriter">optional</param>
    public UpdatingConfigReader (IGenericConfigReader configReader, IPersistentConfigWriter configWriter = null)
    {
      m_configReader = configReader;
      m_configWriter = configWriter;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a config reader
    /// </summary>
    /// <param name="configReader"></param>
    /// <param name="configWriter"></param>
    public void Set (IGenericConfigReader configReader, IPersistentConfigWriter configWriter = null)
    {
      lock (m_configReaderLock) {
        m_configReader = configReader;
        m_configWriter = configWriter;
      }
    }

    /// <summary>
    /// Add a persistent config writer
    /// </summary>
    /// <param name="configWriter"></param>
    public void SetConfigWriter (IPersistentConfigWriter configWriter)
    {
      lock (m_configReaderLock) {
        m_configWriter = configWriter;
      }
    }
    #endregion // Methods

    #region IGenericConfigReader implementation
    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      if (null != m_configReader) {
        return m_configReader.Get<T> (key);
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: no config reader is set => {key} not found");
        }
        throw new ConfigKeyNotFoundException (key);
      }
    }
    #endregion // IGenericConfigReader implementation

    #region IPersistentConfigWriter implementation
    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="v"></param>
    /// <param name="overwrite"></param>
    /// <returns></returns>
    public bool SetPersistentConfig<T> (string key, T v, bool overwrite)
    {
      if (null != m_configWriter) {
        return m_configWriter.SetPersistentConfig (key, v, overwrite);
      }
      else {
        log.Error ($"SetPersistentConfig: no persistent config writer is set");
        throw new InvalidOperationException ("No persistent config writer is set");
      }
    }

    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <param name="key"></param>
    public void ResetPersistentConfig (string key)
    {
      if (null != m_configWriter) {
        m_configWriter.ResetPersistentConfig (key);
      }
      else {
        log.Warn ($"ResetPersistentConfig: no persistent config writer is set");
      }
    }
    #endregion // IPersistentConfigWriter implementation
  }
}
