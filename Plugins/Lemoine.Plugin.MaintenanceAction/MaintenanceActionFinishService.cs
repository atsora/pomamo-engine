// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

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
using Lemoine.Web.Cache;
using Pulse.Web.Cache;

namespace Lemoine.Plugin.MaintenanceAction
{
  class MaintenanceActionFinishService: GenericSyncSaveService<MaintenanceActionFinishRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceActionFinishService).FullName);

    public MaintenanceActionFinishService (Lemoine.Core.Cache.ICacheClient cacheClient)
      : base (cacheClient)
    { }

    public override object GetSync (MaintenanceActionFinishRequestDTO request)
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var dao = new MaintenanceActionDAO ();

        MaintenanceAction maintenanceAction;
        if (request.MachineId.HasValue) {
          maintenanceAction = dao.FindById (request.Id, request.MachineId.Value);
        }
        else {
          maintenanceAction = dao.FindById (request.Id);
        }
        if (null == maintenanceAction) {
          log.ErrorFormat ("Get: " +
                           "unknown maintenance action with ID {0}",
                           request.Id);
          return new ErrorDTO ("No maintenance action with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }

        if (maintenanceAction.Version != request.Version) {
          log.ErrorFormat ("Get: " +
                           "version does not match for maintenance action ID {0}",
                           request.Id);
          return new ErrorDTO ("Stale maintenance action",
                               ErrorStatus.Stale);
        }

        if (maintenanceAction.Status.Equals (MaintenanceActionStatus.Completed)) {
          log.ErrorFormat ("Get: " +
            "the maintenance action was already completed, nothing to do");
          return new ErrorDTO ("Maintenance action already completed",
            ErrorStatus.NotApplicable);
        }

        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Addon.MaintenanceAction.Finish")) {

          // Update maintenanceAction
          maintenanceAction.ModifiedDateTime = DateTime.UtcNow;
          maintenanceAction.Status = MaintenanceActionStatus.Completed;
          maintenanceAction.CompletionDateTime = DateTime.UtcNow;
          dao.MakePersistent (maintenanceAction);

          ClearDomainService.ClearDomain (this.CacheClient, "plugin.maintenanceaction", true);

          // Renew it
          if (request.Renew && maintenanceAction.MaintenanceActionType.Equals (MaintenanceActionType.Preventive)
            && (maintenanceAction.StandardMachiningFrequency.HasValue || maintenanceAction.StandardTotalFrequency.HasValue)) {
            var newMaintenanceAction = new MaintenanceAction (maintenanceAction.Machine,
              maintenanceAction.Title,
              MaintenanceActionType.Preventive);
            newMaintenanceAction.Description = maintenanceAction.Description;
            newMaintenanceAction.StandardMachiningFrequency = maintenanceAction.StandardMachiningFrequency;
            newMaintenanceAction.StandardTotalFrequency = maintenanceAction.StandardTotalFrequency;
            transaction.Commit ();
            return new MaintenanceActionFinishResponseDTO (newMaintenanceAction, true);
          }
          else {
            transaction.Commit ();
            return new MaintenanceActionFinishResponseDTO (maintenanceAction, false);
          }
        } // transaction
      } // session
    }
  }
}
