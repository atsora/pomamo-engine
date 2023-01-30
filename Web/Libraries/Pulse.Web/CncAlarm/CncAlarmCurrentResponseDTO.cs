// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
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
  /// Response DTO for CncAlarmCurrent
  /// </summary>
  [Api("CncAlarmCurrent Response DTO")]
  public class CncAlarmCurrentResponseDTO
  {
    /// <summary>
    /// Data by machine module
    /// </summary>
    public List<CncAlarmCurrentByMachineModuleDTO> ByMachineModule { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    internal CncAlarmCurrentResponseDTO ()
    {
      this.ByMachineModule = new List<CncAlarmCurrentByMachineModuleDTO> ();
    }
  }

  /// <summary>
  /// Response DTO for CncAlarmCurrent by machine module
  /// </summary>
  public class CncAlarmCurrentByMachineModuleDTO
  {
    /// <summary>
    /// Reference to the machine module
    /// </summary>
    public MachineModuleDTO MachineModule { get; set; }
    
    /// <summary>
    /// Data
    /// </summary>
    public List<CncAlarmCurrentByMachineModuleDataDTO> CncAlarms { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="isMain"></param>
    internal CncAlarmCurrentByMachineModuleDTO (IMachineModule machineModule, bool isMain)
    {
      Debug.Assert (null != machineModule);

      this.MachineModule = new MachineModuleDTO ();
      this.MachineModule.Id = machineModule.Id;
      this.MachineModule.Display = machineModule.Display;
      this.MachineModule.Main = isMain;
      this.CncAlarms = new List<CncAlarmCurrentByMachineModuleDataDTO> ();
    }
  }

  /// <summary>
  /// Response DTO for CncAlarmAt by field
  /// </summary>
  public class CncAlarmCurrentByMachineModuleDataDTO
  {
    /// <summary>
    /// UTC date/time of the latest update
    /// </summary>
    public string DateTime { get; set; }

    /// <summary>
    /// Cnc alarm display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Associated color or an empty string if not defined
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Alarm CncInfo
    /// </summary>
    public string CncInfo { get; set; }
    
    /// <summary>
    /// Alarm CncSubInfo
    /// </summary>
    public string CncSubInfo { get; set; }
    
    /// <summary>
    /// Alarm type
    /// </summary>
    public string Type { get; set; }
    
    /// <summary>
    /// Alarm Number (as a string)
    /// </summary>
    public string Number { get; set; }
    
    /// <summary>
    /// Alarm Message
    /// </summary>
    public string Message { get ; set; }

    /// <summary>
    /// Properties (stored as jsonb in the database)
    /// </summary>
    public IDictionary<string, object> Properties { get; set; }
    
    /// <summary>
    /// Severity name if any
    /// </summary>
    public string Severity { get; set; }

    /// <summary>
    /// Severity description if any
    /// </summary>
    public string SeverityDescription { get; set; }

    /// <summary>
    /// Program stop status if any
    /// </summary>
    public string Stop { get; set; }

    /// <summary>
    /// Is there a focus on this alarm ?
    /// </summary>
    public bool? Focus { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    internal CncAlarmCurrentByMachineModuleDataDTO()
    {
    }
  }
}
