// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Interface for the plugins to update the default reason
  /// </summary>
  public interface IReasonLegendExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Get all the reasons that may be set on a machine
    /// with an initialized reason group
    /// </summary>
    /// <returns></returns>
    IEnumerable<IReason> GetUsedReasons ();
  }
}
