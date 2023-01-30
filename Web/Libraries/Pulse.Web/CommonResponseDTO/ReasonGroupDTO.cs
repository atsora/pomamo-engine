// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for ReasonGroupDTO.
  /// </summary>
  public class ReasonGroupDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }

    /// <summary>
    /// Long display: reason group with its description
    /// </summary>
    public string LongDisplay { get; set; }

    /// <summary>
    /// Description of the reason group
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Color
    /// </summary>
    public string Color { get; set; }
    
    /// <summary>
    /// Associated reasons
    /// </summary>
    public List<ReasonDTO> Reasons { get; set; }
  }
  
  /// <summary>
  /// Assembler for ReasonGroupDTO.
  /// </summary>
  public class ReasonGroupDTOAssembler : IGenericDTOAssembler<ReasonGroupDTO, Lemoine.Model.IReasonGroup> 
  {
    /// <summary>
    /// ReasonGroupDTO assembler
    /// </summary>
    /// <param name="reasonGroup"></param>
    /// <returns></returns>
    public ReasonGroupDTO Assemble(Lemoine.Model.IReasonGroup reasonGroup)
    {
      ReasonGroupDTO reasonGroupDTO = new ReasonGroupDTO();
      reasonGroupDTO.Id = reasonGroup.Id;
      reasonGroupDTO.Display = reasonGroup.Display;
      reasonGroupDTO.LongDisplay = reasonGroup.LongDisplay;
      reasonGroupDTO.Description = reasonGroup.DescriptionOrTranslation;
      reasonGroupDTO.Color = reasonGroup.Color;
      reasonGroupDTO.Reasons = new List<ReasonDTO> ();
      foreach (var reason in reasonGroup.Reasons) {
        reasonGroupDTO.Reasons.Add (new ReasonDTOAssembler ().Assemble (reason));
      }
      return reasonGroupDTO;
    }
    
    /// <summary>
    /// ReasonGroupDTO assembler
    /// </summary>
    /// <param name="reasonGroup"></param>
    /// <param name="reasons">only return the reasons that are listed here</param>
    /// <returns></returns>
    public ReasonGroupDTO Assemble(Lemoine.Model.IReasonGroup reasonGroup, IEnumerable<IReason> reasons)
    {
      ReasonGroupDTO reasonGroupDTO = new ReasonGroupDTO();
      reasonGroupDTO.Id = reasonGroup.Id;
      reasonGroupDTO.Display = reasonGroup.Display;
      reasonGroupDTO.Color = reasonGroup.Color;
      reasonGroupDTO.Reasons = new List<ReasonDTO> ();
      foreach (var reason in reasons.Where (r => r.ReasonGroup.Equals (reasonGroup))) {
        reasonGroupDTO.Reasons.Add (new ReasonDTOAssembler ().Assemble (reason));
      }
      return reasonGroupDTO;
    }
    
    /// <summary>
    /// ReasonDTO list assembler
    /// </summary>
    /// <param name="reasonGroups"></param>
    /// <returns></returns>
    public IEnumerable<ReasonGroupDTO> Assemble(IEnumerable<Lemoine.Model.IReasonGroup> reasonGroups)
    {
      IList<ReasonGroupDTO> reasonGroupsDTO = new List<ReasonGroupDTO>();
      foreach (Lemoine.Model.IReasonGroup reasonGroup in reasonGroups) {
        reasonGroupsDTO.Add(Assemble(reasonGroup));
      }
      return reasonGroupsDTO;
    }
  }
}
