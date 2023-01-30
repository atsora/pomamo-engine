// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
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
  /// Response DTO for CncValueCurrent
  /// </summary>
  [Api ("CncValueCurrent Response DTO")]
  public class CncValueCurrentResponseDTO
  {
    /// <summary>
    /// Data by machine module
    /// </summary>
    public List<CncValueCurrentByMachineModuleDTO> ByMachineModule { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    internal CncValueCurrentResponseDTO ()
    {
      this.ByMachineModule = new List<CncValueCurrentByMachineModuleDTO> ();
    }
  }

  /// <summary>
  /// Response DTO for CncValueCurrent by machine module
  /// </summary>
  public class CncValueCurrentByMachineModuleDTO
  {
    /// <summary>
    /// Reference to the machine module
    /// </summary>
    public MachineModuleDTO MachineModule { get; set; }

    /// <summary>
    /// Data by field
    /// </summary>
    public List<CncValueCurrentByMachineModuleFieldDTO> ByField { get; set; }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="machineModule"></param>
    /// <param name="isMain"></param>
    internal CncValueCurrentByMachineModuleDTO (IMachineModule machineModule, bool isMain)
    {
      Debug.Assert (null != machineModule);

      this.MachineModule = new MachineModuleDTO ();
      this.MachineModule.Id = machineModule.Id;
      this.MachineModule.Display = machineModule.Display;
      this.MachineModule.Main = isMain;
      this.ByField = new List<CncValueCurrentByMachineModuleFieldDTO> ();
    }
  }

  /// <summary>
  /// Response DTO for CncValueCurrent by field
  /// </summary>
  public class CncValueCurrentByMachineModuleFieldDTO
  {
    /// <summary>
    /// UTC date/time of the latest update
    /// </summary>
    public string DateTime { get; set; }

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
    /// Constructor
    /// </summary>
    /// <param name="currentCncValue">not null</param>
    internal CncValueCurrentByMachineModuleFieldDTO (ICurrentCncValue currentCncValue)
    {
      Debug.Assert (null != currentCncValue);
      Debug.Assert (null != currentCncValue.Field);

      this.DateTime = ConvertDTO.DateTimeUtcToIsoString (currentCncValue.DateTime);
      this.Field = new FieldDTOAssembler ().Assemble (currentCncValue.Field);
      if (currentCncValue.Field.Id.Equals ((int)FieldId.StackLight)) {
        this.Value = new StackLightDTO ((StackLight)currentCncValue.Value);
      }
      else {
        this.Value = currentCncValue.Value;
      }
      this.Color = Lemoine.Business.ServiceProvider
        .Get (new Lemoine.Business.Field.FieldValueColor (currentCncValue.Field, currentCncValue.Value));
    }
  }

}
