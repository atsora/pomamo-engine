// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetMachineObservationStateListV2 Service.
  /// </summary>
  public class GetMachineObservationStateListV2Service : GenericCachedService<Lemoine.DTO.GetMachineObservationStateListV2>
  {

    static readonly ILog log = LogManager.GetLogger(typeof (GetMachineObservationStateListV2Service).FullName);

    #region Constructors
    /// <summary>
    /// GetMachineObservationStateListV2 is a cached service.
    /// </summary>
    public GetMachineObservationStateListV2Service () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request for GetMachineObservationStateListV2 (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(Lemoine.DTO.GetMachineObservationStateListV2 request)
    {
      int machineId = request.Id;
      DateTime dateOfRequest = DateTime.UtcNow;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machineId);
        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO(machineId);
        }
        
        DateTime? beginDateNullable = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.Begin);
        DateTime beginDate = ( beginDateNullable != null ) ? beginDateNullable.Value :
          dateOfRequest.Subtract(ServiceHelper.GetCurrentPeriodDuration(machine));
        
        if (beginDate >= dateOfRequest)  {
          // begin datetime of range not far enough in the past
          return ServiceHelper.BadDateTimeRange(beginDate, dateOfRequest);
        }
        
        DateTime? endDateNullable = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.End);
        DateTime endDate = (endDateNullable != null) ? endDateNullable.Value : dateOfRequest;
        
        if (beginDate >= endDate)  {
          // bad range not far enough in the past
          return ServiceHelper.BadDateTimeRange(beginDate, endDate);
        }

        IList<IObservationStateSlot> observationSlotList =
          ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindOverlapsRange(machine,
                             new UtcDateTimeRange (beginDate,
                                                   endDate));
        
        Lemoine.DTO.MachineObservationStateSlotListV2DTO MachineObservationStateSlotListV2DTO =
          (new Lemoine.DTO.MachineObservationStateSlotListV2DTOAssembler())
          .Assemble(new Tuple<DateTime,DateTime,IList<IObservationStateSlot>>(beginDate,endDate,observationSlotList));

        return MachineObservationStateSlotListV2DTO;
      }
    }
    #endregion // Methods
  }
}
