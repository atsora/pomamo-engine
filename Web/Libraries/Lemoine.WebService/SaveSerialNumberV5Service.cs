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
  public class SaveSerialNumberV5Service
    : GenericSaveService<Lemoine.DTO.SaveSerialNumberV5>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SaveSerialNumberV5Service).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public SaveSerialNumberV5Service ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync (Lemoine.DTO.SaveSerialNumberV5 request)
    {
      int machineId = request.MachineId;
      string serialNumber = request.SerialNumber;
      DateTime dateTime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc (request.DateTime).Value.ToUniversalTime ();
      bool isBegin = request.IsBegin;
      DateTime dateOfRequest = DateTime.UtcNow;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (machineId);

        if (machine == null) {
          return ServiceHelper.NoMonitoredMachineWithIdErrorDTO (machineId);
        }

        ISerialNumberModification serialModification =
          ModelDAOHelper.ModelFactory.CreateSerialNumberModification (machine, serialNumber,
                                                                     dateTime, isBegin, dateOfRequest);


        IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
        revision.AddModification (serialModification);
        revision.DateTime = dateOfRequest;
        revision.IPAddress = GetRequestRemoteIp ();

        ModelDAOHelper.DAOFactory.SerialNumberModificationDAO.MakePersistent (serialModification);
        ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

        transaction.Commit ();

        int revId = revision.Id;
        return ServiceHelper.ResponseOkDTO (revId, "Save serial number successful");
      }
    }
    #endregion // Methods
  }
}
