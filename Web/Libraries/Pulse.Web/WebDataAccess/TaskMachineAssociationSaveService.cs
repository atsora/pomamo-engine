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
using Lemoine.Web;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// TaskMachineAssociationSave Service.
  /// </summary>
  public class TaskMachineAssociationSaveService : GenericSaveService<Pulse.Web.WebDataAccess.TaskMachineAssociationSave>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TaskMachineAssociationSaveService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public TaskMachineAssociationSaveService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync(Pulse.Web.WebDataAccess.TaskMachineAssociationSave request)
    {
      ITaskMachineAssociation taskMachineAssociation;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("TaskMachineAssociationSaveService"))
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
          
          if (request.TaskId.HasValue) {
            ITask task = ModelDAOHelper.DAOFactory.TaskDAO
              .FindById (request.TaskId.Value);
            if (null == task) {
              log.ErrorFormat ("Get: " +
                               "No reason with ID {0}",
                               request.TaskId.Value);
              transaction.Commit ();
              return new ErrorDTO ("No work order with id " + request.TaskId.Value,
                                   ErrorStatus.WrongRequestParameter);
            }
            else {
              taskMachineAssociation = ModelDAOHelper.ModelFactory
                .CreateTaskMachineAssociation (machine, task, range);
            }
          }
          else {
            taskMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateTaskMachineAssociation (machine, null, range);
          }
          if (request.RevisionId.HasValue) {
            if (-1 == request.RevisionId.Value) { // auto-revision
              IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
              revision.Application = "Lem_AspService";
              revision.IPAddress = GetRequestRemoteIp ();
              ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
              taskMachineAssociation.Revision = revision;
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
                taskMachineAssociation.Revision = revision;
              }
            }
          }
          
          ModelDAOHelper.DAOFactory.TaskMachineAssociationDAO
            .MakePersistent (taskMachineAssociation);
          
          transaction.Commit ();
        }
      }
      
      Debug.Assert (null != taskMachineAssociation);
      return new SaveModificationResponseDTO (taskMachineAssociation);
    }
    #endregion // Methods
  }
}
