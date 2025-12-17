// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
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
using Pulse.Business.Reason;
using Pulse.Web.CommonResponseDTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pulse.Web.Reason
{
  /// <summary>
  /// ReasonSave Service.
  /// </summary>
  public class ReasonSaveService
    : GenericSaveService<ReasonSaveRequestDTO>
    , IHttpContextSupport
    , IBodySupport
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ReasonSaveService).FullName);

    Stream m_body;

    /// <summary>
    /// <see cref="IHttpContextSupport"/>
    /// </summary>
    public HttpContext HttpContext { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ReasonSaveService ()
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override async Task<object> Get (ReasonSaveRequestDTO request)
    {
      IRevision revision;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.ReasonSave.Get")) {
          IMachine machine = await ModelDAOHelper.DAOFactory.MachineDAO
            .FindByIdAsync (request.MachineId);
          if (machine is null) {
            log.Error ($"Get: Machine with {request.MachineId} does not exist");
            transaction.Commit ();
            return new ErrorDTO ("No machine with id " + request.MachineId,
                                 ErrorStatus.WrongRequestParameter);
          }

          // Auto-revision by default
          revision = ModelDAOHelper.ModelFactory.CreateRevision ();
          revision.Application = "Lem_AspService";
          revision.IPAddress = GetRequestRemoteIp ();
          var userId = this.GetAuthenticatedUserId ();
          if (userId.HasValue) {
            var user = await ModelDAOHelper.DAOFactory.UserDAO
              .FindByIdAsync (userId.Value);
            if (user is null) {
              log.Error ($"Get: user with id {userId} does not exist");
            }
            else {
              revision.Updater = user;
            }
          }
          await ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistentAsync (revision);

          var range = new UtcDateTimeRange (request.Range);
          if (!string.IsNullOrEmpty (request.ClassificationId) && request.ClassificationId.StartsWith ("MST", StringComparison.CurrentCultureIgnoreCase)) {
            if (int.TryParse (request.ClassificationId.Substring ("MST".Length), out var machineStateTemplateId)) {
              var machineStateTemplate = await ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                .FindByIdAsync (machineStateTemplateId);
              if (machineStateTemplate is null) {
                log.Error ($"Get: No machine state template with ID {machineStateTemplateId}");
                transaction.Commit ();
                return new ErrorDTO ("No machine state template with id " + machineStateTemplateId,
                                     ErrorStatus.WrongRequestParameter);
              }
              else {
                CreateMachineStateTemplateModification (revision, machine, machineStateTemplate, range);
              }
            }
            else {
              log.Error ($"Get: invalid classification ID {request.ClassificationId}");
              transaction.Commit ();
              return new ErrorDTO ("Invalid classification ID" + request.ClassificationId,
                                   ErrorStatus.WrongRequestParameter);
            }
          }
          else {
            int reasonId = 0;
            if (request.ReasonId.HasValue) {
              reasonId = request.ReasonId.Value;
            }
            else if (!string.IsNullOrEmpty (request.ClassificationId)) {
              if (!int.TryParse (request.ClassificationId, out reasonId)) {
                log.Error ($"Get: invalid reason ID {request.ClassificationId}");
                transaction.Commit ();
                return new ErrorDTO ("Invalid reason ID" + request.ClassificationId,
                                     ErrorStatus.WrongRequestParameter);
              }
            }
            IReason reason = null;
            if (0 != reasonId) {
              reason = await ModelDAOHelper.DAOFactory.ReasonDAO
                .FindByIdAsync (reasonId);
              if (reason is null) {
                log.Error ($"Get: No reason with ID {reasonId}");
                transaction.Commit ();
                return new ErrorDTO ("No reason with id " + reasonId,
                                     ErrorStatus.WrongRequestParameter);
              }
            }

            string jsonData;
            if (string.IsNullOrEmpty (request.ReasonDataKey)) {
              jsonData = null;
            }
            else {
              var reasonDataValue = ((JsonElement)request.ReasonDataValue).GetRawText ();
              jsonData = $$"""
              {
                "{{request.ReasonDataKey}}": {{reasonDataValue}}
              }
              """;
            }
            CreateReasonModification (revision,
                                machine,
                                reason,
                                request.ReasonScore,
                                request.ReasonDetails,
                                range, jsonData);
          }

          transaction.Commit ();
        }
      }

      Debug.Assert (null != revision);
      return new ReasonSaveResponseDTO (revision);
    }

    void CreateMachineStateTemplateModification (IRevision revision, IMachine machine, IMachineStateTemplate machineStateTemplate, UtcDateTimeRange range)
    {
      var modificationId = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
        .Insert (machine, range, machineStateTemplate);

      var modification = ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
        .FindById (modificationId, machine);
      revision.AddModification (modification);
    }

    void CreateReasonModification (IRevision revision, IMachine machine, IReason reason, double? reasonScore, string details, UtcDateTimeRange range, string jsonData)
    {
      long modificationId;
      if (null != reason) {
        var score = reasonScore ?? 100.0;
        modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertManualReason (machine, range, reason, score, details, jsonData);
      }
      else { // null == reason
        modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertResetReason (machine, range);
      }

      var modification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
        .FindById (modificationId, machine);
      revision.AddModification (modification);
    }

    /// <summary>
    /// Post method
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<object> Post (ReasonSavePostRequestDTO request)
    {
      IRevision revision;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.ReasonSave.Post")) {
          var machine = await ModelDAOHelper.DAOFactory.MachineDAO
            .FindByIdAsync (request.MachineId);
          if (null == machine) {
            log.Error ($"Get: Machine with {request.MachineId} does not exist");
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
              log.Error ($"Get: user with id {userId} does not exist");
            }
            else {
              revision.Updater = user;
            }
          }
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

          if (!string.IsNullOrEmpty (request.ClassificationId) && request.ClassificationId.StartsWith ("MST", StringComparison.CurrentCultureIgnoreCase)) {
            if (int.TryParse (request.ClassificationId.Substring ("MST".Length), out var machineStateTemplateId)) {
              var machineStateTemplate = await ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
                .FindByIdAsync (machineStateTemplateId);
              if (machineStateTemplate is null) {
                log.Error ($"Get: No machine state template with ID {machineStateTemplateId}");
                transaction.Commit ();
                return new ErrorDTO ("No machine state template with id " + machineStateTemplateId,
                                     ErrorStatus.WrongRequestParameter);
              }
              else {
                try {
                  var deserializedResult = PostDTO.Deserialize<ReasonSavePostDTO> (m_body);
                  foreach (var range in deserializedResult.ExtractRanges ()) {
                    CreateMachineStateTemplateModification (revision, machine, machineStateTemplate, range);
                  }
                }
                catch (Exception ex) {
                  log.Error ($"Post: exception in deserialization or modification", ex);
                  throw;
                }
              }
            }
            else {
              log.Error ($"Get: invalid classification ID {request.ClassificationId}");
              transaction.Commit ();
              return new ErrorDTO ("Invalid classification ID" + request.ClassificationId,
                                   ErrorStatus.WrongRequestParameter);
            }
          }
          else {
            int reasonId = 0;
            if (request.ReasonId.HasValue) {
              reasonId = request.ReasonId.Value;
            }
            else if (!string.IsNullOrEmpty (request.ClassificationId)) {
              if (!int.TryParse (request.ClassificationId, out reasonId)) {
                log.Error ($"Get: invalid reason ID {request.ClassificationId}");
                transaction.Commit ();
                return new ErrorDTO ("Invalid reason ID" + request.ClassificationId,
                                     ErrorStatus.WrongRequestParameter);
              }
            }
            IReason reason = null;
            if (0 != reasonId) {
              reason = await ModelDAOHelper.DAOFactory.ReasonDAO
                .FindByIdAsync (reasonId);
              if (reason is null) {
                log.Error ($"Get: No reason with ID {reasonId}");
                transaction.Commit ();
                return new ErrorDTO ("No reason with id " + reasonId,
                                     ErrorStatus.WrongRequestParameter);
              }
            }

            // Ranges + jsonData
            try {
              var deserializedResult = PostDTO.Deserialize<ReasonSavePostDTO> (m_body);
              string jsonData;
              if (deserializedResult.ReasonData is null) {
                jsonData = null;
              }
              else {
                jsonData = deserializedResult.ReasonData.Value.GetRawText ();
              }
              foreach (var range in deserializedResult.ExtractRanges ()) {
                CreateReasonModification (revision,
                                    machine,
                                    reason,
                                    request.ReasonScore,
                                    request.ReasonDetails,
                                    range, jsonData);
              }
            }
            catch (Exception ex) {
              log.Error ($"Post: exception in deserialization or modification", ex);
              throw;
            }
          }

          transaction.Commit ();
        }
      }

      Debug.Assert (null != revision);
      return new ReasonSaveResponseDTO (revision);
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
