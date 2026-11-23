// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Lemoine.Web.CommonRequestDTO;
using Lemoine.WebMiddleware.HttpContext;
using Microsoft.AspNetCore.Http;
using Pulse.Web.CommonResponseDTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pulse.Web.Scrap
{
  /// <summary>
  /// ReasonSave Service.
  /// </summary>
  public class ScrapSaveService
    : GenericSaveService<ScrapSaveRequestDTO>
    , IHttpContextSupport
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ScrapSaveService).FullName);

    Stream m_body;

    /// <summary>
    /// <see cref="IHttpContextSupport"/>
    /// </summary>
    public HttpContext HttpContext { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ScrapSaveService ()
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync (ScrapSaveRequestDTO request)
    {
      log.Fatal ($"GetSync: GET request, POST must be used instead");
      return new ErrorDTO ("Invalid GET request, use a POST request", ErrorStatus.WrongRequestParameter);
    }

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post (ScrapSavePostRequestDTO request)
    {
      IRevision revision;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.ReasonSave.Post")) {
          var machine = await ModelDAOHelper.DAOFactory.MachineDAO
            .FindByIdAsync (request.MachineId);
          if (null == machine) {
            log.Error ($"Post: Machine with {request.MachineId} does not exist");
            transaction.Commit ();
            return new ErrorDTO ("No machine with id " + request.MachineId,
                                 ErrorStatus.WrongRequestParameter);
          }

          // Auto-revision by default
          revision = ModelDAOHelper.ModelFactory.CreateRevision ();
          revision.Application = "AspService";
          revision.IPAddress = GetRequestRemoteIp ();
          var userId = this.GetAuthenticatedUserId ();
          if (userId.HasValue) {
            var user = await ModelDAOHelper.DAOFactory.UserDAO
              .FindByIdAsync (userId.Value);
            if (user is null) {
              log.Error ($"Post: user with id {userId} does not exist");
            }
            else {
              revision.Updater = user;
            }
          }
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

          try {
            var deserializedResult = PostDTO.Deserialize<ScrapSavePostDTO> (m_body);

            var range = new UtcDateTimeRange (deserializedResult.Range);
            var operationSlots = await ModelDAOHelper.DAOFactory.OperationSlotDAO
              .FindOverlapsRangeAsync (machine, range);
            if (1 < operationSlots.Count) {
              log.Error ($"Post: more than 1 operation slot in {range}");
              return new ErrorDTO ("Several operation slots for scrap report", ErrorStatus.WrongRequestParameter);
            }
            if (!operationSlots.Any ()) {
              log.Error ($"Post: no operation slot in {range}");
              return new ErrorDTO ("No operation slots for scrap report", ErrorStatus.WrongRequestParameter);
            }
            var operationSlot = operationSlots.Single ();
            if (!operationSlot.DateTimeRange.ContainsRange (range)) {
              log.Error ($"Post: range {range} not included in the operation slot {operationSlot.DateTimeRange}");
              return new ErrorDTO ("Scrap report not included in a single operation slot", ErrorStatus.WrongRequestParameter);
            }

            var scrapReport = ModelDAOHelper.ModelFactory.CreateScrapReport (operationSlot, range);
            if (0 < deserializedResult.Id) {
              var updateScapReport = ModelDAOHelper.DAOFactory.ScrapReportDAO
                .FindById (deserializedResult.Id, machine);
              scrapReport.ReportUpdate = updateScapReport;
            }
            // Cycles and parts of the report: same computation as in the ScrapAt service
            var cycles = await ModelDAOHelper.DAOFactory.OperationCycleDAO
              .FindOverlapsRangeAsync (machine, range);
            var adjustedCycles = cycles.Where (x => x.Quantity.HasValue).ToList ();
            scrapReport.NbCycles = cycles.Count ();
            scrapReport.NbParts = operationSlot.Operation.GetTotalNumberOfIntermediateWorkPieces () * (scrapReport.NbCycles - adjustedCycles.Count)
              + adjustedCycles.Sum (x => x.Quantity.Value);
            scrapReport.NbValid = deserializedResult.ValidCount;
            scrapReport.NbSetup = deserializedResult.SetupCount;
            scrapReport.NbFixable = deserializedResult.FixableCount;
            scrapReport.Details = deserializedResult.Details;

            foreach (var r in deserializedResult.Reasons) {
              var nonConformanceReason = await ModelDAOHelper.DAOFactory.NonConformanceReasonDAO
                .FindByIdAsync (r.Id);
              if (nonConformanceReason is null) {
                log.Error ($"Post: unknown non conformance reason id {r.Id}");
                return new ErrorDTO ($"Unknown non conformance reason {r.Id}", ErrorStatus.WrongRequestParameter);
              }
              scrapReport.Reasons.Add (ModelDAOHelper.ModelFactory.CreateScrapReasonReport (scrapReport, nonConformanceReason, r.Number));
            }
            scrapReport.NbScrap = deserializedResult.Reasons.Sum (r => r.Number);

            // Note: the reasons are saved with the report (cascade)
            await ModelDAOHelper.DAOFactory.ScrapReportDAO.MakePersistentAsync (scrapReport);

            revision.AddModification (scrapReport);
          }
          catch (Exception ex) {
            log.Error ($"Post: exception in deserialization or modification", ex);
            throw;
          }

          transaction.Commit ();
        }
      }

      Debug.Assert (null != revision);
      return new ScrapSaveResponseDTO (revision);
    }

    #region IBodySupport
    /// <summary>
    /// <see cref="IBodySupport"/>
    /// </summary>
    /// <param name="body"></param>
    public void SetBody (Stream body)
    {
      m_body = body;
    }
    #endregion // IBodySupport
  }
}
