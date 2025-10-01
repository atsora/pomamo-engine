// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Core.Log;

namespace Lemoine.DTO
{
  /// <summary>
  /// Request DTO for GetLastMachineStatus (V2)
  /// </summary>
  [Route("/GetLastMachineStatusV2/", "GET")]
  [Route("/GetLastMachineStatusV2/{Id}", "GET")]
  [Route("/GetLastMachineStatusV2/{Id}/{Begin}", "GET")]
  public class GetLastMachineStatusV2
  {
    /// <summary>
    /// Id of requested machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Optional start of period in ISO format
    /// </summary>
    public string Begin { get; set; }
  }
}
