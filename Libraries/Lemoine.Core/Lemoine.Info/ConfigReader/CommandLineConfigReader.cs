// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// String Config reader from command line parameters
  /// 
  /// Thread safe
  /// </summary>
  public sealed class CommandLineStringConfigReader
    : IStringConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CommandLineStringConfigReader).FullName);

    readonly IDictionary<string, string> m_dictionary = new ConcurrentDictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CommandLineStringConfigReader ()
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Add parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="parameterSeperator"></param>
    /// <param name="keyValueSeparator"></param>
    public void AddParameters (string parameters, char parameterSeperator = ';', char keyValueSeparator = '=')
    {
      AddParameters (parameters.Split (parameterSeperator), keyValueSeparator);
    }

    /// <summary>
    /// Add parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="separator"></param>
    public void AddParameters (IEnumerable<string> parameters, char separator = '=')
    {
      foreach (var parameter in parameters) {
        var keyValue = parameter.Split (new char[] { separator }, 2);
        if (2 != keyValue.Length) {
          log.Error ($"AddParameters: invalid parameter {parameter}, probably no separator {separator} in it");
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"AddParameters: add {keyValue[0]} = {keyValue[1]}");
          }
          m_dictionary[keyValue[0]] = keyValue[1];
        }
      }
    }

    #region IStringConfigReader implementation
    /// <summary>
    /// <see cref="IStringConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString (string key)
    {
      string v;
      if (m_dictionary.TryGetValue (key, out v)) {
        return v;
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ($"Get: key {key} not found");
        }
        throw new ConfigKeyNotFoundException (key);
      }
    }
    #endregion
  }

  /// <summary>
  /// Config reader from command line parameters
  /// 
  /// Thread safe
  /// </summary>
  public sealed class CommandLineConfigReader
    : AutoConvertConfigReader<CommandLineStringConfigReader>
    , IGenericConfigReader
  {
    static readonly ILog log = LogManager.GetLogger (typeof (CommandLineConfigReader).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public CommandLineConfigReader ()
      : base (new CommandLineStringConfigReader ())
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Add parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="parameterSeperator"></param>
    /// <param name="keyValueSeparator"></param>
    public void AddParameters (string parameters, char parameterSeperator = ';', char keyValueSeparator = '=')
    {
      this.StringConfigReader.AddParameters (parameters, parameterSeperator, keyValueSeparator);
    }

    /// <summary>
    /// Add parameters
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="separator"></param>
    public void AddParameters (IEnumerable<string> parameters, char separator = '=')
    {
      this.StringConfigReader.AddParameters (parameters, separator);
    }
  }
}
