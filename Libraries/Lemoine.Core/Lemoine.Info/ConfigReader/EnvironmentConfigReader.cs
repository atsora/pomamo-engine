// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Config reader that gets the value in the environment variables
  /// after converting the characters in upper case and replacing '.' by '_'
  /// 
  /// Thread safe
  /// </summary>
  public class EnvironmentStringConfigReader: IStringConfigReader
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (EnvironmentStringConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EnvironmentStringConfigReader ()
    {
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IStringConfigReader implementation
    /// <summary>
    /// <see cref="IStringConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString(string key)
    {
      var transformedKey = key.ToUpperInvariant ();
      transformedKey = transformedKey.Replace ('.', '_');
      string v = System.Environment.GetEnvironmentVariable (transformedKey);
      if (null == v) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetString: key {key}, transformed {transformedKey} is not in the environment variables");
        }
        throw new ConfigKeyNotFoundException (key);
      }
      return v;
    }
    #endregion
  }
  
  /// <summary>
  /// Config reader that gets the value in the environment variables
  /// 
  /// Thread safe
  /// </summary>
  public class EnvironmentConfigReader
    : AutoConvertConfigReader
    , IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EnvironmentConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public EnvironmentConfigReader ()
      : base (new EnvironmentStringConfigReader ())
    {
    }
    #endregion // Constructors
  }
}
