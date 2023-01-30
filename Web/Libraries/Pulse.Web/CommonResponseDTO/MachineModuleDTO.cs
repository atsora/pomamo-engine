// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for MachineModule
  /// </summary>
  [Api("MachineModule Response DTO")]
  public class MachineModuleDTO
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public MachineModuleDTO () { }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule">not null</param>
    internal protected MachineModuleDTO (Lemoine.Model.IMachineModule machineModule)
    {
      System.Diagnostics.Debug.Assert (null != machineModule);
      
      this.Id = machineModule.Id;
      this.Display = machineModule.Display;
      if ( (null != machineModule.MonitoredMachine)
          && (null != machineModule.MonitoredMachine.MainMachineModule)
          && (machineModule.Id == machineModule.MonitoredMachine.MainMachineModule.Id)) {
        this.Main = true;
      }
      else {
        this.Main = false;
      }
    }
    
    /// <summary>
    /// Id of machine module
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Is it the main machine module ?
    /// </summary>
    public bool Main { get; set; }
  }
}
