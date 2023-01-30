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
  /// Request DTO for GetRangeAround
  /// </summary>
  [Route("/GetRangeAround/", "GET")]
  [Route("/GetRangeAround/{Around}", "GET")]
  [Route("/GetRangeAround/{Around}/{RangeType}", "GET")]
  [Route("/GetRangeAround/{Around}/{RangeType}/{RangeSize}", "GET")]
  public class GetRangeAround
  {
    /// <summary>
    /// Date time in ISO format
    /// </summary>
    public string Around { get; set; }
    
    /// <summary>
    /// Range Type (string) :
    ///  shift, day(default), week, month, quarter, semester, year
    /// </summary>
    public string RangeType { get; set; }
    
    /// <summary>
    /// Number of RangeType requested (default 1)
    /// </summary>
    public Nullable<int> RangeSize { get; set; }
  }
}
