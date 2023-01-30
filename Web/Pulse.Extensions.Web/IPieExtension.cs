// Copyright (C) 2009-2023 Lemoine Automation Technologies
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
  /// Extension to the Pie web service
  /// </summary>
  public interface IPieExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension. Return true if the extension is active, else false
    /// </summary>
    /// <param name="group"></param>
    /// <returns>the extension is active</returns>
    bool Initialize (IGroup group);

    /// <summary>
    /// Priority.
    /// 
    /// Extension implementations with the highest priority are considered first
    /// </summary>
    double Score { get; }

    /// <summary>
    /// Pie type.
    /// 
    /// If empty, do not display any pie
    /// </summary>
    string PieType { get; }

    /// <summary>
    /// The pie is permanent, the pie type won't never change.
    /// 
    /// Else the pie type may be updated from time to time
    /// </summary>
    bool Permanent { get; }
  }
}
