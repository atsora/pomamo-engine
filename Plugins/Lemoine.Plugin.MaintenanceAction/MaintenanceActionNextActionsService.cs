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
using Lemoine.Extensions.Web.Services;

namespace Lemoine.Plugin.MaintenanceAction
{
  class MaintenanceActionNextActionsService: GenericSyncCachedService<MaintenanceActionNextActionsRequestDTO>
  {
    const string DEFAULT_NUMBER_KEY = "MaintenanceAction.Next.Number";
    const int DEFAULT_NUMBER_DEFAULT = 10;
    const string CACHE_TIMEOUT_KEY = "MaintenanceAction.Next.CacheTimeOut";
    static readonly TimeSpan CACHE_TIMEOUT_DEFAULT = CacheTimeOut.CurrentLong.GetTimeSpan ();

    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceActionNextActionsService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public MaintenanceActionNextActionsService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }

    public override object GetWithoutCache (MaintenanceActionNextActionsRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ("Web.Addon.MaintenanceAction.NextActions")) {
          int number = Lemoine.Info.ConfigSet.LoadAndGet<int> (DEFAULT_NUMBER_KEY, DEFAULT_NUMBER_DEFAULT);
          if (request.Number.HasValue) {
            number = request.Number.Value;
          }

          var dao = new MaintenanceActionDAO ();
          var maintenanceActions = dao.FindOpen ();

          var maintenanceActionsDto = maintenanceActions
            .Select (action => new MaintenanceActionWithMachineDTO (action,
                                                                    MaintenanceActionEstimatedDateTime.GetEstimatedDateTime (action)))
            .OrderBy (action => action.EstimatedDateTime)
            .Take (number);

          return new MaintenanceActionNextActionsResponseDTO (maintenanceActionsDto);
        } // transaction
      } // session
    }

    /// <summary>
    /// Get the cache time out
    /// </summary>
    /// <param name="url"></param>
    /// <param name="requestDTO"></param>
    /// <returns></returns>
    protected override TimeSpan GetCacheTimeOut (string url, MaintenanceActionNextActionsRequestDTO requestDTO)
    {
      return Lemoine.Info.ConfigSet.LoadAndGet<TimeSpan> (CACHE_TIMEOUT_KEY,
        CACHE_TIMEOUT_DEFAULT);
    }
  }
}
