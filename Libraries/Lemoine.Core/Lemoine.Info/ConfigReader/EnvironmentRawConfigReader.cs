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
  /// using the raw key without any transformation
  /// 
  /// Thread safe
  /// </summary>
  public class EnvironmentRawStringConfigReader : IStringConfigReader
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (EnvironmentRawStringConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EnvironmentRawStringConfigReader ()
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
    public string GetString (string key)
    {
      string v = System.Environment.GetEnvironmentVariable (key);
      if (null == v) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetString: key {key} is not in the environment variables");
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
  public class EnvironmentRawConfigReader
    : AutoConvertConfigReader
    , IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (EnvironmentRawConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public EnvironmentRawConfigReader ()
      : base (new EnvironmentRawStringConfigReader ())
    {
    }
    #endregion // Constructors
  }
}
