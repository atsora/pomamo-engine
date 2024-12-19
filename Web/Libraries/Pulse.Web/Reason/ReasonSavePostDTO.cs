// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;
using System.Text.Json;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// Post request DTO for ReasonSave
  /// </summary>
  public class ReasonSavePostDTO : Lemoine.Web.CommonRequestDTO.RangesPostDTO
  {
    /// <summary>
    /// Reason data
    /// </summary>
    [ApiMember (Name = "ReasonData", Description = "Reason data", ParameterType = "post", DataType = "JsonElement", IsRequired = false)]
    public JsonElement? ReasonData { get; set; }
  }
}
