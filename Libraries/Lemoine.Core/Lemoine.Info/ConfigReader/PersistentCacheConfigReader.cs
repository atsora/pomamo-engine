// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Config reader using a persistent cache in case the data is not available
  /// 
  /// Thread safe if the associated config reader is thread safe
  /// </summary>
  public sealed class PersistentCacheConfigReader
    : IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (PersistentCacheConfigReader).FullName);

    readonly IGenericConfigReader m_configReader;
    readonly string m_path;

    CachedConfigReader m_persistentConfigReader = null;
    OptionsFileConfigReader m_fileConfigReader = null;
    readonly object m_persistentConfigReaderLock = new object ();

    /// <summary>
    /// Constructor
    /// </summary>
    public PersistentCacheConfigReader (IGenericConfigReader configReader, string fileName)
    {
      m_configReader = configReader;
      m_path = Path.Combine (PulseInfo.LocalConfigurationDirectory, fileName);
    }

    /// <summary>
    /// <see cref="IGenericConfigReader" />
    ///
    /// The main config reader is called outside the lock,
    /// because it may take other locks and read the configuration again (risk of dead lock).
    /// Only the accesses to the persistent cache file are protected by the lock.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
      if (log.IsTraceEnabled) {
        log.Trace ($"Get: key {key}");
      }

      T v;
      try {
        v = m_configReader.Get<T> (key);
      }
      catch (ConfigKeyNotFoundException) {
        lock (m_persistentConfigReaderLock) {
          try {
            LoadPersistentConfigReader ();
            CheckNotFound<T> (key);
          }
          catch (Exception ex) {
            log.Error ($"Get: error in the persistent cache for the not found key {key}", ex);
          }
        }
        throw;
      }
      catch (Exception ex) {
        log.Error ($"Get: try to read it from persistent cache instead for {key}", ex);
        lock (m_persistentConfigReaderLock) {
          if (!File.Exists (m_path)) {
            log.Error ($"Get: {m_path} does not exist for {key}, throw the exception", ex);
            throw;
          }
          try {
            LoadPersistentConfigReader ();
            return m_persistentConfigReader.Get<T> (key);
          }
          catch (ConfigKeyNotFoundException) {
            throw;
          }
          catch (Exception ex1) {
            log.Error ($"Get: error in persistent config reader for {key}", ex1);
            throw;
          }
        }
      }

      lock (m_persistentConfigReaderLock) {
        try {
          LoadPersistentConfigReader ();
          CheckCacheValue (key, v);
        }
        catch (Exception ex) {
          log.Error ($"Get: error when updating the persistent cache for {key}, but return the value {v}", ex);
        }
      }
      return v;
    }

    void LoadPersistentConfigReader ()
    {
      lock (m_persistentConfigReaderLock) {
        if (null == m_persistentConfigReader) {
          if (!File.Exists (m_path)) {
            if (log.IsDebugEnabled) {
              log.Debug ($"LoadPersistentConfigReader: create an empty file {m_path}");
            }
            File.WriteAllText (m_path, "");
          }

          m_fileConfigReader = OptionsFileConfigReader.CreateFromPath (m_path);
          m_persistentConfigReader = new CachedConfigReader (m_fileConfigReader);
        }
      }
    }

    void CheckCacheValue<T> (string key, T v)
    {
      try {
        var cacheValue = m_persistentConfigReader.Get<T> (key);
        if (!object.Equals (cacheValue, v)) {
          InvalidateCacheFile ();
          Add (key, v);
        }
      }
      catch (ConfigKeyNotFoundException) {
        Add (key, v);
      }
      catch (Exception ex) {
        log.Error ($"CheckCacheValue: exception => invalidate", ex);
        InvalidateCacheFile ();
        Add (key, v);
      }
    }

    void CheckNotFound<T> (string key)
    {
      try {
        m_persistentConfigReader.Get<T> (key);
      }
      catch (ConfigKeyNotFoundException) {
        return;
      }
      catch (Exception ex) {
        log.Error ($"CheckNotFound: exception => invalidate", ex);
        InvalidateCacheFile ();
        return;
      }
      InvalidateCacheFile ();
    }

    bool Add<T> (string key, T v)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"Add: {key}={v}");
      }
      m_fileConfigReader.SetPersistentConfig (key, v, false);
      m_persistentConfigReader.Add (key, v, false);
      return true;
    }

    /// <summary>
    /// Invalidate the persistent cache file
    /// </summary>
    public void InvalidateCacheFile ()
    {
      if (null != m_persistentConfigReader) {
        lock (m_persistentConfigReaderLock) {
          m_persistentConfigReader.Clear ();
          m_persistentConfigReader = null;

          try {
            File.Delete (m_path);
          }
          catch (Exception ex) {
            log.Error ($"InvalidateCacheFile: error in deleting the file {m_path}", ex);
            throw;
          }

          File.WriteAllText (m_path, "");
          m_fileConfigReader = OptionsFileConfigReader.CreateFromPath (m_path);
          m_persistentConfigReader = new CachedConfigReader (m_fileConfigReader);
        }
      }
    }
  }
}
