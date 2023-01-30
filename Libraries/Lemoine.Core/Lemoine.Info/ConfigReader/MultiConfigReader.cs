// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Multi config reader
  /// 
  /// Thread safe
  /// </summary>
  public sealed class MultiConfigReader : IGenericConfigReader
  {
    #region Members
#if (NETSTANDARD2_1 || NETCOREAPP2_0)
    readonly ConcurrentQueue<IGenericConfigReader> m_configReaders = new ConcurrentQueue<IGenericConfigReader> ();
#else
    ConcurrentQueue<IGenericConfigReader> m_configReaders = new ConcurrentQueue<IGenericConfigReader> ();
    readonly SemaphoreSlim m_configReadersSemaphore = new SemaphoreSlim (1, 1);
#endif
    #endregion // Members

    readonly ILog log = LogManager.GetLogger (typeof (MultiConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public MultiConfigReader ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Add a config reader
    /// </summary>
    /// <param name="configReader"></param>
    public void Add (IGenericConfigReader configReader)
    {
      m_configReaders.Enqueue (configReader);
    }

    /// <summary>
    /// Clear all the config readers
    /// </summary>
    public void Clear ()
    {
#if (NETSTANDARD2_1 || NETCOREAPP2_0)
      m_configReaders.Clear ();
#else
      using (var semaphoreSlimHolder = SemaphoreSlimHolder.Create (m_configReadersSemaphore)) {
        m_configReaders = new ConcurrentQueue<IGenericConfigReader> ();
      }
#endif // NETSTANDARD2_1 || NETCOREAPP2_0
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
      foreach (var configReader in m_configReaders) {
        try {
          return configReader.Get<T> (key);
        }
        catch (Exception ex) {
          if (log.IsDebugEnabled) {
            log.Debug ($"Get: config reader {configReader} returned an exception for key {key}=> try the next config reader", ex);
          }
        }
      }

      if (log.IsDebugEnabled) {
        log.Debug ($"Get: key {key} not found");
      }
      throw new ConfigKeyNotFoundException (key);
    }

    #endregion
  }
}
