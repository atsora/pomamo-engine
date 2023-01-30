// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Description of PrefixConfigReader.
  /// 
  /// Thread safe if configReader is thread safe
  /// </summary>
  public sealed class PrefixConfigReader: IGenericConfigReader
  {
    #region Members
    readonly string m_prefix;
    readonly IGenericConfigReader m_configReader;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (PrefixConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="prefix"></param>
    /// <param name="configReader"></param>
    public PrefixConfigReader (string prefix, IGenericConfigReader configReader)
    {
      m_prefix = prefix;
      m_configReader = configReader;
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods

    #region IGenericConfigReader implementation
    /// <summary>
    /// <see cref="IGenericConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T Get<T>(string key)
    {
      return m_configReader.Get<T> (m_prefix + key);
    }
    #endregion
  }
}
