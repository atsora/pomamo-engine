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

namespace Pulse.Web.CncValue
{
  /// <summary>
  /// Response DTO for CncValueAt
  /// </summary>
  [Api("CncValueAt Response DTO")]
  public class CncValueAtResponseDTO
  {
    /// <summary>
    /// Data by machine module
    /// </summary>
    public List<CncValueAtByMachineModuleDTO> ByMachineModule { get; set; }
    
    /// <summary>
    /// At
    /// </summary>
    public string At { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="at"></param>
    internal CncValueAtResponseDTO (string at)
    {
      this.At = at;
      this.ByMachineModule = new List<CncValueAtByMachineModuleDTO> ();
    }
  }
  
  /// <summary>
  /// Response DTO for CncValueAt by machine module
  /// </summary>
  public class CncValueAtByMachineModuleDTO
  {
    /// <summary>
    /// Reference to the machine module
    /// </summary>
    public MachineModuleDTO MachineModule { get; set; }
    
    /// <summary>
    /// Data by field
    /// </summary>
    public List<CncValueAtByMachineModuleFieldDTO> ByField { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="isMain"></param>
    internal CncValueAtByMachineModuleDTO (IMachineModule machineModule, bool isMain)
    {
      Debug.Assert (null != machineModule);
      
      this.MachineModule = new MachineModuleDTO ();
      this.MachineModule.Id = machineModule.Id;
      this.MachineModule.Display = machineModule.Display;
      this.MachineModule.Main = isMain;
      this.ByField = new List<CncValueAtByMachineModuleFieldDTO> ();
    }
  }
  
  /// <summary>
  /// Response DTO for CncValueAt by field
  /// </summary>
  public class CncValueAtByMachineModuleFieldDTO
  {
    /// <summary>
    /// Reference to the field
    /// </summary>
    public FieldDTO Field { get; set; }
    
    /// <summary>
    /// Value
    /// </summary>
    public object Value { get; set; }
    
    /// <summary>
    /// Associated color or an empty string if not defined
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Is that the main performance field ?
    /// </summary>
    public bool PerformanceField { get; set; }
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="field"></param>
    /// <param name="isPerformanceField"></param>
    internal CncValueAtByMachineModuleFieldDTO (IField field, bool isPerformanceField)
    {
      Debug.Assert (null != field);
      
      this.Field = new FieldDTOAssembler ().Assemble (field);
      this.PerformanceField = isPerformanceField;
    }
  }

}
