// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

using System.Net;


namespace Lemoine.Web.Misc
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api("Request DTO for EmailSend service")]
  [ApiResponse(HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route("/Misc/Email/Send/Get", "GET", Summary = "Service to send an e-mail", Notes = "To use with ?To=&Subject=&Body=")]
  [Route("/Misc/Email/Send/Get/{To}/{Subject}/{Body}", "GET", Summary = "Service to send an e-mail", Notes = "")]
  [AllowAnonymous]
  public class EmailSendRequestDTO : IReturn<EmailSendResponseDTO>
  {
    /// <summary>
    /// Sender
    /// </summary>
    [ApiMember(Name = "From", Description = "e-mail sender", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string From { get; set; }

    /// <summary>
    /// Recipient
    /// </summary>
    [ApiMember(Name = "To", Description = "e-mail recipient", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string To { get; set; }

    /// <summary>
    /// Cc
    /// </summary>
    [ApiMember(Name = "Cc", Description = "e-mail Cc", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Cc { get; set; }
    
    /// <summary>
    /// Bcc
    /// </summary>
    [ApiMember(Name = "Bcc", Description = "e-mail Bcc", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Bcc { get; set; }

    /// <summary>
    /// Subject
    /// </summary>
    [ApiMember(Name = "Subject", Description = "e-mail subject", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Subject { get; set; }

    /// <summary>
    /// Body
    /// </summary>
    [ApiMember(Name = "Body", Description = "e-mail body (GET only)", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Body { get; set; }
  }

  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for EmailSend service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Misc/Email/Send/Post", "POST", Summary = "Service to send an e-mail. The body is in the POST", Notes = "To use with ?To=&Subject=&Body=")]
  [Route ("/Misc/Email/Send/Post/{To}/{Subject}", "POST", Summary = "Service to send an e-mail. The body is in the POST", Notes = "")]
  public class EmailSendPostRequestDTO : IReturn<EmailSendResponseDTO>
  {
    /// <summary>
    /// Sender
    /// </summary>
    [ApiMember (Name = "From", Description = "e-mail sender", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string From { get; set; }

    /// <summary>
    /// Recipient
    /// </summary>
    [ApiMember (Name = "To", Description = "e-mail recipient", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string To { get; set; }

    /// <summary>
    /// Cc
    /// </summary>
    [ApiMember (Name = "Cc", Description = "e-mail Cc", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Cc { get; set; }

    /// <summary>
    /// Bcc
    /// </summary>
    [ApiMember (Name = "Bcc", Description = "e-mail Bcc", ParameterType = "path", DataType = "string", IsRequired = false)]
    public string Bcc { get; set; }

    /// <summary>
    /// Subject
    /// </summary>
    [ApiMember (Name = "Subject", Description = "e-mail subject", ParameterType = "path", DataType = "string", IsRequired = true)]
    public string Subject { get; set; }
  }
}
