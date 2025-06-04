// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;

using Lemoine.Core.Log;
using Lemoine.Info;

namespace Lemoine.I18N
{
  /// <summary>
  /// Class to set the locale settings of the current application.
  /// </summary>
  public sealed class LocaleSettings
  {
    private static readonly string LANGUAGE_KEY = "Language";

    bool m_manuallySet;

    private static readonly ILog log = LogManager.GetLogger (typeof (LocaleSettings).FullName);

    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    LocaleSettings ()
    {
      m_manuallySet = false;
    }

    /// <summary>
    /// Current culture
    /// </summary>
    public static CultureInfo CurrentCulture
    {
      get { return Thread.CurrentThread.CurrentUICulture; }
    }

    /// <summary>
    /// Set the language of the current thread.
    /// </summary>
    /// <param name="cultureInfo">Culture to set to the current thread</param>
    public static void SetLanguage (CultureInfo cultureInfo)
    {
      Thread.CurrentThread.CurrentCulture = cultureInfo;
      Thread.CurrentThread.CurrentUICulture = cultureInfo;
      Instance.m_manuallySet = true;
    }

    /// <summary>
    /// Set the language of the current thread.
    /// 
    /// If culture is an invalid culture name, an exception can be raised.
    /// </summary>
    /// <param name="culture">Name of the culture</param>
    public static void SetLanguage (string culture)
    {
      SetLanguage (new CultureInfo (culture));
    }

    /// <summary>
    /// Set the language of the current thread from a configuration key
    /// if a language has not already been set manually.
    /// 
    /// The config key is Language.
    /// 
    /// If the key does not exist in the configuration or if it is invalid,
    /// nothing is done and false is returned.
    /// 
    /// Note that the Administrator privilege may be required to get some configuration vlaues.
    /// </summary>
    /// <returns>true if the language was set from the config set</returns>
    public static bool SetLanguageFromConfigSetIfNotManuallySet ()
    {
      if (Instance.m_manuallySet) {
        return false;
      }
      else {
        return SetLanguageFromConfigSet ();
      }
    }

    /// <summary>
    /// Set the language of the current thread from a configuration key
    /// 
    /// The config key is Language.
    /// 
    /// If the key does not exist in the configuration or if it is invalid,
    /// nothing is done and false is returned.
    /// 
    /// Note that the Administrator privilege may be required to get some configuration vlaues.
    /// </summary>
    /// <returns>true if the language was set from the config set</returns>
    public static bool SetLanguageFromConfigSet ()
    {
      string language;
      try {
        language = Lemoine.Info.ConfigSet.Get<string> (LANGUAGE_KEY);
      }
      catch (ConfigKeyNotFoundException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"SetLanguageFromConfigSet: key {LANGUAGE_KEY} not set", ex);
        }
        return false;
      }
      catch (KeyNotFoundException ex) {
        log.Fatal ($"SetLanguageFromConfigSet: (with deprecated KeyNotFoundException) key {LANGUAGE_KEY} not set", ex);
        return false;
      }
      catch (Exception ex) {
        log.Error ("SetLanguageFromConfigSet: exception", ex);
        throw;
      }
      if (language == null) {
        return false;
      }
      else {
        SetLanguage (language);
        log.InfoFormat ("The following language <{0}> was successfully set from the config set",
                        language);
        return true;
      }
    }

    #region Instance
    static LocaleSettings Instance
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

      internal static readonly LocaleSettings instance = new LocaleSettings ();
    }
    #endregion
  }
}
