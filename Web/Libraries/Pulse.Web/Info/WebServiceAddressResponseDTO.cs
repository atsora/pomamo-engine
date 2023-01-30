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

namespace Pulse.Web.Info
{
  /// <summary>
  /// Reponse DTO for the WebServiceAddress service
  /// </summary>
  [Api("WebServiceAddress Response DTO")]
  public class WebServiceAddressResponseDTO
  {
    /// <summary>
    /// Address of the web service, for example lctr or web1
    /// </summary>
    public string Address { get; set; }
    
    /// <summary>
    /// Base URL to use for the web service with a trailing /, for example http://lctr:5000/ or https://lctr:5001/ or http://web1:8081/
    /// </summary>
    public string Url { get; set; }
  }
}

