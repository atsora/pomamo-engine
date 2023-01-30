// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Web;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// UtilizationTarget service
  /// 
  /// Service to get only the % utilization target.
  /// 
  /// To get the % utilization, use the Utilization service
  /// </summary>
  public class UtilizationTargetService
    : GenericCachedService<UtilizationTargetRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UtilizationTargetService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public UtilizationTargetService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.Config)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(UtilizationTargetRequestDTO request)
    {
      UtilizationTargetResponseDTO response = new UtilizationTargetResponseDTO ();
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.UtilizationTarget"))
        {
          int machineId = request.MachineId;
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (machineId);
          if (null == machine) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "unknown machine with ID {0}",
                             machineId);
            return new ErrorDTO ("No monitored machine with the specified ID",
                                 ErrorStatus.WrongRequestParameter);
          }
          
          IGoal goal = ModelDAOHelper.DAOFactory.GoalDAO
            .FindMatch (GoalTypeId.UtilizationPercentage, null, machine);
          if (null == goal) {
            log.ErrorFormat ("GetWithoutCache: " +
                             "no defined utilization target for machine ID {0}",
                             machineId);
            return new ErrorDTO ("No utilization target is defined",
                                 ErrorStatus.MissingConfiguration);
          }
          
          response.Machine = new MachineDTOAssembler ().Assemble (machine);
          response.TargetPercentage = goal.Value / 100.0;
        } // transaction
      } // session

      return response;
    }
    #endregion // Methods
  }
}
