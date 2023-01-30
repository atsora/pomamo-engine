// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.DTO;
using System.Collections.Generic;

using Lemoine.Core.Log;

namespace Lemoine.WebService
{
  /// <summary>
  /// Lemoine.DTO.GetOperationCycles service
  /// </summary>
  public class GetOperationCyclesService: GenericCachedService<Lemoine.DTO.GetOperationCycles>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (GetOperationCyclesService).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public GetOperationCyclesService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentShort)
    {
    }
    #endregion // Constructors

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (Lemoine.DTO.GetOperationCycles request)
    {
      int machineId = request.Id;
      UtcDateTimeRange range = ConvertDTO.IsoStringToUtcDateTimeRange (request.Begin, request.End);
      if (!range.Lower.HasValue) {
        return new ErrorDTO(String.Format("Parameter Begin={0} must have valid not null value.",request.Begin), ErrorStatus.PERMANENT);
      }
      if (range.IsEmpty ()) {
        return ServiceHelper.BadDateTimeRange(range);
      }
      
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById(machineId);
        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO(machineId);
        }
        IList<IOperationCycle> list = ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllInRange(machine, range);
        return (new OperationCycleDTOAssembler()).Assemble(list);
      }
    }
    
  }
}
