// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Model;
using Lemoine.ModelDAO;

using Lemoine.Core.Log;
using Pulse.Web.CommonResponseDTO;
using Pulse.Web.WebDataAccess.CommonResponseDTO;
using Lemoine.Extensions.Web.Responses;
using Microsoft.AspNetCore.Http;
using Lemoine.Extensions.Web.Interfaces;
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// MachineStateTemplateMachineAssociationSave Service.
  /// </summary>
  public class MachineStateTemplateMachineAssociationSaveService
    : GenericSaveService<Pulse.Web.WebDataAccess.MachineStateTemplateMachineAssociationSave>
    , IHttpContextSupport
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateMachineAssociationSaveService).FullName);

    /// <summary>
    /// <see cref="IHttpContextSupport"/>
    /// </summary>
    public HttpContext HttpContext { get; set; }
    
    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public MachineStateTemplateMachineAssociationSaveService ()
    {
    }
#endregion // Constructors

#region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync(Pulse.Web.WebDataAccess.MachineStateTemplateMachineAssociationSave request)
    {
      IMachineStateTemplateAssociation machineStateTemplateAssociation;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("MachineStateTemplateMachineAssociationSaveService"))
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
          
          UtcDateTimeRange range = new UtcDateTimeRange (request.Range);
          
          IMachineStateTemplate machineStateTemplate = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
            .FindById (request.MachineStateTemplateId);
          if (null == machineStateTemplate) {
            log.ErrorFormat ("Get: " +
                             "No reason with ID {0}",
                             request.MachineStateTemplateId);
            transaction.Commit ();
            return new ErrorDTO ("No machine state template with id " + request.MachineStateTemplateId,
                                 ErrorStatus.WrongRequestParameter);
          }
          else {
            machineStateTemplateAssociation = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine, machineStateTemplate, range);
            machineStateTemplateAssociation.Option = AssociationOption.Synchronous;
          }
          if (request.UserId.HasValue) {
            IUser user = ModelDAOHelper.DAOFactory.UserDAO
              .FindById (request.UserId.Value);
            if (null == user) {
              log.ErrorFormat ("Get: " +
                               "no user with ID {0}",
                               request.UserId);
              transaction.Commit ();
              return new ErrorDTO ("No user with the specified ID",
                                   ErrorStatus.WrongRequestParameter);
            }
            else {
              machineStateTemplateAssociation.User = user;
            }
          }
          if (request.ShiftId.HasValue) {
            IShift shift = ModelDAOHelper.DAOFactory.ShiftDAO
              .FindById (request.ShiftId.Value);
            if (null == shift) {
              log.ErrorFormat ("Get: " +
                               "no shift with ID {0}",
                               request.ShiftId);
              transaction.Commit ();
              return new ErrorDTO ("No shift with the specified ID",
                                   ErrorStatus.WrongRequestParameter);
            }
            else {
              machineStateTemplateAssociation.Shift = shift;
            }
          }
          if (request.Force.HasValue) {
            machineStateTemplateAssociation.Force = request.Force.Value;
          }
          if (request.RevisionId.HasValue) {
            if (-1 == request.RevisionId.Value) { // auto-revision
              IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
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
              machineStateTemplateAssociation.Revision = revision;
            }
            else {
              IRevision revision = ModelDAOHelper.DAOFactory.RevisionDAO
                .FindById (request.RevisionId.Value);
              if (null == revision) {
                log.WarnFormat ("Get: " +
                                "No revision with ID {0}",
                                request.RevisionId.Value);
              }
              else {
                machineStateTemplateAssociation.Revision = revision;
              }
            }
          }
          
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (machineStateTemplateAssociation);
          
          transaction.Commit ();
        }
      }
      
      Debug.Assert (null != machineStateTemplateAssociation);
      return new SaveModificationResponseDTO (machineStateTemplateAssociation);
    }
#endregion // Methods
  }
}
