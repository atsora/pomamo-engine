// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetLastCycleWithSerialNumberV2 service
  /// </summary>
  public class GetLastCycleWithSerialNumberV2Service: GenericCachedService<Lemoine.DTO.GetLastCycleWithSerialNumberV2>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetLastCycleWithSerialNumberV2Service).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetLastCycleWithSerialNumberV2Service ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetLastCycleWithSerialNumberV2 request)
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
        
        Lemoine.DTO.LastCycleWithSerialNumberV2DTO lastCycleWithSerialNumberV2DTO =
          (new Lemoine.DTO.LastCycleWithSerialNumberV2DTOAssembler())
          .Assemble(new Tuple<IMonitoredMachine, DateTime>(machine,
                                                                              beginDate));
        
        return lastCycleWithSerialNumberV2DTO;
      }
    }
  }
}
