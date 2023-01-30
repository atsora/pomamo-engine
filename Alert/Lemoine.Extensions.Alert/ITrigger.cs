// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Xml;

namespace Lemoine.Extensions.Alert
{
  /// <summary>
  /// Alert trigger
  /// </summary>
  public interface ITrigger
  {
    /// <summary>
    /// From a specified data, check if an action must be triggered
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    bool Eval (XmlElement data);
  }
}
