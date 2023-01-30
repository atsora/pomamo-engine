// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Collections;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.DTO;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// GetLastWorkInformationV3 service
  /// </summary>
  public class GetLastWorkInformationV3Service: GenericCachedService<Lemoine.DTO.GetLastWorkInformationV3>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetLastWorkInformationV3Service).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetLastWorkInformationV3Service () : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetLastWorkInformationV3 request)
    {
      int machineId = request.Id;
      
      DateTime dateOfRequest = DateTime.UtcNow;
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ()) {
        
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machineId);
        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO(machineId);
        }
        if(machine.OperationBar == OperationBar.None){
          MonitoredMachineOperationBarDTO monitoredMachineOperationBar = new MonitoredMachineOperationBarDTO();
          monitoredMachineOperationBar.MonitoredMachineOperationBar = OperationBar.None;
          return monitoredMachineOperationBar;
        }
        
        DateTime? datetime = null;
        if (null != request.Begin) {
          datetime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc(request.Begin);
        }
        DateTime beginDate =
          datetime.HasValue ? datetime.Value : dateOfRequest.Subtract(ServiceHelper.GetCurrentPeriodDuration(machine));

        Lemoine.DTO.LastWorkInformationV3DTO lastWorkInformationV3DTO =
          (new Lemoine.DTO.LastWorkInformationV3DTOAssembler())
          .Assemble(new Tuple<IMonitoredMachine, DateTime>(machine,
                                                                              beginDate));
        
        return lastWorkInformationV3DTO;
      }
    }
  }
}
