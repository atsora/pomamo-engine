// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Lemoine.Core.Log;

namespace Lemoine.Web.CommonRequestDTO
{
  /// <summary>
  /// Post request DTO for int ids list
  /// </summary>
  public class IntIdsPostDTO: PostDTO
  {
    /// <summary>
    /// Ids
    /// </summary>
    [ApiMember(Name="Ids", Description="Requested ids", ParameterType="path", DataType="List(int)", IsRequired=false)]
    public List<int> Ids { get; set; }
  }
}
