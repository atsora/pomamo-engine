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
using Lemoine.Extensions.Web.Responses;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Operation/Import service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Operation/Import/", "GET", Summary = "Import some operation data from a cycle", Notes = "To use with ?MachineId=&DateTime=")]
  [Route ("/Operation/Import/Get/{MachineId}/{DateTime}", "GET", Summary = "Import some operation data from a cycle", Notes = "")]
  public class ImportRequestDTO: IReturn<OkDTO>
  {
    /// <summary>
    /// Id of the machine
    /// </summary>
    [ApiMember (Name = "MachineId", Description = "Machine Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int MachineId { get; set; }

    /// <summary>
    /// Date/time that intersects the operation cycle
    /// </summary>
    [ApiMember (Name = "DateTime", Description = "Operation cycle date/time in UTC", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string DateTime { get; set; }

    /// <summary>
    /// Override the data if there is already a data in database
    /// </summary>
    [ApiMember (Name = "Override", Description = "Override the data if it already exists", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool Override { get; set; }

    /// <summary>
    /// Import the operation loading duration
    /// </summary>
    [ApiMember (Name = "LoadingDuration", Description = "Import the operation loading duration", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool LoadingDuration { get; set; }

    /// <summary>
    /// Import the operation machining duration
    /// </summary>
    [ApiMember (Name = "MachiningDuration", Description = "Import the operation machining duration", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool MachiningDuration { get; set; }

    /// <summary>
    /// Import the sequence durations
    /// </summary>
    [ApiMember (Name = "SequenceDuration", Description = "Import the sequence durations", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool SequenceDuration { get; set; }

    /// <summary>
    /// Import the tool number that is associated to each sequence
    /// </summary>
    [ApiMember (Name = "ToolNumber", Description = "Import the tool number that is associated to each sequence, if a tool numbers were detected", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool ToolNumber { get; set; }

    /// <summary>
    /// Consider the sequence to sequence duration instead of the raw duration of the sequence. Default false
    /// </summary>
    [ApiMember (Name = "SequenceToSequence", Description = "Consider the sequence to sequence duration instead of the raw duration of the sequence. Default false", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool SequenceToSequence { get; set; }
  }
}
