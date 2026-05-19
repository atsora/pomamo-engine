// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Business.Operation;
using Lemoine.Extensions.Business.Group;
using Lemoine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulse.Extensions.Web
{
  /// <summary>
  /// Extension to the Signal web services
  /// </summary>
  public interface ISignalExtension
    : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension. Return true if the extension is active, else false
    /// </summary>
    /// <returns>the extension is active</returns>
    bool Initialize ();

    /// <summary>
    /// Get signals that are associated to the specific group of machines and a specific role
    /// </summary>
    /// <param name="group"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    IList<Signal> GetSignals (IGroup group, IRole role);
  }
}
