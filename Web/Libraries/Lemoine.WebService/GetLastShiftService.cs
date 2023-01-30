// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Collections.Generic;
using Lemoine.DTO;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetLastShift service
  /// </summary>
  public class GetLastShiftService: GenericCachedService<Lemoine.DTO.GetLastShift>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetLastShiftService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetLastShiftService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetLastShift request)
    {
      Debug.Assert (null != request);
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        LastShiftDTO dto = new LastShiftDTO ();
        
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (request.MachineId);
        if (null == machine) {
          return ServiceHelper.NoMachineWithIdErrorDTO (request.MachineId);
        }
        
        IObservationStateSlot osSlot = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindAt(machine, DateTime.UtcNow);
        if (null != osSlot) {
          IShift shift = osSlot.Shift;
          if (null != shift) {
            dto.Shift = new ShiftDTOAssembler ().Assemble(shift);
          }
          DateTime? day = osSlot.Day;
          if (day.HasValue) {
            dto.Day = ConvertDTO.DayToString (day.Value);
          }
        }
        else { // No os/shift is defined => return just the day
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAt (DateTime.UtcNow);
          if (null != daySlot) {
            DateTime? day = daySlot.Day;
            if (day.HasValue) {
              dto.Day = ConvertDTO.DayToString (day.Value);
            }
          }
        }
        
        return dto;
      } // session
    }
  }
}
