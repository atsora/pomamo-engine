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

namespace Pulse.Web.CncAlarm
{
  /// <summary>
  /// Response DTO for CncAlarmColor
  /// </summary>
  [Api("CncAlarmColor Response DTO")]
  public class CncAlarmColorResponseDTO
  {
    /// <summary>
    /// Reference to the slots
    /// </summary>
    public List<CncAlarmColorBlockDTO> Blocks { get; set; }
    
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
  }
  
  /// <summary>
  /// Reason color slots: block
  /// </summary>
  public class CncAlarmColorBlockDTO
  {
    /// <summary>
    /// Range
    /// </summary>
    public string Range { get; set; }
    
    /// <summary>
    /// Day if applicable
    /// </summary>
    public string Day { get; set; }

    /// <summary>
    /// Main cnc alarm color, without considering the details
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Details
    /// </summary>
    public List<CncAlarmColorBlockDetailDTO> Details { get; set; }
  }
  
  /// <summary>
  /// Block detail
  /// </summary>
  public class CncAlarmColorBlockDetailDTO
  {
    /// <summary>
    /// Reason color
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Duration in s
    /// </summary>
    public int Duration { get; set; }
  }

}
