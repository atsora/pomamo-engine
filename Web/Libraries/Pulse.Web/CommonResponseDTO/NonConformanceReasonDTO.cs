// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for NonConformanceReasonDTO.
  /// </summary>
  public class NonConformanceReasonDTO
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
  /// Assembler for NonConformanceReasonDTO.
  /// </summary>
  public class NonConformanceReasonDTOAssembler : IGenericDTOAssembler<NonConformanceReasonDTO, Lemoine.Model.INonConformanceReason>
  {
    /// <summary>
    /// NonConformanceReasonDTO assembler
    /// </summary>
    /// <param name="nonConformanceReason"></param>
    /// <returns></returns>
    public NonConformanceReasonDTO Assemble (Lemoine.Model.INonConformanceReason nonConformanceReason)
    {
      var reasonDTO = new NonConformanceReasonDTO ();
      reasonDTO.Id = nonConformanceReason.Id;
      reasonDTO.Display = nonConformanceReason.Display;
      // TODO: ...
      /*
      reasonDTO.LongDisplay = nonConformanceReason.LongDisplay;
      reasonDTO.Description = nonConformanceReason.DescriptionOrTranslation;
      reasonDTO.Color = nonConformanceReason.Color;
      */
      return reasonDTO;
    }

    /// <summary>
    /// ReasonDTO list assembler
    /// </summary>
    /// <param name="nonConformanceReasons"></param>
    /// <returns></returns>
    public IEnumerable<NonConformanceReasonDTO> Assemble (IEnumerable<Lemoine.Model.INonConformanceReason> nonConformanceReasons)
    {
      IList<NonConformanceReasonDTO> reasonDTOList = new List<NonConformanceReasonDTO> ();
      foreach (var reason in nonConformanceReasons) {
        reasonDTOList.Add (Assemble (reason));
      }
      return reasonDTOList;
    }
  }
}
