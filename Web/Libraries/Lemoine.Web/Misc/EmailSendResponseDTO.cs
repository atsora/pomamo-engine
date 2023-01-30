// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Misc
{
  /// <summary>
  /// Response DTO for EmailSend service
  /// </summary>
  [Api("EmailSend Response DTO")]
  public class EmailSendResponseDTO: OkDTO
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public EmailSendResponseDTO ()
      : base ("E-mail sent successfully")
    { }
  }
}
