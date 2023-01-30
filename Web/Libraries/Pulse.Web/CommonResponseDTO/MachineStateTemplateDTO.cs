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
  /// Response DTO for MachineStateTemplate
  /// </summary>
  [Api("MachineStateTemplate Response DTO")]
  public class MachineStateTemplateDTO
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
    /// Alternative to Display for compatibility
    /// 
    /// Obsolete: to remove later - NEVER USED
    /// </summary>
    public string Text {
      get { return this.Display; }
      set { this.Display=value; }
    }
    
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
  /// Assembler for MachineStateTemplateDTO
  /// </summary>
  public class MachineStateTemplateDTOAssembler : IGenericDTOAssembler<MachineStateTemplateDTO, Lemoine.Model.IMachineStateTemplate>
  {
    /// <summary>
    /// MachineStateTemplateDTO assembler
    /// </summary>
    /// <param name="machineStateTemplate">nullable</param>
    /// <returns></returns>
    public MachineStateTemplateDTO Assemble(Lemoine.Model.IMachineStateTemplate machineStateTemplate)
    {
      if (null == machineStateTemplate) {
        return null;
      }
      MachineStateTemplateDTO machineStateTemplateDTO = new MachineStateTemplateDTO();
      machineStateTemplateDTO.Id = ((Lemoine.Collections.IDataWithId<int>)machineStateTemplate).Id;
      machineStateTemplateDTO.Display = machineStateTemplate.Display;
      machineStateTemplateDTO.BgColor = ColorGenerator.GetColor ("MachineStateTemplate", machineStateTemplateDTO.Id);
      machineStateTemplateDTO.FgColor = ColorGenerator.GetContrastColor (machineStateTemplateDTO.BgColor);
      return machineStateTemplateDTO;
    }
    
    /// <summary>
    /// MachineStateTemplateDTO list assembler
    /// </summary>
    /// <param name="machineStateTemplates">not null</param>
    /// <returns></returns>
    public IEnumerable<MachineStateTemplateDTO> Assemble(IEnumerable<Lemoine.Model.IMachineStateTemplate> machineStateTemplates)
    {
      Debug.Assert (null != machineStateTemplates);
      
      IList<MachineStateTemplateDTO> result = new List<MachineStateTemplateDTO> ();
      foreach (var machineStateTemplate in machineStateTemplates) {
        result.Add (Assemble (machineStateTemplate));
      }
      return result;
    }
  }
}
