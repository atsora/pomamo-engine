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
  /// Request DTO for GetCyclesInPeriod version 2.
  /// </summary>
  [Route("/GetCyclesWithWorkInformationsInPeriodV2/", "GET")]
  [Route("/GetCyclesWithWorkInformationsInPeriodV2/{Id}", "GET")]
  [Route("/GetCyclesWithWorkInformationsInPeriodV2/{Id}/{Begin}", "GET")]
  [Route("/GetCyclesWithWorkInformationsInPeriodV2/{Id}/{Begin}/{End}", "GET")]
  public class GetCyclesWithWorkInformationsInPeriodV2
  {
    /// <summary>
    /// Id of requested machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Optional start of period in Iso string format
    /// </summary>
    public string Begin { get; set; }

    /// <summary>
    /// Optional end of period in Iso string format
    /// </summary>
    public string End { get; set; }
  }
}
