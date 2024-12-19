// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Lemoine.Web.CommonRequestDTO;
using Pulse.Web.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using System.Threading.Tasks;
using Lemoine.Extensions.Web.Interfaces;
using System.IO;
using Microsoft.AspNetCore.Http;
using Lemoine.Web;
using Lemoine.WebMiddleware.HttpContext;
using Pulse.Business.Reason;
using System.Linq;

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

          IReason reason = null;
          if (request.ReasonId.HasValue) {
            reason = await ModelDAOHelper.DAOFactory.ReasonDAO
              .FindByIdAsync (request.ReasonId.Value);
            if (reason is null) {
              log.Error ($"Get: No reason with ID {request.ReasonId.Value}");
              transaction.Commit ();
              return new ErrorDTO ("No reason with id " + request.ReasonId.Value,
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
          UtcDateTimeRange range = new UtcDateTimeRange (request.Range);
          CreateModification (revision,
                              machine,
                              reason,
                              request.ReasonScore,
                              request.ReasonDetails,
                              range, jsonData);

          transaction.Commit ();
        }
      }

      Debug.Assert (null != revision);
      return new ReasonSaveResponseDTO (revision);
    }

    void CreateModification (IRevision revision, IMachine machine, IReason reason, double? reasonScore, string details, UtcDateTimeRange range, string jsonData)
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
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

          IReason reason = null;
          if (request.ReasonId.HasValue) {
            reason = await ModelDAOHelper.DAOFactory.ReasonDAO
              .FindByIdAsync (request.ReasonId.Value);
            if (null == reason) {
              log.Error ($"Get: No reason with ID {request.ReasonId.Value}");
              transaction.Commit ();
              return new ErrorDTO ("No reason with id " + request.ReasonId.Value,
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
              CreateModification (revision,
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
