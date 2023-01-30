// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT
using Pulse.Web.CommonResponseDTO;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Reponse DTO for the MachineModeCategoryLegend service
  /// </summary>
  [Api("MachineModeCategoryLegend Response DTO")]
  public class MachineModeCategoryLegendResponseDTO
  {
    /// <summary>
    /// List of legend items
    /// </summary>
    public List<MachineModeCategoryDTO> Items { get; set; }
  }
}

