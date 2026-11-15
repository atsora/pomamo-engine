// Copyright (C) 2026 Atsora Solutions

using System;
using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Pulse.Web.Doc
{
  /// <summary>
  /// Request DTO for the /Doc/WorkInfoDocCount service
  /// </summary>
  [Api ("Request DTO for /Doc/WorkInfoDocCount service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Doc/WorkInfoDocCount", "GET", Summary = "Number of distinct documents of an operation (its active revision) and/or of a component (or part)", Notes = "To use with ?OperationId=&ComponentId=, at least one of them")]
  public class WorkInfoDocCountRequestDTO : IReturn<WorkInfoDocCountResponseDTO>
  {
    /// <summary>
    /// Id of the operation: the documents of its active revision are counted
    /// </summary>
    [ApiMember (Name = "OperationId", Description = "Id of the operation: the documents of its active revision are counted", ParameterType = "query", DataType = "int", IsRequired = false)]
    public int? OperationId { get; set; }

    /// <summary>
    /// Id of the component, or of the part (a part shares the id of its component)
    /// </summary>
    [ApiMember (Name = "ComponentId", Description = "Id of the component, or of the part (a part shares the id of its component)", ParameterType = "query", DataType = "int", IsRequired = false)]
    public int? ComponentId { get; set; }
  }
}
