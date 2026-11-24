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
            if (range.IsEmpty () || !range.Lower.HasValue || !range.Upper.HasValue) {
              log.Error ($"Post: range {range} is empty or not bounded");
              return new ErrorDTO ("The range of a scrap report must be bounded", ErrorStatus.WrongRequestParameter);
            }
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

            // The dates of a report never change: an update is a new report with the same range,
            // which replaces the latest version of the report on this range
            // (the version that was displayed may have been replaced since: the latest one is considered)
            var activeReports = (await ModelDAOHelper.DAOFactory.ScrapReportDAO
              .FindOverlapsRangeAsync (machine, range))
              .ToList ();
            IScrapReport updatedReport = null;
            if (0 < deserializedResult.Id) {
              var displayedReport = ModelDAOHelper.DAOFactory.ScrapReportDAO
                .FindById (deserializedResult.Id, machine);
              if (displayedReport is null) {
                log.Error ($"Post: unknown scrap report id {deserializedResult.Id}");
                return new ErrorDTO ($"Unknown scrap report {deserializedResult.Id}", ErrorStatus.WrongRequestParameter);
              }
              if (!object.Equals (displayedReport.DateTimeRange, range)) {
                log.Error ($"Post: range {range} is not the range {displayedReport.DateTimeRange} of the updated report {deserializedResult.Id}");
                return new ErrorDTO ("The range of a scrap report can't be changed", ErrorStatus.WrongRequestParameter);
              }
              updatedReport = activeReports
                .Where (x => object.Equals (x.DateTimeRange, range))
                .OrderByDescending (x => x.Id)
                .FirstOrDefault () ?? displayedReport;
            }
            // A new report must not overlap an existing report, an update must not overlap the reports of other ranges
            var overlappingReport = activeReports
              .FirstOrDefault (x => (updatedReport is null) || !object.Equals (x.DateTimeRange, range));
            if (overlappingReport is not null) {
              log.Error ($"Post: range {range} overlaps the scrap report {overlappingReport.Id} {overlappingReport.DateTimeRange}");
              return new ErrorDTO ("The scrap report overlaps another scrap report", ErrorStatus.WrongRequestParameter);
            }

            // Cycles and parts of the report: same computation as in the ScrapAt service,
            // the completed cycles whose end is in the range
            var cycles = await ScrapReportCycles.FindAsync (machine, range.Lower.Value, range.Upper.Value);
            if (!cycles.Any ()) {
              log.Error ($"Post: no completed cycle in {range}");
              return new ErrorDTO ("No completed cycle in the period of the scrap report", ErrorStatus.WrongRequestParameter);
            }
            var scrapReport = ModelDAOHelper.ModelFactory.CreateScrapReport (operationSlot, range);
            scrapReport.ReportUpdate = updatedReport;
            scrapReport.NbCycles = cycles.Count;
            scrapReport.NbParts = ScrapReportCycles.GetNbParts (operationSlot.Operation, cycles);
            scrapReport.NbValid = deserializedResult.ValidCount;
            scrapReport.Details = deserializedResult.Details;

            // Scrap parts: the reasons of category Scrap
            // Fixable parts: the reasons of category Fixable, plus the optional FixableCount (parts without reason)
            // Set-up parts: the reasons of category Setup, plus the optional SetupCount (parts without reason)
            int nbScrap = 0;
            int nbFixable = deserializedResult.FixableCount;
            int nbSetup = deserializedResult.SetupCount;
            foreach (var r in deserializedResult.Reasons) {
              var nonConformanceReason = await ModelDAOHelper.DAOFactory.NonConformanceReasonDAO
                .FindByIdAsync (r.Id);
              if (nonConformanceReason is null) {
                log.Error ($"Post: unknown non conformance reason id {r.Id}");
                return new ErrorDTO ($"Unknown non conformance reason {r.Id}", ErrorStatus.WrongRequestParameter);
              }
              scrapReport.Reasons.Add (ModelDAOHelper.ModelFactory.CreateScrapReasonReport (scrapReport, nonConformanceReason, r.Number));
              switch (nonConformanceReason.Category) {
                case NonConformanceReasonCategory.Fixable:
                  nbFixable += r.Number;
                  break;
                case NonConformanceReasonCategory.Setup:
                  nbSetup += r.Number;
                  break;
                case NonConformanceReasonCategory.Scrap:
                default:
                  nbScrap += r.Number;
                  break;
              }
            }
            scrapReport.NbScrap = nbScrap;
            scrapReport.NbFixable = nbFixable;
            scrapReport.NbSetup = nbSetup;

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
