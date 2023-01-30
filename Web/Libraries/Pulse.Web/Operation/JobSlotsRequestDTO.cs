// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Net;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Operation/JobSlots service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/JobSlots/", "GET", Summary = "Get the job slots in a specified range", Notes = "To use with ?Range=")]
  [Route ("/Operation/JobSlots/Get/{Range}", "GET", Summary = "Get the job slots in a specified range", Notes = "")]
  public class JobSlotsRequestDTO : IReturn<JobSlotsResponseDTO>
  {
    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that corresponds to Today
    /// </summary>
    [ApiMember (Name = "Range", Description = "Requested range. Default is today. CurrentShift is an accepted parameter", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Range { get; set; }

    /// <summary>
    /// List of machine ids (separated by a comma ',')
    /// 
    /// Default: an empty list that corresponds to all machines
    /// </summary>
    [ApiMember (Name = "MachineIds", Description = "List of machine ids separated by ','. Default is an empty list that corresponds to all machines", ParameterType = "path", DataType = "List(int)", IsRequired = false)]
    public IList<int> MachineIds { get; set; }
  }
}
