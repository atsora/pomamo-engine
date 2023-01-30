// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Extensions.Alert
{
  /// <summary>
  /// List of possible types to match a config e-mail
  /// </summary>
  public enum ConfigEMailInputType
  {
    /// <summary>
    /// List
    /// </summary>
    List = 1,
    Text = 2,
  }

  /// <summary>
  /// Extension to ConfigEMailAction
  /// </summary>
  public interface IConfigEMailExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Associated Alert data type
    /// </summary>
    string DataType { get; }

    /// <summary>
    /// Text that is associated to the alert data type
    /// </summary>
    string DataTypeText { get; }

    /// <summary>
    /// Input type in the configuration interface
    /// </summary>
    ConfigEMailInputType InputType { get; }

    /// <summary>
    /// List of possible values in case InputType=List
    /// </summary>
    IEnumerable<string> InputList { get; }

    /// <summary>
    /// Check it the value from the data matches the configuration input
    /// </summary>
    /// <param name="configInput">Configuration input</param>
    /// <param name="v">Value from the data</param>
    /// <returns></returns>
    bool Match (string configInput, string v);
  }
}
