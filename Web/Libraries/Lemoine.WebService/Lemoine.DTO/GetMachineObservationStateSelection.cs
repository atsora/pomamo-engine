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
  /// Request DTO for GetMachineObservationStateSelection
  /// </summary>
  [Route("/GetMachineObservationStateSelection/", "GET")]
  public class GetMachineObservationStateSelection
  {
    // nothing
  }
}
