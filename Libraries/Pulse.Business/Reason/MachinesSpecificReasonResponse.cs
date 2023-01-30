// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Business.Reason
{
  /// <summary>
  /// Response of request MachinesSpecificReason
  /// </summary>
  public class MachinesSpecificReasonResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public MachinesSpecificReasonResponse ()
    {
      this.DateTime = DateTime.UtcNow;
      this.MachineItems = new List<MachinesSpecificReasonMachineResponse> ();
    }

    /// <summary>
    /// UTC date/time of the response
    /// </summary>
    public DateTime DateTime { get; private set; }

    /// <summary>
    /// List of machines
    /// </summary>
    public IEnumerable<MachinesSpecificReasonMachineResponse> MachineItems { get; set; }
  }

  /// <summary>
  /// Data by machine
  /// </summary>
  public class MachinesSpecificReasonMachineResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    public MachinesSpecificReasonMachineResponse (IMachine machine)
    {
      Debug.Assert (null != machine);

      this.Machine = machine;
    }

    /// <summary>
    /// Optional start date/time.
    ///
    /// This is not set if the Sort option is false
    /// </summary>
    public LowerBound<DateTime>? StartDateTime { get; set; }

    /// <summary>
    /// Associated machine (not null)
    /// </summary>
    public IMachine Machine { get; private set; }
  }
}
