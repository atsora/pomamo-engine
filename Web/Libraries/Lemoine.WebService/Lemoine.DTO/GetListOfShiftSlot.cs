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
  /// Request DTO for GetListOfShiftSlot.
  /// </summary>
  [Route("/GetListOfShiftSlot/", "GET")]
  [Route("/GetListOfShiftSlot/{Begin}", "GET")]
  [Route("/GetListOfShiftSlot/{Begin}/{End}", "GET")]
  public class GetListOfShiftSlot
  {
    /// <summary>
    /// Optional start of period in ISO format
    /// </summary>
    public string Begin { get; set; }
    
    /// <summary>
    /// Optional end of period in ISO format
    /// </summary>
    public string End { get; set; }

  }
}
