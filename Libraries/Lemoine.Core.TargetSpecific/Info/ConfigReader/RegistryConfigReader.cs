// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader;
using Microsoft.Win32;

namespace Lemoine.Info.ConfigReader.TargetSpecific
{
  /// <summary>
  /// GenericConfigReader using the key values in the registry
  /// 
  /// Thread safe
  /// </summary>
  public class RegistryConfigReader : IGenericConfigReader, IPersistentConfigWriter
  {
    #region Members
    readonly bool m_lazy = false;
    readonly bool m_supportedPlatform = false;
    readonly RegistryKey m_root;
    readonly string m_key;
    readonly IDictionary<string, object> m_keyValue = new ConcurrentDictionary<string, object> ();
    readonly IDictionary<string, byte> m_notFoundKeys = new ConcurrentDictionary<string, byte> (); // Because there is no concurrent set
    #endregion // Members

    static readonly string REGISTRY_KEY =
#if ATSORA
      "SOFTWARE\\Atsora\\Tracking";
#elif LEMOINE
      "SOFTWARE\\Lemoine\\PULSE";
#else
      "SOFTWARE\\Pomamo";
#endif

    static readonly ILog log = LogManager.GetLogger (typeof (RegistryConfigReader).FullName);

    #region Constructors
    /// <summary>
    /// Constructor considering the default key REGISTRY_KEY
    /// </summary>
    public RegistryConfigReader (bool lazy = false)
      : this (REGISTRY_KEY, lazy)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key"></param>
    /// <param name="lazy"></param>
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    RegistryConfigReader (string key, bool lazy = false)
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    {
#if NETSTANDARD
      log.Fatal ($"GetString: .Net Standard compilation is not supported");
      m_supportedPlatform = false;
      m_root = null;
      m_key = null;
      throw new ConfigKeyNotFoundException (key, "Not supported compilation", new NotSupportedException ("Net Standard"));
#else // !NETSTANDARD
      m_supportedPlatform =
#if NET48 || NETCOREAPP
        RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
#else // !(NET48 || NETCOREAPP)
        true;
#endif // !(NET48 || NETCOREAPP)
      if (m_supportedPlatform
#if NET5_0_OR_GREATER
        && OperatingSystem.IsWindows ()
#endif // NET5_0_OR_GREATER
        ) {
        m_lazy = lazy;
        if (null != Registry.LocalMachine) {
          m_root = Registry.LocalMachine;
        }
        else { // null == Registry.LocalMachine
          Debug.Assert (false);
          log.Fatal ("RegistryConfigReader: root is null, it must be compiled for .NET core or .NET Framework");
          throw new NotSupportedException ();
        }
        m_key = key;
        if (!m_lazy) {
          Load ();
        }
      }
#endif // NETSTANDARD
    }
    #endregion // Constructors

#if NET5_0_OR_GREATER
    [SupportedOSPlatform ("windows")]
#endif // NET5_0_OR_GREATER
    void Load ()
    {
      try {
        using (RegistryKey odbcKey = m_root.OpenSubKey (m_key)) {
          if (null == odbcKey) {
            log.Error ($"Load: key {m_key} does not exist");
          }
          else { // null != odbcKey
            string[] names = odbcKey.GetValueNames ();
            foreach (string name in names) {
              var v = odbcKey.GetValue (name);
              if (v is null) {
                m_notFoundKeys[name] = new byte ();
              }
              else {
                m_keyValue[name] = v;
              }
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"Load: Could not get the info in registry in key {m_key}", ex);
      }
    }

#if NET5_0_OR_GREATER
    [SupportedOSPlatform ("windows")]
#endif // NET5_0_OR_GREATER
    object ReadKey (string key)
    {
      try {
        using (RegistryKey odbcKey = m_root.OpenSubKey (m_key)) {
          if (odbcKey is null) {
            log.Warn ($"RegistryConfigReader: key {m_key} does not exist");
            m_notFoundKeys[key] = new byte ();
            throw new ConfigKeyNotFoundException (key);
          }
          var v = odbcKey.GetValue (key);
          if (v is null) {
            m_notFoundKeys[key] = new byte ();
            throw new ConfigKeyNotFoundException (key);
          }
          else {
            m_keyValue[key] = v;
            return v;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"ReadKey: Could not get the key {key} in registry", ex);
        m_notFoundKeys[key] = new byte ();
        throw;
      }
    }

#if NET5_0_OR_GREATER
    [SupportedOSPlatform ("windows")]
#endif // NET5_0_OR_GREATER
    bool WriteKey (string name, object v, bool overwrite)
    {
      try {
        using (RegistryKey odbcKey = m_root.OpenSubKey (m_key, true)) {
          if (!overwrite) {
            var existingValue = odbcKey.GetValue (name);
            if (null != existingValue) {
              if (log.IsDebugEnabled) {
                log.Debug ($"WriteKey: {name}={existingValue} already in registry and overwrite={overwrite} => return false");
              }
              return false;
            }
          }
          odbcKey.SetValue (name, v);
          m_keyValue[name] = v;
          m_notFoundKeys.Remove (name);
          return true;
        }
      }
      catch (System.Security.SecurityException ex) {
        log.Error ($"WriteKey: administrator rights is required to write {name}={v} in registry", ex);
        throw;
      }
      catch (Exception ex) {
        log.Error ($"WriteKey: Could not write {name}={v} in registry", ex);
        throw;
      }
    }

#if NET5_0_OR_GREATER
    [SupportedOSPlatform ("windows")]
#endif // NET5_0_OR_GREATER
    void RemoveKey (string name)
    {
      try {
        using (RegistryKey odbcKey = m_root.OpenSubKey (m_key, true)) {
          odbcKey.DeleteValue (name);
          m_keyValue.Remove (name);
          m_notFoundKeys[name] = new byte ();
        }
      }
      catch (Exception ex) {
        log.Error ($"RemoveKey: Could not remove {name} in registry", ex);
        throw;
      }
    }

    #region IGenericConfigReader implementation
#if NET5_0_OR_GREATER
    [SupportedOSPlatform ("windows")]
#endif // NET5_0_OR_GREATER
    object Get (string key)
    {
      if (m_lazy && !m_keyValue.ContainsKey (key)) {
        if (m_notFoundKeys.ContainsKey (key)) {
          throw new ConfigKeyNotFoundException (key);
        }
        else {
          return ReadKey (key);
        }
      }

      try {
        return m_keyValue[key];
      }
      catch (KeyNotFoundException ex) {
        throw new ConfigKeyNotFoundException (key, ex);
      }
    }

    /// <summary>
    /// <see cref="IGenericConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T> (string key)
    {
#if NETSTANDARD
        log.Fatal ($"GetString: .Net Standard compilation is not supported");
        throw new ConfigKeyNotFoundException (key, "Not supported compilation", new PlatformNotSupportedException ("Net Standard"));
#else // !NETSTANDARD
      if (!m_supportedPlatform
#if NET5_0_OR_GREATER
        || !OperatingSystem.IsWindows ()
#endif // NET5_0_OR_GREATER
        ) {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: not supported platform, return KeyNotFound for key {key}");
        }
        throw new ConfigKeyNotFoundException (key, "Not supported platform", new PlatformNotSupportedException ("Registry is not supported on this platform"));
      }

      return GetOnWindows<T> (key);
#endif // NETSTANDARD
    }

#if NET5_0_OR_GREATER
    [SupportedOSPlatform ("windows")]
#endif // NET5_0_OR_GREATER
    T GetOnWindows<T> (string key)
    {
      object o = Get (key);
      if (o is null) {
        log.Fatal ($"GetOnWindows: value for key {key} was null, which is unexpected");
        throw new ConfigKeyNotFoundException (key);
      }
      else if (o is T) {
        return (T)o;
      }
      else {
        return AutoConvertConfigReader.Convert<T> (o.ToString ());
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
    public bool SetPersistentConfig<T> (string key, T v, bool overwrite)
    {
#if NETSTANDARD
        log.Fatal ($"SetPersistentConfig: .Net Standard compilation is not supported");
        throw new InvalidProgramException ("Not supported compilation", new PlatformNotSupportedException ("Net Standard"));
#else // !NETSTANDARD
      if (!m_supportedPlatform
#if NET5_0_OR_GREATER
        || !OperatingSystem.IsWindows ()
#endif // NET5_0_OR_GREATER
        ) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SetPersistentConfig: not supported platform, return an exception for {key}={v}");
        }
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }

      return WriteKey (key, v.ToString (), overwrite);
#endif // NETSTANDARD
    }

    /// <summary>
    /// <see cref="IPersistentConfigWriter"/>
    /// </summary>
    /// <param name="key"></param>
    public void ResetPersistentConfig (string key)
    {
#if NETSTANDARD
        log.Fatal ($"ResetPersistentConfig: .Net Standard compilation is not supported");
        throw new InvalidProgramException ("Not supported compilation", new PlatformNotSupportedException ("Net Standard"));
#else // !NETSTANDARD
      if (!m_supportedPlatform
#if NET5_0_OR_GREATER
        || !OperatingSystem.IsWindows ()
#endif // NET5_0_OR_GREATER
        ) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ResetPersistentConfig: not supported platform, return an exception for key={key}");
        }
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }

      RemoveKey (key);
#endif // NETSTANDARD
    }
    #endregion // IPersistentConfigWriter implementation

  }
}
