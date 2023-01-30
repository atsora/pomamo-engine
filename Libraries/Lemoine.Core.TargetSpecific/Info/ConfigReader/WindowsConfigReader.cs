// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader.TargetSpecific
{
  /// <summary>
  /// StringConfigReader that takes values from .exe.config
  /// 
  /// Thread safe
  /// </summary>
  public class WindowsStringConfigReader : IStringConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WindowsStringConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public WindowsStringConfigReader ()
    {
    }
    #endregion // Constructors

    #region IStringConfigReader implementation
    /// <summary>
    /// <see cref="IStringConfigReader"/>
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString (string key)
    {
#if NETSTANDARD
      log.Fatal ($"GetString: .Net Standard compilation is not supported");
      throw new ConfigKeyNotFoundException (key, "Not supported compilation", new NotSupportedException ("Net Standard"));
#else // !NETSTANDARD
      try {
        string result = System.Configuration.ConfigurationManager.AppSettings[key];
        if (null == result) {
          throw new ConfigKeyNotFoundException (key);
        }
        return result;
      }
      catch (PlatformNotSupportedException ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetString: platform is not supported");
        }
        throw new ConfigKeyNotFoundException (key, "Not supported platform", ex);
      }
#endif // NETSTANDARD
    }
    #endregion
  }

  /// <summary>
  /// Config reader for a .exe.config file
  /// 
  /// Thread safe
  /// </summary>
  public class WindowsConfigReader
    : AutoConvertConfigReader
    , IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WindowsConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public WindowsConfigReader ()
      : base (new WindowsStringConfigReader ())
    {
    }
    #endregion // Constructors
  }
}
