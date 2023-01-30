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
  /// Request DTO for GetLastCycleWithSerialNumberV2
  /// </summary>
  [Route("/GetLastCycleWithSerialNumberV2/", "GET")]
  [Route("/GetLastCycleWithSerialNumberV2/{Id}", "GET")]
  [Route("/GetLastCycleWithSerialNumberV2/{Id}/{Begin}", "GET")]
  public class GetLastCycleWithSerialNumberV2
  {
    /// <summary>
    /// Id of requested machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Optional start of period in ISO format
    /// </summary>
    public string Begin { get; set; }
  }
}
