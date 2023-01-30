// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Extensions.Web.Interfaces;
using System.IO;
using Lemoine.DTO;

namespace Lemoine.WebService
{
  /// <summary>
  /// ReasonSave Service.
  /// </summary>
  public class SaveMachineObservationStateV2Service
    : GenericSaveService<Lemoine.DTO.SaveMachineObservationStateV2>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SaveMachineObservationStateV2Service).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public SaveMachineObservationStateV2Service ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync (Lemoine.DTO.SaveMachineObservationStateV2 request)
    {
      int machineId = request.Id;
      int machineObservationStateId = request.MachineObservationStateId;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (machineId);

        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO (machineId);
        }

        IMachineObservationState machineObservationState = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById (machineObservationStateId);

        if (machineObservationState == null) {
          return ServiceHelper.NoMachineObservationStateWithIdErrorDTO (machineObservationStateId);
        }

        UtcDateTimeRange range = Lemoine.DTO.ConvertDTO.IsoStringToUtcDateTimeRange (request.Begin, request.End);
        if (range.IsEmpty ()) { // bad range
          return ServiceHelper.BadDateTimeRange (range);
        }

        IMachineObservationStateAssociation newMachineObservationStateAssociation =
          ModelDAOHelper.ModelFactory.
          CreateMachineObservationStateAssociation (machine, machineObservationState, range);

        ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (newMachineObservationStateAssociation);

        IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
        revision.DateTime = DateTime.UtcNow;
        revision.AddModification (newMachineObservationStateAssociation);
        revision.Updater = null;
        revision.IPAddress = GetRequestRemoteIp ();
        ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

        transaction.Commit ();
        return ServiceHelper.ResponseOkDTO (revision.Id, "Save machine observation state successful");
      }
    }
    #endregion // Methods
  }
}
