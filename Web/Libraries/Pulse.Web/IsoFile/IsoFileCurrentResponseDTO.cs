// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Model;

namespace Pulse.Web.IsoFile
{
  /// <summary>
  /// Response DTO of the service IsoFile/Current
  /// </summary>
  [Api ("IsoFile/CurrentIsoFile Response DTO")]
  public class IsoFileCurrentResponseDTO
  {
    /// <summary>
    /// Current date/time (now)
    /// </summary>
    public string CurrentDateTime { get; set; }

    /// <summary>
    /// Iso file detections status date/time if known
    /// </summary>
    public string DetectionStatus { get; set; }

    /// <summary>
    /// The returned data is too old (its valid date/time is older that a margin, which is by default 1 minute)
    /// </summary>
    public bool? TooOld { get; set; }

    /// <summary>
    /// Data by machine module
    /// </summary>
    public List<IsoFileCurrentByMachineModuleDTO> ByMachineModule { get; set; }

    /// <summary>
    /// Summary of all the iso files in the modules
    /// </summary>
    public string IsoFiles { get; set; } = null;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="now"></param>
    internal IsoFileCurrentResponseDTO (DateTime now)
    {
      this.CurrentDateTime = ConvertDTO.DateTimeUtcToIsoString (now);
      this.ByMachineModule = new List<IsoFileCurrentByMachineModuleDTO> ();
    }
  }

  /// <summary>
  /// Response DTO of the service IsoFile/Current, by machine module
  /// </summary>
  public class IsoFileCurrentByMachineModuleDTO
  {
    /// <summary>
    /// Associated machine module
    /// </summary>
    public MachineModuleDTO MachineModule { get; set; }

    /// <summary>
    /// Iso file detections status date/time if known
    /// </summary>
    public string DetectionStatus { get; set; }

    /// <summary>
    /// Current (last identified) ISO file / NC Program.
    /// 
    /// If there is no active current iso file, null is returned
    /// </summary>
    public IsoFileDTO IsoFile { get; set; }

    /// <summary>
    /// Date/time range of the last identified iso file
    /// </summary>
    public string Range { get; set; }

    /// <summary>
    /// Individual iso file data must be considered as too old (in case the operation detection status is unknown).
    /// 
    /// Note that you must consider first the TooOld property at the machine level
    /// </summary>
    public bool? TooOld { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    internal IsoFileCurrentByMachineModuleDTO (IMachineModule machineModule, UtcDateTimeRange range)
    {
      this.MachineModule = new MachineModuleDTO (machineModule);
      this.Range = range.ToString (dt => ConvertDTO.DateTimeUtcToIsoString (dt));
    }
  }
}
