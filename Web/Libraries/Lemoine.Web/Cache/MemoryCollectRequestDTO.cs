// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Net;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.Cache
{
  /// <summary>
  /// Request DTO
  /// </summary>
  [Api ("Request DTO for /Memory/Collect service")]
  [ApiResponse (HttpStatusCode.InternalServerError, "Oops, something broke")]
  [Route ("/Memory/Collect", "GET", Summary = "Force the garbage collector to collect the memory", Notes = "")]
  [AllowAnonymous]
  public class MemoryCollectRequestDTO : IReturn<OkDTO>
  {
    /// <summary>
    /// 
    /// </summary>
    [ApiMember (Name = "CompactLargeObjectHeap", Description = "Compact once the large object heap", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool CompactLargeObjectHeap { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    [ApiMember (Name = "CompactSmallObjectHeap", Description = "Compact also the small object heap", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool CompactSmallObjectHeap { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    [ApiMember (Name = "Optimized", Description = "Optimize the garbage collection", ParameterType = "path", DataType = "boolean", IsRequired = false)]
    public bool Optimized { get; set; } = false;

    /// <summary>
    /// Only applicable if CompactSmallObjectHeap or Optimized is set
    /// </summary>
    [ApiMember (Name = "Blocking", Description = "Set the blocking option of the garbage collection", ParameterType = "path", DataType = "boolean", IsRequired = false)]

    public bool Blocking { get; set; } = false;
  }
}

