// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Lemoine.Core.Log;

namespace Lemoine.Info.TargetSpecific
{
  /// <summary>
  /// Get the versions in registry
  /// </summary>
  public sealed class PulseVersions
  {
    IDictionary<string, string> m_versions = new Dictionary<string, string> (); // map[stringName] = stringValue

    bool m_valid = false;

    static readonly ILog log = LogManager.GetLogger (typeof (PulseVersions).FullName);

    #region Getters / Setters
    /// <summary>
    /// Get the versions of the different software
    /// </summary>
    public static IDictionary<string, string> Versions
    {
      get { return Instance.m_versions; }
    }

    /// <summary>
    /// Is it valid ?
    /// 
    /// In case it is not, the registry version
    /// may be empty
    /// </summary>
    public static bool Valid
    {
      get { return Instance.m_valid; }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private PulseVersions ()
    {
      try {
        Load ();
      }
      catch (Exception ex) {
        log.Error ("PulseVersions: Could not get the info in registry", ex);
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Reload the Pulse info data
    /// </summary>
    public static void Reload ()
    {
      try {
        Instance.Load ();
      }
      catch (Exception ex) {
        log.Error ("Reload: Load failed", ex);
      }
    }

    void Load ()
    {
      try {
#if NETSTANDARD
        log.Fatal ($"Load: .Net Standard compilation is not fully supported");
        throw new NotSupportedException ("Net Standard");
#else // !NETSTANDARD
#if NET48 || NETCOREAPP
        if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
          m_versions.Clear ();
        }
        else {
#else // !(NET48 || NETCOREAPP)
        {
#endif // !(NET48 || NETCOREAPP)
          this.m_versions.Clear ();

          ReadRegistry (Microsoft.Win32.RegistryView.Registry64);
          ReadRegistry (Microsoft.Win32.RegistryView.Registry32);
        }
#endif // NETSTANDARD

        // XxxVersion files in VersionsDirectory
        var versionsDirectory = Lemoine.Info.ConfigSet
          .LoadAndGet ("VersionsDirectory", ""); // Only defined on Linux
        if (!string.IsNullOrEmpty (versionsDirectory) && Directory.Exists (versionsDirectory)) {
          var versionFiles = Directory.GetFiles (versionsDirectory, "*Version");
          foreach (var versionFile in versionFiles) {
            var versionFilePath = Path.Combine (versionsDirectory, versionFile);
            string firstLine;
            using (var reader = new StreamReader (versionFilePath)) {
              firstLine = reader.ReadLine ();
            }
            var fileName = Path.GetFileName (versionFilePath);
            if (log.IsDebugEnabled) {
              log.Debug ($"Load: add version {fileName}={firstLine} from file {versionFilePath}");
            }
            this.m_versions.Add (fileName, firstLine);
          }
        }

        this.m_valid = true;
      }
      catch (Exception ex) {
        this.m_valid = false;
        log.Error ("Load: exception", ex);
        throw;
      }
    }

#if !NETSTANDARD
    void ReadRegistry (Microsoft.Win32.RegistryView registryView)
    {
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Fatal ($"ReadRegistry: not supported (not windows system)");
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }
#endif // !(NET48 || NETCOREAPP)

      try {
        using (var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey (Microsoft.Win32.RegistryHive.LocalMachine, registryView)) {
          var registryKey =
#if ATSORA
            @"SOFTWARE\Atsora\Tracking"
#elif LEMOINE
            @"SOFTWARE\Lemoine\PULSE"
#else
            @"SOFTWARE\Pomamo"
#endif
          ;
          using (var pulseKey = baseKey.OpenSubKey (registryKey)) {
            if (null == pulseKey) {
              log.Info ($"ReadRegistry: key HKLM/{registryKey} does not exist in view {registryView}, skip this view");
            }
            else {
              string[] names = pulseKey.GetValueNames ();
              foreach (string name in names
                .Where (x => x.EndsWith ("Version", StringComparison.InvariantCultureIgnoreCase) && !m_versions.Keys.Contains (x))) {
                var v = (string)pulseKey.GetValue (name);
                this.m_versions.Add (name, v);
              }
            }
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"ReadRegistry: could not read the registry in view {registryView}", ex);
      }
    }
#endif // !NETSTANDARD

    #endregion // Methods

    #region Instance
    static PulseVersions Instance
    {
      get { return Nested.instance; }
    }

    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested ()
      {
      }

      internal static readonly PulseVersions instance = new PulseVersions ();
    }
    #endregion
  }
}
