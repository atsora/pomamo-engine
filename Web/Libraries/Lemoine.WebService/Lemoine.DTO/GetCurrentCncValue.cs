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

namespace Lemoine.DTO
{
  /// <summary>
  /// Request DTO for GetCurrentCncValue
  /// 
  /// Deprecated: use /CncValue/Current instead
  /// </summary>
  [Route("/GetCurrentCncValue/", "GET")]
  [Route("/GetCurrentCncValue/{Id}", "GET")]
  public class GetCurrentCncValue
  {
    /// <summary>
    /// Id of requested machine
    /// </summary>
    public int Id { get; set; }
  }
}
