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

  /// <summary>
  /// Reason data
  /// </summary>
  public class ReasonDataPostDTO
  {
    /// <summary>
    /// Reason data key
    /// </summary>
    [ApiMember (Name = "Key", Description = "Reason data key", ParameterType = "post", DataType = "string", IsRequired = true)]
    public string Key { get; set; }

    /// <summary>
    /// Reason data value
    /// </summary>
    [ApiMember (Name = "Value", Description = "Reason data value", ParameterType = "post", DataType = "object", IsRequired = true)]
    public object Value { get; set; }
  }
}
