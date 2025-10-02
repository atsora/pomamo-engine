// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Reponse DTO for the MissingWorkInformation service
  /// </summary>
  [Api("MissingWorkInformation Response DTO")]
  public class MissingWorkInformationResponseDTO
  {
    /// <summary>
    /// Is there a  missing work information (work order or component) in the specified range ? 
    /// </summary>
    public bool IsMissingWorkInformation { get; set; }
  }
}

