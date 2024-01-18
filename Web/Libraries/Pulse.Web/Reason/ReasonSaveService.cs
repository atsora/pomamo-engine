// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

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

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ReasonSaveService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync (ReasonSaveRequestDTO request)
    {
      IRevision revision;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.ReasonSave.Get")) {
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (request.MachineId);
          if (null == machine) {
            log.ErrorFormat ("Get: " +
                             "Machine with {0} does not exist",
                             request.MachineId);
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
            var user = ModelDAOHelper.DAOFactory.UserDAO
              .FindById (userId.Value);
            if (user is null) {
              log.Error ($"GetSync: user with id {userId} does not exist");
            }
            else {
              revision.Updater = user;
            }
          }
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

          IReason reason = null;
          if (request.ReasonId.HasValue) {
            reason = ModelDAOHelper.DAOFactory.ReasonDAO
              .FindById (request.ReasonId.Value);
            if (null == reason) {
              log.ErrorFormat ("Get: " +
                               "No reason with ID {0}",
                               request.ReasonId.Value);
              transaction.Commit ();
              return new ErrorDTO ("No reason with id " + request.ReasonId.Value,
                                   ErrorStatus.WrongRequestParameter);
            }
          }

          UtcDateTimeRange range = new UtcDateTimeRange (request.Range);
          CreateModification (revision,
                              machine,
                              reason,
                              request.ReasonScore,
                              request.ReasonDetails,
                              range);

          transaction.Commit ();
        }
      }

      Debug.Assert (null != revision);
      return new ReasonSaveResponseDTO (revision);
    }

    void CreateModification (IRevision revision, IMachine machine, IReason reason, double? reasonScore, string details, UtcDateTimeRange range)
    {
      long modificationId;
      if (null != reason) {
        var score = reasonScore ?? 100.0;
        modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertManualReason (machine, range, reason, score, details);
      }
      else { // null == reason
        modificationId = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
          .InsertResetReason (machine, range);
      }

      var modification = ModelDAOHelper.DAOFactory.ReasonMachineAssociationDAO
        .FindById (modificationId, machine);
      revision.AddModification (modification);
    }

#if NSERVICEKIT
    /// <summary>
    /// Response to POST request for ReasonMachineAssociation/Save service
    /// </summary>
    /// <param name="request"></param>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public object Post (ReasonSavePostRequestDTO request,
                        NServiceKit.ServiceHost.IHttpRequest httpRequest)
    {
      IRevision revision;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("Web.ReasonSave.Post"))
        {
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (request.MachineId);
          if (null == machine) {
            log.ErrorFormat ("Get: " +
                             "Machine with {0} does not exist",
                             request.MachineId);
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
            var user = ModelDAOHelper.DAOFactory.UserDAO
              .FindById (userId.Value);
            if (user is null) {
              log.Error ($"GetSync: user with id {userId} does not exist");
            }
            else {
              revision.Updater = user;
            }
          }
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
          
          IReason reason = null;
          if (request.ReasonId.HasValue) {
            reason = ModelDAOHelper.DAOFactory.ReasonDAO
              .FindById (request.ReasonId.Value);
            if (null == reason) {
              log.ErrorFormat ("Get: " +
                               "No reason with ID {0}",
                               request.ReasonId.Value);
              transaction.Commit ();
              return new ErrorDTO ("No reason with id " + request.ReasonId.Value,
                                   ErrorStatus.WrongRequestParameter);
            }
          }

          // Ranges
          RangesPostDTO deserializedResult = PostDTO.Deserialize<RangesPostDTO> (httpRequest);
          foreach (var range in deserializedResult.Convert ()) {
            CreateModification (revision,
                                machine,
                                reason,
                                request.ReasonScore,
                                request.ReasonDetails,
                                range);
          }
          
          transaction.Commit ();
        }
      }
      
      Debug.Assert (null != revision);
      return new ReasonSaveResponseDTO (revision);
    }
#else // !NSERVICEKIT

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
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (request.MachineId);
          if (null == machine) {
            log.ErrorFormat ("Get: " +
                             "Machine with {0} does not exist",
                             request.MachineId);
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
            var user = ModelDAOHelper.DAOFactory.UserDAO
              .FindById (userId.Value);
            if (user is null) {
              log.Error ($"GetSync: user with id {userId} does not exist");
            }
            else {
              revision.Updater = user;
            }
          }
          ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);

          IReason reason = null;
          if (request.ReasonId.HasValue) {
            reason = ModelDAOHelper.DAOFactory.ReasonDAO
              .FindById (request.ReasonId.Value);
            if (null == reason) {
              log.ErrorFormat ("Get: " +
                               "No reason with ID {0}",
                               request.ReasonId.Value);
              transaction.Commit ();
              return new ErrorDTO ("No reason with id " + request.ReasonId.Value,
                                   ErrorStatus.WrongRequestParameter);
            }
          }

          // Ranges
          RangesPostDTO deserializedResult = PostDTO.Deserialize<RangesPostDTO> (m_body);
          foreach (var range in deserializedResult.Convert ()) {
            // TODO: use an asynchronous method
            await Task.Run (() =>
              CreateModification (revision,
                                  machine,
                                  reason,
                                  request.ReasonScore,
                                  request.ReasonDetails,
                                  range)
            );
          }

          transaction.Commit ();
        }
      }

      Debug.Assert (null != revision);
      return new ReasonSaveResponseDTO (revision);
    }
#endif // NSERVICEKIT

    #endregion // Methods

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
