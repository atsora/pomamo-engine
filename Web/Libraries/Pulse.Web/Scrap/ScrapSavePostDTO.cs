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
using System.ComponentModel;
using Lemoine.Web.CommonRequestDTO;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// Reason part of <see cref="ScrapSavePostDTO"/>
  /// </summary>
  public class ScrapSaveReasonPostDTO
  {
    [ApiMember (Name = "Id", Description = "Non conformance reason id", ParameterType = "post", DataType = "int", IsRequired = true)]
    public int Id { get; set; }

    [ApiMember (Name = "Number", Description = "Number of associated parts", ParameterType = "post", DataType = "int", IsRequired = true)]
    public int Number { get; set; }
  }

  /// <summary>
  /// Post request DTO for ReasonSave
  /// </summary>
  public class ScrapSavePostDTO : PostDTO
  {
    [ApiMember (Name = "Id", Description = "Id of a previous scrap report (Default is 0, not applicable)", ParameterType = "post", DataType = "long", IsRequired = false)]
    [DefaultValue (0)]
    public long Id { get; set; } = 0;

    [ApiMember (Name = "Range", Description = "Date/time range", ParameterType = "post", DataType = "String", IsRequired = true)]
    public string Range { get; set; }

    [ApiMember (Name = "ValidCount", Description = "Number of valid parts", ParameterType = "post", DataType = "int", IsRequired = false)]
    [DefaultValue (0)]
    public int ValidCount { get; set; } = 0;

    [ApiMember (Name = "Details", Description = "Additional comment", ParameterType = "post", DataType = "String", IsRequired = false)]
    [DefaultValue ("")]
    public string Details { get; set; } = "";

    [ApiMember (Name = "Reasons", Description = "Quantity per non conformance reason", ParameterType = "post", DataType = "list", IsRequired = false)]
    public IList<ScrapSaveReasonPostDTO> Reasons { get; set; } = new List<ScrapSaveReasonPostDTO> ();
  }
}
