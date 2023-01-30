// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Output DTO to report some data on a shift
  /// </summary>
  public class ShiftDTO
  {
    /// <summary>
    /// Shift Id
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Shift display
    /// </summary>
    public string Display { get; set; }
    
    /// <summary>
    /// Color of the shift
    /// </summary>
    public string Color { get; set; }
  }

  /// <summary>
  /// Assembler for of ShiftDTO.
  /// </summary>
  public class ShiftDTOAssembler : IGenericDTOAssembler<ShiftDTO, Lemoine.Model.IShift>
  {
    readonly ILog log = LogManager.GetLogger<ShiftDTOAssembler> ();

    /// <summary>
    /// ShiftDTO assembler
    /// </summary>
    /// <param name="shift">Not null</param>
    /// <returns></returns>
    public ShiftDTO Assemble(Lemoine.Model.IShift shift)
    {
      Debug.Assert (null != shift);

      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (shift)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.ShiftDTO.Assemble")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ShiftDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)shift).Id);
            if (null == initialized) {
              log.Error ($"Assemble: shift with id {((Lemoine.Collections.IDataWithId<int>)shift).Id} does not exist");
              return null;
            }
            else {
              return Assemble (initialized);
            }
          }
        }
      }

      ShiftDTO shiftDTO = new ShiftDTO();
      shiftDTO.Id = shift.Id;
      shiftDTO.Display = shift.Display;
      shiftDTO.Color = shift.Color;

      return shiftDTO;
    }
    
    /// <summary>
    /// ShiftDTO list assembler
    /// </summary>
    /// <param name="shifts"></param>
    /// <returns></returns>
    public IEnumerable<ShiftDTO> Assemble(IEnumerable<Lemoine.Model.IShift> shifts)
    {
      IList<ShiftDTO> shiftDTOs = new List<ShiftDTO>();
      foreach (Lemoine.Model.IShift shift in shifts) {
        shiftDTOs.Add(Assemble(shift));
      }
      return shiftDTOs;
    }
    
  }
}
