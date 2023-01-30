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
  public class SaveNonConformanceReportService
    : GenericSaveService<Lemoine.DTO.SaveNonConformanceReport>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (SaveNonConformanceReportService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public SaveNonConformanceReportService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync (Lemoine.DTO.SaveNonConformanceReport request)
    {
      int deliverablePieceId = request.DeliverablePieceId;
      int machineId = request.MachineId;
      int? intermediateWorkPieceId = request.IntermediateWorkPieceId;
      int? nonConformanceReasonId = request.NonConformanceReasonId;
      string details = request.NonConformanceDetails;
      bool? fixable = request.Fixable;
      string operationDateTimeStr = request.OperationDateTime;
      DateTime dateOfRequest = DateTime.UtcNow;

      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {

        IDeliverablePiece deliverablePiece = ModelDAOHelper.DAOFactory.DeliverablePieceDAO.FindById (deliverablePieceId);
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (machineId);
        IMonitoredMachine monitoredMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (machineId);
        INonConformanceReport nonConformanceReport = ModelDAOHelper.ModelFactory.CreateNonConformanceReport (deliverablePiece, machine);

        if (nonConformanceReasonId.HasValue) {
          nonConformanceReport.NonConformanceReason = ModelDAOHelper.DAOFactory.NonConformanceReasonDAO.FindById (nonConformanceReasonId.Value);
        }

        if (intermediateWorkPieceId.HasValue) {
          nonConformanceReport.IntermediateWorkPiece = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindById (intermediateWorkPieceId.Value);
        }

        if (fixable.HasValue) {
          nonConformanceReport.NonConformanceFixable = fixable.Value;
        }

        DateTime? operationDateTime = Lemoine.DTO.ConvertDTO.IsoStringToDateTimeUtc (operationDateTimeStr);
        if (operationDateTime.HasValue) {
          nonConformanceReport.NonConformanceOperationDateTime = operationDateTime.Value;
        }
        nonConformanceReport.NonConformanceDetails = details;

        IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
        revision.AddModification (nonConformanceReport);
        revision.DateTime = dateOfRequest;
        revision.IPAddress = GetRequestRemoteIp ();

        IOperationCycle operationCycle = ModelDAOHelper.DAOFactory.OperationCycleDAO.FindWithEndEqualTo (monitoredMachine, operationDateTime.Value);
        IOperationCycleDeliverablePiece operationCycleDeliverablePiece = ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.FindWithOperationCycleDeliverablePiece (operationCycle, deliverablePiece);
        operationCycleDeliverablePiece.NonConformanceReason = nonConformanceReport.NonConformanceReason;

        ModelDAOHelper.DAOFactory.OperationCycleDeliverablePieceDAO.MakePersistent (operationCycleDeliverablePiece);
        ModelDAOHelper.DAOFactory.NonConformanceReportDAO.MakePersistent (nonConformanceReport);
        ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

        transaction.Commit ();

        int revId = revision.Id;
        return ServiceHelper.ResponseOkDTO (revId, "Save nonconformance report successful");
      }
    }
    #endregion // Methods
  }
}
