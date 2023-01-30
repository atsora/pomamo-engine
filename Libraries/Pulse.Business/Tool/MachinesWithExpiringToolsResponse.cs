// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Business.Tool
{
  /// <summary>
  /// Response of request MachinesWithExpiringTools
  /// </summary>
  public class MachinesWithExpiringToolsResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public MachinesWithExpiringToolsResponse ()
    {
      this.DateTime = DateTime.UtcNow;
      this.Machines = new List<IMachine> ();
    }

    /// <summary>
    /// UTC date/time of the response
    /// </summary>
    public DateTime DateTime { get; private set; }

    /// <summary>
    /// List of machines
    /// </summary>
    public IList<IMachine> Machines { get; set; }

    /// <summary>
    /// Remaining time of the next machine with expiring tools
    /// </summary>
    public TimeSpan? NextRemainingTime { get; set; }
  }
}
