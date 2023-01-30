// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Model;

namespace Pulse.Extensions.Web.Responses
{
  /// <summary>
  /// Response DTO for Machine
  /// </summary>
  [Api("Machine Response DTO")]
  public class MachineDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Display priority of the machine (-1 if not set)
    /// </summary>
    public int DisplayPriority { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machine"></param>
    public MachineDTO (IMachine machine)
    {
      this.Id = machine.Id;
      this.Display = machine.Display;
      this.DisplayPriority = machine.DisplayPriority ?? -1;
    }
  }
}
