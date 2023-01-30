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


namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for User/FindById service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/User/FindById/", "GET", Summary = "Deprecated UserDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/User/FindById/{Id}", "GET", Summary = "Deprecated UserDAO.FindById", Notes = "")]
  [Route("/Data/User/FindById/", "GET", Summary = "UserDAO.FindById", Notes = "To use with ?Id=")]
  [Route("/Data/User/FindById/{Id}", "GET", Summary = "UserDAO.FindById", Notes = "")]
  public class UserFindById : IReturn<UserDTO>
  {
    /// <summary>
    /// Id of requested User
    /// </summary>
    [ApiMember(Name = "Id", Description = "User Id", ParameterType = "path", DataType = "int", IsRequired = true)]
    public int Id { get; set; }
  }
}
