// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;

namespace Lemoine.Extensions.Alert
{
  /// <summary>
  /// Alert action
  /// 
  /// This is the interface for any kind of alert action (E-Mail, SMS, ...)
  /// </summary>
  public interface IAction
  {
    /// <summary>
    /// Execute the action from the specified data
    /// </summary>
    /// <param name="data"></param>
    void Execute (XmlElement data);
  }
}
