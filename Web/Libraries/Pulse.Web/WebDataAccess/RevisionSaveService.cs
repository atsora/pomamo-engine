// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Pulse.Web.WebDataAccess.CommonResponseDTO;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// RevisionSave Service.
  /// </summary>
  public class RevisionSaveService : GenericSaveService<Pulse.Web.WebDataAccess.RevisionSave>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RevisionSaveService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public RevisionSaveService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync(Pulse.Web.WebDataAccess.RevisionSave request)
    {
      IRevision revision;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("RevisionSaveService"))
        {
          revision = ModelDAOHelper.ModelFactory.CreateRevision ();
          if (!string.IsNullOrEmpty (request.Application)) {
            revision.Application = request.Application;
          }
          if (!string.IsNullOrEmpty (request.IPAddress)) {
            revision.IPAddress = request.IPAddress;
          }
          else {
            revision.IPAddress = GetRequestRemoteIp ();
          }
          if (!string.IsNullOrEmpty (request.Comment)) {
            revision.Comment = request.Comment;
          }
          if (request.UserId.HasValue) {
            IUpdater user = ModelDAOHelper.DAOFactory.UserDAO
              .FindById (request.UserId.Value);
            if (null == user) {
              log.WarnFormat ("Get: " +
                              "user with ID {0} does not exist",
                              request.UserId.Value);
            }
            else {
              revision.Updater = user;
            }
          }
          else if (request.ServiceId.HasValue) {
            IUpdater service = ModelDAOHelper.DAOFactory.ServiceDAO
              .FindById (request.ServiceId.Value);
            if (null == service) {
              log.WarnFormat ("Get: " +
                              "service with ID {0} does not exist",
                              request.ServiceId.Value);
            }
            else {
              revision.Updater = service;
            }
          }
          
          ModelDAOHelper.DAOFactory.RevisionDAO
            .MakePersistent (revision);
          
          transaction.Commit ();
        }
      }
      
      Debug.Assert (null != revision);
      return new SaveResponseDTO (revision.Id);
    }
    #endregion // Methods
  }
}
