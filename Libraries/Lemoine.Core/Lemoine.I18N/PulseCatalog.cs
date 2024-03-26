// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

using Lemoine.Core.Log;

namespace Lemoine.I18N
{
  /// <summary>
  /// Class to easily translate the applications.
  /// 
  /// This class is a singleton class.
  /// </summary>
  public sealed class PulseCatalog
  {
    #region Members
    ICatalog m_implementation = null;
    CultureInfo m_cultureInfo;
    #endregion

    private static readonly ILog log = LogManager.GetLogger(typeof (PulseCatalog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Implementation
    /// </summary>
    public static ICatalog Implementation {
      get { return Instance.m_implementation; }
      set { Instance.m_implementation = value; }
    }
    
    /// <summary>
    /// Associated culture.
    /// If this property is null, the culture is taken from CurrentUICulture.
    /// </summary>
    public static CultureInfo CultureInfo {
      get { return Instance.m_cultureInfo; }
      set { Instance.m_cultureInfo = value; }
    }
    #endregion
    
    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    PulseCatalog()
    {
      LocaleSettings.SetLanguageFromConfigSetIfNotManuallySet ();
    }
    #endregion
    
    #region Methods
    /// <summary>
    /// Returns a native language translation of the text passed as the parameter.
    /// </summary>
    /// <param name="s">A string containing the text to be translated. It is used as the key to search the catalog on. Not null or empty</param>
    /// <returns>The translated string, if found in the catalog, s otherwise.</returns>
    public static string GetString (string s)
    {
      Debug.Assert (!string.IsNullOrEmpty (s));

      if (null == Instance.m_implementation) {
        log.Warn ($"GetString: no implementation set => return key {s}");
        return s;
      }
      else {
        var cultureInfo = Instance.m_cultureInfo ?? CultureInfo.CurrentUICulture;
        Debug.Assert (null != cultureInfo);

        try {
          string translation = Instance.m_implementation.GetString (s, cultureInfo);
          if (null == translation) {
            if (log.IsDebugEnabled) {
              log.Debug ($"GetString: no translation found for {s}, return the key");
            }
            return s;
          }
          else {
            return translation;
          }
        }
        catch (Exception ex) {
          log.Error ($"GetString: return the key {s} because i18n implementation returned an exception", ex);
          return s;
        }
      }
    }

    /// <summary>
    /// Returns a native language translation of the text passed as the parameter.
    /// </summary>
    /// <param name="s">A string containing the text to be translated. It is used as the key to search the catalog on. (not null or empty)</param>
    /// <param name="d">Default translation value (not null)</param>
    /// <returns>The translated string, if found in the catalog, d otherwise.</returns>
    public static string GetString (string s, string d)
    {
      Debug.Assert (!string.IsNullOrEmpty (s));
      Debug.Assert (null != d);

      if (null == Instance.m_implementation) {
        log.Warn ($"GetString: no implementation set => return {d} for key {s}");
        return d;
      }
      else {
        var cultureInfo = Instance.m_cultureInfo ?? CultureInfo.CurrentUICulture;
        Debug.Assert (null != cultureInfo);

        try {
          var translation = Instance.m_implementation.GetString (s, cultureInfo);
          if (null == translation) {
            log.DebugFormat ("GetString: " +
                             "no translation found for {0}, return the default value {1}",
                             s, d);
            return d;
          }
          else {
            return translation;
          }
        }
        catch (Exception ex) {
          log.Error ($"GetString: return the default {d} for key {s} because i18n implementation returned an exception", ex);
          return d;
        }
      }
    }
    #endregion

    #region Instance
    static PulseCatalog Instance
    {
      get { return Nested.instance; }
    }
    
    class Nested
    {
      // Explicit static constructor to tell C# compiler
      // not to mark type as beforefieldinit
      static Nested()
      {
      }

      internal static readonly PulseCatalog instance = new PulseCatalog ();
    }
    #endregion
  }
}