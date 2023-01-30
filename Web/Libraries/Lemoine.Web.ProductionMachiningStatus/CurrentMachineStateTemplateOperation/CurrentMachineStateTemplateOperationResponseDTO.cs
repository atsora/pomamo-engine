// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using Lemoine.Model;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
#endif // NSERVICEKIT

namespace Lemoine.Web.ProductionMachiningStatus.CurrentMachineStateTemplateOperation
{
  /// <summary>
  /// Response DTO for one item
  /// </summary>
  [Api("Response DTO for /CurrentMachineStateTemplateOperation service")]
  public class CurrentMachineStateTemplateOperationResponseDTO
  {
    /// <summary>
    /// Start date/time of the machine state template slot / operation
    /// </summary>
    public string Since { get; set; }
    
    /// <summary>
    /// Associated machine state template
    /// </summary>
    public MachineStateTemplateDTO MachineStateTemplate { get; set; }
    
    /// <summary>
    /// Operation
    /// </summary>
    public OperationDTO Operation { get; set; }
  }
  
  /// <summary>
  /// Assembler for MachineDTO
  /// </summary>
  public class CurrentMachineStateTemplateOperationResponseDTOAssembler
  {
    /// <summary>
    /// Assemble
    /// </summary>
    /// <param name="machineStateTemplate"></param>
    /// <param name="operation"></param>
    /// <param name="since"></param>
    /// <returns></returns>
    public CurrentMachineStateTemplateOperationResponseDTO Assemble (IMachineStateTemplate machineStateTemplate,
                                                                     IOperation operation,
                                                                     Bound<DateTime> since)
    {
      var responseDTO = new CurrentMachineStateTemplateOperationResponseDTO ();
      responseDTO.MachineStateTemplate = new MachineStateTemplateDTOAssembler ().Assemble (machineStateTemplate);
      responseDTO.Operation = new OperationDTOAssembler ().AssembleLong (operation);
      responseDTO.Since = ConvertDTO.DateTimeUtcToIsoString (since);
      return responseDTO;
    }
  }
}
