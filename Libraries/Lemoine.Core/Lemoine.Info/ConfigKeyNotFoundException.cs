// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Info
{
  /// <summary>
  /// Exception that is raised when a config key is not found in a specific config reader
  /// </summary>
  public class ConfigKeyNotFoundException : Exception
  {
    readonly ILog log = LogManager.GetLogger<ConfigKeyNotFoundException> ();

    readonly string m_key;

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="key">key (not null)</param>
    public ConfigKeyNotFoundException (string key)
      : base ($"Key {key} not found")
    {
      Debug.Assert (null != key);

      m_key = key;
    }

    /// <summary>
    /// Constructor with an inner exception
    /// </summary>
    /// <param name="key">key (not null)</param>
    /// <param name="inner"></param>
    public ConfigKeyNotFoundException (string key, Exception inner)
      : base ($"Key {key} not found", inner)
    {
      Debug.Assert (null != key);

      m_key = key;
    }

    /// <summary>
    /// Constructor with a message
    /// </summary>
    /// <param name="key">key (not null)</param>
    /// <param name="message"></param>
    public ConfigKeyNotFoundException (string key, string message)
      : base ($"Key {key} not found: {message}")
    {
      Debug.Assert (null != key);

      m_key = key;
    }

    /// <summary>
    /// Constructor with an additional message and an inner exception
    /// </summary>
    /// <param name="key">key (not null)</param>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public ConfigKeyNotFoundException (string key, string message, Exception inner)
      : base ($"Key {key} not found: {message}", inner)
    {
      Debug.Assert (null != key);

      m_key = key;
    }
    #endregion // Constructors

  }
}
