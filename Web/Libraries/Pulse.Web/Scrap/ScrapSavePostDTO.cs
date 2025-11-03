// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using System.Text.Json;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Post request DTO for ReasonSave
  /// </summary>
  public class ScrapSavePostDTO : Lemoine.Web.CommonRequestDTO.RangesPostDTO
  {
    [ApiMember (Name = "Range", Description = "Date/time range", ParameterType = "post", DataType = "String", IsRequired = true)]
    public string Range { get; set; }

    // TODO: update scrapreport

    /// <summary>
    /// Reason data
    /// </summary>
    [ApiMember (Name = "ReasonData", Description = "Reason data", ParameterType = "post", DataType = "JsonElement", IsRequired = false)]
    public JsonElement? ReasonData { get; set; }
  }
}
