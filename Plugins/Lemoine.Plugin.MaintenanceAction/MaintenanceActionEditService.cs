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
  class MaintenanceActionEditService: GenericSyncSaveService<MaintenanceActionEditRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceActionEditService).FullName);

    public MaintenanceActionEditService (Lemoine.Core.Cache.ICacheClient cacheClient)
      : base (cacheClient)
    { }

    public override object GetSync (MaintenanceActionEditRequestDTO request)
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

        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Addon.MaintenanceAction.Edit")) {

          maintenanceAction.ModifiedDateTime = DateTime.UtcNow;

          if (!string.IsNullOrEmpty (request.Title)) {
            maintenanceAction.Title = request.Title;
          }
          if (!string.IsNullOrEmpty (request.Description)) {
            if (isRemove (request.Description)) {
              maintenanceAction.Description = "";
            }
            else {
              maintenanceAction.Description = request.Description;
            }
          }

          IFormatProvider provider = CultureInfo.InvariantCulture;
          if (!string.IsNullOrEmpty (request.StopDateTime)) {
            if (isRemove (request.StopDateTime)) {
              maintenanceAction.StopDateTime = null;
            }
            else {
              maintenanceAction.StopDateTime = System.DateTime.Parse (request.StopDateTime, provider,
                DateTimeStyles.AssumeUniversal
                | DateTimeStyles.AdjustToUniversal);
            }
          }
          if (!string.IsNullOrEmpty (request.PlannedDateTime)) {
            if (isRemove (request.PlannedDateTime)) {
              maintenanceAction.PlannedDateTime = null;
            }
            else {
              maintenanceAction.PlannedDateTime = System.DateTime.Parse (request.PlannedDateTime, provider,
                DateTimeStyles.AssumeUniversal
                | DateTimeStyles.AdjustToUniversal);
            }
          }
          if (request.RemainingMachiningDurationSinceCreation.HasValue) {
            if (request.RemainingMachiningDurationSinceCreation.Value == 0) {
              maintenanceAction.RemainingMachiningDuration = null;
            }
            else {
              maintenanceAction.RemainingMachiningDuration = TimeSpan.FromSeconds (request.RemainingMachiningDurationSinceCreation.Value);
            }
          }
          if (request.StandardMachiningFrequency.HasValue) {
            if (request.StandardMachiningFrequency.Value == 0) {
              maintenanceAction.StandardMachiningFrequency = null;
            }
            else {
              maintenanceAction.StandardMachiningFrequency = TimeSpan.FromSeconds (request.StandardMachiningFrequency.Value);
            }
          }
          if (request.StandardTotalFrequency.HasValue) {
            if (request.StandardTotalFrequency.Value == 0) {
              maintenanceAction.StandardTotalFrequency = null;
            }
            else {
              maintenanceAction.StandardTotalFrequency = TimeSpan.FromSeconds (request.StandardTotalFrequency.Value);
            }
          }

          dao.MakePersistent (maintenanceAction);

          transaction.Commit ();

          ClearDomainService.ClearDomain (this.CacheClient, "plugin.maintenanceaction", true);

          return new MaintenanceActionEditResponseDTO (maintenanceAction);
        } // transaction
      } // session
    }

    bool isRemove (string s)
    {
      return string.Equals (s, "remove", StringComparison.InvariantCultureIgnoreCase);
    }
  }
}
