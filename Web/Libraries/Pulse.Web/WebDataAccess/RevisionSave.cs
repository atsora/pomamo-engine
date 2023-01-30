// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

using System.Net;
using Pulse.Web.WebDataAccess.CommonResponseDTO;


namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for Revision/Save service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Revision/Save/", "GET", Summary = "Deprecated RevisionDAO.MakePersistent", Notes = "To use with ?Application=&IPAddress=&Comment=&UpdaterId=")]
  [Route("/Data/Revision/Save/", "GET", Summary = "RevisionDAO.MakePersistent", Notes = "To use with ?Application=&IPAddress=&Comment=&UpdaterId=")]
  public class RevisionSave : IReturn<SaveResponseDTO>
  {
    /// <summary>
    /// Application
    /// </summary>
    [ApiMember(Name = "Application", Description = "", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Application { get; set; }

    /// <summary>
    /// IP Address
    /// </summary>
    [ApiMember(Name = "IPAddress", Description = "", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string IPAddress { get; set; }

    /// <summary>
    /// Comment
    /// </summary>
    [ApiMember(Name = "Comment", Description = "", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Comment { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    [ApiMember(Name = "UserId", Description = "", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? UserId { get; set; }

    /// <summary>
    /// Service ID
    /// </summary>
    [ApiMember(Name = "ServiceId", Description = "", ParameterType = "path", DataType = "int", IsRequired = false)]
    public int? ServiceId { get; set; }
  }
}
