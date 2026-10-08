// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Web;
using Pulse.Web.CommonResponseDTO;
using Pulse.Web.WebDataAccess.CommonResponseDTO;

namespace Pulse.Web.WebDataAccess
{
  /// <summary>
  /// OperationMachineAssociationSave Service
  /// </summary>
  public class OperationMachineAssociationSaveService : GenericSaveService<OperationMachineAssociationSave>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationMachineAssociationSaveService).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationMachineAssociationSaveService ()
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync (OperationMachineAssociationSave request)
    {
      IOperationMachineAssociation operationMachineAssociation;

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (IDAOTransaction transaction = session.BeginTransaction ("OperationMachineAssociationSaveService")) {
          var machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (request.MachineId);
          if (machine is null) {
            log.Error ($"GetSync: machine with id {request.MachineId} does not exist");
            transaction.Commit ();
            return new ErrorDTO ($"No machine with id {request.MachineId}",
                                 ErrorStatus.WrongRequestParameter);
          }

          var range = new UtcDateTimeRange (request.Range);

          IOperation operation = null;
          if (request.OperationId.HasValue) {
            operation = ModelDAOHelper.DAOFactory.OperationDAO
              .FindById (request.OperationId.Value);
            if (operation is null) {
              log.Error ($"GetSync: no operation with id {request.OperationId.Value}");
              transaction.Commit ();
              return new ErrorDTO ($"No operation with id {request.OperationId.Value}",
                                   ErrorStatus.WrongRequestParameter);
            }
          }

          operationMachineAssociation = ModelDAOHelper.ModelFactory
            .CreateOperationMachineAssociation (machine, range);
          operationMachineAssociation.Operation = operation;

          if (request.RevisionId.HasValue) {
            if (-1 == request.RevisionId.Value) { // auto-revision
              var revision = ModelDAOHelper.ModelFactory.CreateRevision ();
              revision.Application = "AspService";
              revision.IPAddress = GetRequestRemoteIp ();
              ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
              operationMachineAssociation.Revision = revision;
            }
            else {
              var revision = ModelDAOHelper.DAOFactory.RevisionDAO
                .FindById (request.RevisionId.Value);
              if (revision is null) {
                log.Warn ($"GetSync: no revision with id {request.RevisionId.Value}");
              }
              else {
                operationMachineAssociation.Revision = revision;
              }
            }
          }

          ModelDAOHelper.DAOFactory.OperationMachineAssociationDAO
            .MakePersistent (operationMachineAssociation);

          transaction.Commit ();
        }
      }

      return new SaveModificationResponseDTO (operationMachineAssociation);
    }
  }
}
