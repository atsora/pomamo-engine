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
  [Api ("Request DTO for /Operation/CurrentSequence service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/CurrentSequence/", "GET", Summary = "", Notes = "To use with ?MachineId=")]
  [Route ("/Operation/CurrentSequence/Get/{MachineId}", "GET", Summary = "", Notes = "")]
  public class CurrentSequenceRequestDTO : IReturn<CurrentSequenceResponseDTO>
  {
    /// <summary>
    /// Machine Id
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }
  }
}
