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
using Lemoine.Web;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for MachineObservationState
  /// </summary>
  [Api("MachineObservationState Response DTO")]
  public class MachineObservationStateDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Foreground color
    /// </summary>
    public string FgColor { get; set; }
    
    /// <summary>
    /// Background color
    /// </summary>
    public string BgColor { get; set; }
  }
  
  /// <summary>
  /// Assembler for MachineObservationStateDTO
  /// </summary>
  public class MachineObservationStateDTOAssembler : IGenericDTOAssembler<MachineObservationStateDTO, Lemoine.Model.IMachineObservationState>
  {
    /// <summary>
    /// MachineObservationStateDTO assembler
    /// </summary>
    /// <param name="machineObservationState">nullable</param>
    /// <returns></returns>
    public MachineObservationStateDTO Assemble(Lemoine.Model.IMachineObservationState machineObservationState)
    {
      if (null == machineObservationState) {
        return null;
      }
      MachineObservationStateDTO machineObservationStateDTO = new MachineObservationStateDTO();
      machineObservationStateDTO.Id = ((Lemoine.Collections.IDataWithId<int>)machineObservationState).Id;
      machineObservationStateDTO.Display = machineObservationState.Display;
      machineObservationStateDTO.BgColor = ColorGenerator.GetColor ("MachineObservationState", machineObservationStateDTO.Id);
      machineObservationStateDTO.FgColor = ColorGenerator.GetContrastColor (machineObservationStateDTO.BgColor);
      return machineObservationStateDTO;
    }
    
    /// <summary>
    /// MachineObservationStateDTO list assembler
    /// </summary>
    /// <param name="machineObservationStates">not null</param>
    /// <returns></returns>
    public IEnumerable<MachineObservationStateDTO> Assemble(IEnumerable<Lemoine.Model.IMachineObservationState> machineObservationStates)
    {
      Debug.Assert (null != machineObservationStates);
      
      IList<MachineObservationStateDTO> result = new List<MachineObservationStateDTO> ();
      foreach (var machineObservationState in machineObservationStates) {
        result.Add (Assemble (machineObservationState));
      }
      return result;
    }
  }
}
