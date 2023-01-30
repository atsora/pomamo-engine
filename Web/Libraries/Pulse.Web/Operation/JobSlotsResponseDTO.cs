// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Extensions.Web.Responses;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Response DTO for JobSlots
  /// </summary>
  [Api ("JobSlots Response DTO")]
  public class JobSlotsResponseDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public JobSlotsResponseDTO ()
    {
      this.Jobs = new List<JobSlotsDTO> ();
    }

    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<JobSlotsDTO> Jobs { get; set; }

    /// <summary>
    /// Requested range
    /// </summary>
    public string RequestedRange { get; set; }

    /// <summary>
    /// Range that applies (when there is a job)
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Range without any infinite bounds and limited to now
    /// </summary>
    public string LimitedRange { get; set; }
  }

  /// <summary>
  /// Group here the slot blocks for a specific job
  /// </summary>
  public class JobSlotsDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="job"></param>
    public JobSlotsDTO (Lemoine.Model.IJob job)
    {
      this.WorkOrderId = job.WorkOrderId;
      this.ProjectId = job.ProjectId;
      this.ProjectDisplay = job.Project.Display;
      this.Blocks = new List<JobSlotBlockDTO> ();
    }

    /// <summary>
    /// WorkOrder Id
    /// </summary>
    public int WorkOrderId { get; set; }

    /// <summary>
    /// Project Id
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// Project display
    /// </summary>
    public string ProjectDisplay { get; set; }

    /// <summary>
    /// Foreground color
    /// </summary>
    public string FgColor { get; set; }

    /// <summary>
    /// Background color
    /// </summary>
    public string BgColor { get; set; }

    /// <summary>
    /// List of slot blocks for this specific job
    /// </summary>
    public List<JobSlotBlockDTO> Blocks { get; set;  }

    /// <summary>
    /// Impacted machines
    /// </summary>
    public List<MachineDTO> Machines { get; set; }

    /// <summary>
    /// List of machine Ids separated by a comma
    /// </summary>
    public string MachineIds { get; set; }
  }

  /// <summary>
  /// Job slots: block
  /// 
  /// If there are more than one detail item:
  /// <item>the pattern is diagonal-stripe-3</item>
  /// <item>the color and the patterncolor are the colors of the two main items</item>
  /// 
  /// The color is generated from the job ID
  /// </summary>
  public class JobSlotBlockDTO
  {
    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="range"></param>
    public JobSlotBlockDTO (string range)
    {
      this.Range = range;
      this.Machines = new List<MachineDTO> ();
    }

    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Impacted machines
    /// </summary>
    public List<MachineDTO> Machines { get; set; }

    /// <summary>
    /// List of machine Ids separated by a comma
    /// </summary>
    public string MachineIds { get; set; }
  }
}
