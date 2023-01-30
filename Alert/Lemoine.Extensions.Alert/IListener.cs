// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml;

namespace Lemoine.Extensions.Alert
{
  /// <summary>
  /// Interface for an alert listener
  /// </summary>
  public interface IListener
  {
    /// <summary>
    /// Get in the listener the next data.
    /// 
    /// Returns null when there is no data any more to return
    /// </summary>
    /// <returns>new data or null</returns>
    XmlElement GetData ();
  }
}
