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
  class MaintenanceActionHistoryService : GenericSyncCachedService<MaintenanceActionHistoryRequestDTO>
  {
    static readonly string CACHE_TIMEOUT_KEY = "MaintenanceAction.History.CacheTimeOut";
    static readonly TimeSpan CACHE_TIMEOUT_DEFAULT = CacheTimeOut.CurrentLong.GetTimeSpan ();

    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceActionHistoryService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public MaintenanceActionHistoryService ()
      : base (Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    public override object GetWithoutCache (MaintenanceActionHistoryRequestDTO request)
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

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Addon.MaintenanceAction.History")) {
          var dao = new MaintenanceActionDAO ();
          var maintenanceActions = dao.FindAll (machine);

          var maintenanceActionsDto = maintenanceActions
            .Select (action => new MaintenanceActionDTO (action,
                                                         MaintenanceActionEstimatedDateTime.GetEstimatedDateTime (action)))
            .OrderBy (action => action.EstimatedDateTime);

          return new MaintenanceActionHistoryResponseDTO (maintenanceActionsDto);
        } // transaction
      } // session
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, MaintenanceActionHistoryRequestDTO requestDTO)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_KEY,
        CACHE_TIMEOUT_DEFAULT);
    }
  }
}
