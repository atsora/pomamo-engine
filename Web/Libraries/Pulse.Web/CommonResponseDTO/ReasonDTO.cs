// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for ReasonDTO.
  /// </summary>
  public class ReasonDTO
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
    /// Long display: reason with its description
    /// </summary>
    public string LongDisplay { get; set; }

    /// <summary>
    /// Alternative to Display for compatibility
    /// 
    /// Obsolete: to remove later
    /// </summary>
    public string Text
    {
      get { return this.Display; }
      set { this.Display = value; }
    }

    /// <summary>
    /// Description of the reason
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Color
    /// 
    /// Note: this will be replaced soon by the reason group color
    /// </summary>
    public string Color { get; set; }
  }

  /// <summary>
  /// Assembler for ReasonDTO.
  /// </summary>
  public class ReasonDTOAssembler : IGenericDTOAssembler<ReasonDTO, Lemoine.Model.IReason>
  {
    /// <summary>
    /// ReasonDTO assembler
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    public ReasonDTO Assemble (Lemoine.Model.IReason reason)
    {
      ReasonDTO reasonDTO = new ReasonDTO ();
      reasonDTO.Id = reason.Id;
      reasonDTO.Display = reason.Display;
      reasonDTO.LongDisplay = reason.LongDisplay;
      reasonDTO.Description = reason.DescriptionOrTranslation;
      reasonDTO.Color = reason.Color;
      return reasonDTO;
    }

    /// <summary>
    /// ReasonDTO list assembler
    /// </summary>
    /// <param name="reasonList"></param>
    /// <returns></returns>
    public IEnumerable<ReasonDTO> Assemble (IEnumerable<Lemoine.Model.IReason> reasonList)
    {
      IList<ReasonDTO> reasonDTOList = new List<ReasonDTO> ();
      foreach (Lemoine.Model.IReason reason in reasonList) {
        reasonDTOList.Add (Assemble (reason));
      }
      return reasonDTOList;
    }
  }
}
