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
using Pulse.Web.CommonResponseDTO;
using Lemoine.Model;
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Response DTO of the service Operation/CurrentSequence
  /// </summary>
  [Api ("Operation/CurrentSequence Response DTO")]
  public class CurrentSequenceResponseDTO
  {
    /// <summary>
    /// Current date/time (now)
    /// </summary>
    public string CurrentDateTime { get; set; }

    /// <summary>
    /// Operation detections status date/time if known
    /// </summary>
    public string OperationDetectionStatus { get; set; }

    /// <summary>
    /// The returned data is too old (its valid date/time is older that a margin, which is by default 1 minute)
    /// </summary>
    public bool? TooOld { get; set; }

    /// <summary>
    /// Data by machine module
    /// </summary>
    public List<CurrentSequenceByMachineModuleDTO> ByMachineModule {get; set;}

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="now"></param>
    internal CurrentSequenceResponseDTO (DateTime now)
    {
      this.CurrentDateTime = ConvertDTO.DateTimeUtcToIsoString (now);
      this.ByMachineModule = new List<CurrentSequenceByMachineModuleDTO> ();
    }
  }

  /// <summary>
  /// Response DTO of the service Operation/CurrentSequence, by machine module
  /// </summary>
  public class CurrentSequenceByMachineModuleDTO
  {
    /// <summary>
    /// Associated machine module
    /// </summary>
    public MachineModuleDTO MachineModule { get; set; }

    /// <summary>
    /// Current (last identified) sequence.
    /// 
    /// If there is no active sequence, null is returned
    /// </summary>
    public SequenceDTO Sequence { get; set; }

    /// <summary>
    /// Date/time range of the last identified sequence
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Individual sequence data must be considered as too old (in case the operation detection status is unknown).
    /// 
    /// Note that you must consider first the TooOld property at the machine level
    /// </summary>
    public bool? TooOld { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    internal CurrentSequenceByMachineModuleDTO (IMachineModule machineModule, UtcDateTimeRange range)
    {
      this.MachineModule = new MachineModuleDTO (machineModule);
      this.Range = range.ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
    }
  }
}
