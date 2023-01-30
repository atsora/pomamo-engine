// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

using Lemoine.Core.Log;

namespace Lemoine.I18N.TargetSpecific
{
  /// <summary>
  /// Class to set the locale settings of the current application.
  /// </summary>
  public sealed class LocaleSettings
  {
    private static readonly ILog log = LogManager.GetLogger (typeof (LocaleSettings).FullName);
#if !NETSTANDARD
    private static readonly string REGISTRY_PULSE_KEY = "SOFTWARE\\Lemoine\\PULSE";
    private static readonly string LANGUAGE_KEY = "Language";
#endif // !NETSTANDARD

    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    LocaleSettings ()
    {
    }

    #region Methods
    /// <summary>
    /// Get the language that is set in the registry.
    /// 
    /// The read key is HKLM\\SOFTWARE\\Lemoine\\PULSE\\Language
    /// 
    /// If the key in the registry does not exist or is invalid,
    /// NULL is returned.
    /// 
    /// This works only on windows, else PlatformNotSupportedException is thrown
    /// </summary>
    /// <returns>Language set in registry or null</returns>
    public static string GetLanguageInRegistry ()
    {
#if NETSTANDARD
      log.Fatal ($"Load: .Net Standard compilation is not supported");
      throw new NotSupportedException ("Net Standard");
#else // !NETSTANDARD
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ($"GetLanguageInRegistry: not supported (not windows system)");
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }
#endif // !(NET48 || NETCOREAPP)

      return ReadLanguageInRegistryView (Microsoft.Win32.RegistryView.Registry32) ?? ReadLanguageInRegistryView (Microsoft.Win32.RegistryView.Registry64);
#endif // NETSTANDARD
    }

#if !NETSTANDARD
    static string ReadLanguageInRegistryView (Microsoft.Win32.RegistryView registryView)
    {
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Fatal ($"ReadLanguageInRegistryView: not supported (not windows system)");
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }
#endif // !(NET48 || NETCOREAPP)

      try {
        using (var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey (Microsoft.Win32.RegistryHive.LocalMachine, registryView)) {
          using (var pulseKey = baseKey.OpenSubKey (REGISTRY_PULSE_KEY)) {
            if (pulseKey is null) {
              log.Info ($"ReadLanguageInRegistry: key HKLM/SOFTWARE/Lemoine/PULSE does not exist in view {registryView}, skip this view");
              return null;
            }
            else {
              return (string)pulseKey.GetValue (LANGUAGE_KEY);
            }
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"ReadLanguageInRegistry: could not read the registry in view {registryView}", ex);
        return null;
      }
    }

    static bool DeleteLanguageInRegistryView (Microsoft.Win32.RegistryView registryView)
    {
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Fatal ($"DeleteLanguageInRegistryView: not supported (not windows system)");
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }
#endif // !(NET48 || NETCOREAPP)

      try {
        using (var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey (Microsoft.Win32.RegistryHive.LocalMachine, registryView)) {
          using (var pulseKey = baseKey.OpenSubKey (REGISTRY_PULSE_KEY, true)) {
            if (pulseKey is null) {
              log.Info ($"DeleteLanguageInRegistry: key HKLM/SOFTWARE/Lemoine/PULSE does not exist in view {registryView}, skip this view");
              return false;
            }
            else {
              pulseKey.DeleteValue (LANGUAGE_KEY);
              return true;
            }
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"DeleteLanguageInRegistry: could not read the registry in view {registryView}", ex);
        return false;
      }
    }

    static bool SetLanguageInRegistryView (string language, Microsoft.Win32.RegistryView registryView)
    {
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Fatal ($"SetLanguageInRegistryView: not supported (not windows system)");
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }
#endif // !(NET48 || NETCOREAPP)

      try {
        using (var baseKey = Microsoft.Win32.RegistryKey.OpenBaseKey (Microsoft.Win32.RegistryHive.LocalMachine, registryView)) {
          using (var pulseKey = baseKey.OpenSubKey (REGISTRY_PULSE_KEY, true)) {
            if (pulseKey is null) {
              log.Info ($"DeleteLanguageInRegistry: key HKLM/SOFTWARE/Lemoine/PULSE does not exist in view {registryView}, skip this view");
              return false;
            }
            else {
              pulseKey.SetValue (LANGUAGE_KEY, language);
              return true;
            }
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"DeleteLanguageInRegistry: could not read the registry in view {registryView}", ex);
        return false;
      }
    }
#endif // !NETSTANDARD

    /// <summary>
    /// Reset the language key in registry to use the default language
    /// </summary>
    /// <returns>Success</returns>
    public static bool ResetLanguageInRegistry ()
    {
#if NETSTANDARD
      log.Fatal ($"Load: .Net Standard compilation is not supported");
      throw new NotSupportedException ("Net Standard");
#else // !NETSTANDARD
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ($"GetLanguageInRegistry: not supported (not windows system)");
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }
#endif // NET48 || NETCOREAPP

      var result64 = DeleteLanguageInRegistryView (Microsoft.Win32.RegistryView.Registry64);
      var result32 = DeleteLanguageInRegistryView (Microsoft.Win32.RegistryView.Registry32);
      return result64 || result32;
#endif // NETSTANDARD
    }

    /// <summary>
    /// Set the language in registry.
    /// </summary>
    /// <param name="language">Language code to set in registry</param>
    /// <returns>Success</returns>
    public static bool SetLanguageInRegistry (string language)
    {
#if NETSTANDARD
      log.Fatal ($"Load: .Net Standard compilation is not supported");
      throw new NotSupportedException ("Net Standard");
#else // !NETSTANDARD
#if NET48 || NETCOREAPP
      if (!RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
        log.Error ($"GetLanguageInRegistry: not supported (not windows system)");
        throw new PlatformNotSupportedException ("Registry is not supported on this platform");
      }
#endif // NET48 || NETCOREAPP

      var result64 = SetLanguageInRegistryView (language, Microsoft.Win32.RegistryView.Registry64);
      var result32 = SetLanguageInRegistryView (language, Microsoft.Win32.RegistryView.Registry32);
      return result64 || result32;
#endif // NETSTANDARD
    }
    #endregion
  }
}
