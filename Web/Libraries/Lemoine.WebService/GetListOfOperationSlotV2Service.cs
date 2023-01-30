// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Collections;
using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetListOfOperationSlotV2 service
  /// </summary>
  public class GetListOfOperationSlotV2Service: GenericCachedService<Lemoine.DTO.GetListOfOperationSlotV2>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetListOfOperationSlotV2Service).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetListOfOperationSlotV2Service ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetListOfOperationSlotV2 request)
    {
      int machineId = request.Id;
      DateTime dateOfRequest = DateTime.UtcNow;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machineId);
        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO(machineId);
        }
        
        DateTime? datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.Begin);
        DateTime beginDate =
          datetime.HasValue ? datetime.Value :
          dateOfRequest.Subtract(ServiceHelper.GetCurrentPeriodDuration(machine));
        
        datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.End);
        DateTime endDate =  datetime.HasValue ? datetime.Value :  dateOfRequest;
        
        if (beginDate >= endDate)  {
          // bad range not far enough in the past
          return ServiceHelper.BadDateTimeRange(beginDate, endDate);
        }

        IList<IOperationSlot> operationSlotList =
          ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRange (machine,
                              new UtcDateTimeRange (beginDate,
                                                    endDate));

        Lemoine.DTO.ListOfOperationSlotV2DTO listOfOperationSlotV2DTO =
          (new Lemoine.DTO.ListOfOperationSlotV2DTOAssembler())
          .Assemble(new Tuple<DateTime, DateTime, Tuple<IMonitoredMachine, IList<IOperationSlot>>>
                    (beginDate, endDate,
                     new Tuple<IMonitoredMachine, IList<IOperationSlot>> (machine, operationSlotList)));

        
        return listOfOperationSlotV2DTO;
      }
    }
  }
}
