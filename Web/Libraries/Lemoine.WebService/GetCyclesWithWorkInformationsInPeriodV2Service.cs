// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetCyclesWithWorkInformationsInPeriodV2 service
  /// </summary>
  public class GetCyclesWithWorkInformationsInPeriodV2Service: GenericCachedService<Lemoine.DTO.GetCyclesWithWorkInformationsInPeriodV2>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetCyclesWithWorkInformationsInPeriodV2Service).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetCyclesWithWorkInformationsInPeriodV2Service ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetCyclesWithWorkInformationsInPeriodV2 request)
    {
      int machineId = request.Id;
      DateTime dateOfRequest = DateTime.UtcNow;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machineId);
        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO(machineId);
        }
        DateTime? datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.Begin);
        
        DateTime beginDate = datetime.HasValue ? 
          datetime.Value : dateOfRequest.Subtract(ServiceHelper.GetCurrentPeriodDuration(machine));
        
        datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.End);
        DateTime endDate =  datetime.HasValue ? datetime.Value : dateOfRequest;
        
        if (beginDate >= endDate)  {
          // bad range not far enough in the past
          return ServiceHelper.BadDateTimeRange(beginDate, endDate);
        }

        IList<IOperationCycle> operationCycleList =
          ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllInRange(machine,
                                                                     new UtcDateTimeRange (beginDate, endDate));

        Lemoine.DTO.CyclesWithWorkInformationsInPeriodV2DTO cyclesWithWorkInformationsInPeriodV2DTO =
          (new Lemoine.DTO.CyclesWithWorkInformationsInPeriodV2DTOAssembler())
          .Assemble(new Tuple<DateTime,DateTime,IList<IOperationCycle>>(beginDate,endDate,operationCycleList));

        return cyclesWithWorkInformationsInPeriodV2DTO;
      }
      
    }
  }
}
