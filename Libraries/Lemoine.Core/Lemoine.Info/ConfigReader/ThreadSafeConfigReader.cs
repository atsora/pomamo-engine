// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Thread safe layer for a config reader
  /// </summary>
  public sealed class ThreadSafeConfigReader
    : IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MemoryConfigReader).FullName);

    readonly IGenericConfigReader m_configReader;
    readonly object m_lock = new object ();
    
    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor considering the default file .exe.options
    /// </summary>
    /// <param name="configReader"></param>
    public ThreadSafeConfigReader (IGenericConfigReader configReader)
    {
      m_configReader = configReader;
    }
    #endregion // Constructors

    #region IGenericConfigReader implementation
    /// <summary>
    /// <see cref="IGenericConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(string key)
    {
      lock (m_lock)
      {
        return m_configReader.Get<T> (key);
      }
    }
    #endregion
  }
}
