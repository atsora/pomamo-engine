// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Cache;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;

namespace Lemoine.Plugin.MaintenanceAction
{
  class MaintenanceActionOpenMachineActionsService: GenericSyncCachedService<MaintenanceActionOpenMachineActionsRequestDTO>
  {
    static readonly string CACHE_TIMEOUT_KEY = "MaintenanceAction.OpenMachineActions.CacheTimeOut";
    static readonly TimeSpan CACHE_TIMEOUT_DEFAULT = CacheTimeOut.CurrentLong.GetTimeSpan ();

    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceActionOpenMachineActionsService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public MaintenanceActionOpenMachineActionsService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    public override object GetWithoutCache (MaintenanceActionOpenMachineActionsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        // - Machine
        int machineId = request.MachineId;
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (machineId);
        if (null == machine) {
          log.ErrorFormat ("Get: " +
                           "unknown machine with ID {0}",
                           machineId);
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Addon.MaintenanceAction.OpenMachineActions")) {
          var dao = new MaintenanceActionDAO ();
          var maintenanceActions = dao.FindOpen (machine);

          var maintenanceActionsDto = maintenanceActions
            .Select (action => new MaintenanceActionDTO (action,
                                                         MaintenanceActionEstimatedDateTime.GetEstimatedDateTime (action)))
            .OrderBy (action => action.EstimatedDateTime);

          return new MaintenanceActionOpenMachineActionsResponseDTO (maintenanceActionsDto);
        } // transaction
      } // session
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, MaintenanceActionOpenMachineActionsRequestDTO requestDTO)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_KEY,
        CACHE_TIMEOUT_DEFAULT);
    }

  }
}
