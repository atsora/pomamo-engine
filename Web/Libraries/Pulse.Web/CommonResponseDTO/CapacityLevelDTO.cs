// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// DTO for CapacityLevelDTO.
  /// </summary>
  public class CapacityLevelDTO
  {
    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display
    /// </summary>
    public string Display { get; set; }
  }

  /// <summary>
  /// Assembler for CapacityLevelDTO.
  /// </summary>
  public class CapacityLevelDTOAssembler : IGenericDTOAssembler<CapacityLevelDTO, Lemoine.Model.CapacityLevel>
  {
    /// <summary>
    /// CapacityLevelDTO assembler
    /// </summary>
    /// <param name="capacityLevel"></param>
    /// <returns></returns>
    public CapacityLevelDTO Assemble (Lemoine.Model.CapacityLevel capacityLevel)
    {
      CapacityLevelDTO capacityLevelDTO = new CapacityLevelDTO ();
      capacityLevelDTO.Id = (int)capacityLevel;
      var i18nKey = "CapacityLevel" + capacityLevel.ToString ();
      capacityLevelDTO.Display = Lemoine.I18N.PulseCatalog.GetString (i18nKey, capacityLevel.ToString ());
      return capacityLevelDTO;
    }

    /// <summary>
    /// CapacityLevelDTO list assembler
    /// </summary>
    /// <param name="capacityLevels"></param>
    /// <returns></returns>
    public IEnumerable<CapacityLevelDTO> Assemble (IEnumerable<Lemoine.Model.CapacityLevel> capacityLevels)
    {
      IList<CapacityLevelDTO> capacityLevelsDTO = new List<CapacityLevelDTO> ();
      foreach (Lemoine.Model.CapacityLevel capacityLevel in capacityLevels) {
        capacityLevelsDTO.Add (Assemble (capacityLevel));
      }
      return capacityLevelsDTO;
    }
  }
}
