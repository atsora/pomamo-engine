// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

using Lemoine.Core.Log;
using Lemoine.Info.ConfigReader;

namespace Lemoine.Info
{
  /// <summary>
  /// Get some options from an .exe.options file
  /// </summary>
  public sealed class OptionsFile
  {
    #region Members
    IGenericConfigReader m_configReader;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (OptionsFile).FullName);

    #region Constructors
    /// <summary>
    /// Private constructor (singleton class !)
    /// </summary>
    private OptionsFile()
    {
      MultiConfigReader multiConfigReader = new MultiConfigReader ();
      multiConfigReader.Add (OptionsFileConfigReader.CreateFromExtension ());
      multiConfigReader.Add (new WindowsConfigReader ());
      m_configReader = new CachedConfigReader (multiConfigReader);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get an option value
    /// 
    /// If not found, null is returned
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetOption (string key)
    {
      try {
        return Instance.m_configReader.Get<string> (key);
      }
      catch (ConfigKeyNotFoundException ex) {
        log.Warn ($"GetOption: key {key} was not found => return null", ex);
        return null;
      }
      catch (KeyNotFoundException ex) {
        log.Fatal ($"GetOption: catch deprecated KeyNotFoundException for key {key} => return null", ex);
        return null;
      }
      catch (Exception ex) {
        log.Exception (ex, $"GetOption");
        throw;
      }
    }
    #endregion // Methods
    
    #region Instance
    static OptionsFile Instance
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

      internal static readonly OptionsFile instance = new OptionsFile ();
    }
    #endregion // Instance
  }
}
