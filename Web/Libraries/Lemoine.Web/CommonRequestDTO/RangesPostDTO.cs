// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;

namespace Lemoine.Web.CommonRequestDTO
{
  /// <summary>
  /// Post request DTO for ranges list
  /// </summary>
  public class RangesPostDTO: PostDTO
  {
    /// <summary>
    /// Date/time range
    /// 
    /// Default: "" that would correspond to [now, now]
    /// </summary>
    [ApiMember(Name="Ranges", Description="Requested date/time ranges", ParameterType="path", DataType="string", IsRequired=false)]
    public List<string> Ranges { get; set; }
    
    /// <summary>
    /// Convert the string ranges into a list of UtcDateTimeRange
    /// </summary>
    /// <returns></returns>
    public IList<UtcDateTimeRange> Convert ()
    {
      IList<UtcDateTimeRange> result = new List<UtcDateTimeRange> ();
      foreach (var stringRange in Ranges) {
        UtcDateTimeRange range;
        if (string.IsNullOrEmpty (stringRange)) {
          range = new UtcDateTimeRange (DateTime.UtcNow, DateTime.UtcNow, "[]");
        }
        else {
          range = new UtcDateTimeRange (stringRange);
        }
        result.Add (range);
      }
      return result;
    }
  }
}
