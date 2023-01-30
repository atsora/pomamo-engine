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
  class MaintenanceActionCreateService: GenericSyncSaveService<MaintenanceActionCreateRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MaintenanceActionCreateService).FullName);

    public MaintenanceActionCreateService (Lemoine.Core.Cache.ICacheClient cacheClient)
      : base (cacheClient)
    { }

    public override object GetSync (MaintenanceActionCreateRequestDTO request)
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

        MaintenanceActionType maintenanceActionType;
        // TODO: with .NET 4, use Enum.TryParse instead
        try {
          maintenanceActionType = (MaintenanceActionType)Enum.Parse (typeof (MaintenanceActionType), request.Type);
        }
        catch (Exception) {
          log.ErrorFormat ("Get: " +
                           "unknown maintenance action type {0}",
                           request.Type);
          return new ErrorDTO ("Unknown maintenance action type",
                               ErrorStatus.WrongRequestParameter);
        }

        using (IDAOTransaction transaction = session.BeginTransaction ("Web.Addon.MaintenanceAction.Create")) {
          var maintenanceAction = new MaintenanceAction (machine, request.Title, maintenanceActionType);

          IFormatProvider provider = CultureInfo.InvariantCulture;
          if (!string.IsNullOrEmpty (request.StopDateTime)) {
            maintenanceAction.StopDateTime = System.DateTime.Parse (request.StopDateTime, provider,
              DateTimeStyles.AssumeUniversal
              | DateTimeStyles.AdjustToUniversal);
          }
          if (!string.IsNullOrEmpty (request.PlannedDateTime)) {
            maintenanceAction.PlannedDateTime = System.DateTime.Parse (request.PlannedDateTime, provider,
              DateTimeStyles.AssumeUniversal
              | DateTimeStyles.AdjustToUniversal);
          }
          if (request.RemainingMachiningDuration.HasValue) {
            maintenanceAction.RemainingMachiningDuration = TimeSpan.FromSeconds (request.RemainingMachiningDuration.Value);
          }
          if (request.StandardMachiningFrequency.HasValue) {
            maintenanceAction.StandardMachiningFrequency = TimeSpan.FromSeconds (request.StandardMachiningFrequency.Value);
          }
          if (request.StandardTotalFrequency.HasValue) {
            maintenanceAction.StandardTotalFrequency = TimeSpan.FromSeconds (request.StandardTotalFrequency.Value);
          }

          var dao = new MaintenanceActionDAO ();
          dao.MakePersistent (maintenanceAction);

          transaction.Commit ();

          ClearDomainService.ClearDomain (this.CacheClient, "plugin.maintenanceaction", true);

          return new MaintenanceActionCreateResponseDTO (maintenanceAction);
        } // transaction
      } // session
    }

  }
}
