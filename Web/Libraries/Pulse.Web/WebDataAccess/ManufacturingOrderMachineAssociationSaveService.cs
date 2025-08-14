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
  /// ManufacturingOrderMachineAssociationSave Service.
  /// </summary>
  public class ManufacturingOrderMachineAssociationSaveService : GenericSaveService<Pulse.Web.WebDataAccess.ManufacturingOrderMachineAssociationSave>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ManufacturingOrderMachineAssociationSaveService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public ManufacturingOrderMachineAssociationSaveService ()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetSync(Pulse.Web.WebDataAccess.ManufacturingOrderMachineAssociationSave request)
    {
      IManufacturingOrderMachineAssociation manufacturingOrderMachineAssociation;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ("ManufacturingOrderMachineAssociationSaveService"))
        {
          IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
            .FindById (request.MachineId);
          if (null == machine) {
            log.Error ($"Get: Machine with {request.MachineId} does not exist");
            transaction.Commit ();
            return new ErrorDTO ("No machine with id " + request.MachineId,
                                 ErrorStatus.WrongRequestParameter);
          }
          
          UtcDateTimeRange range = new UtcDateTimeRange (request.Range);
          
          if (request.ManufacturingOrderId.HasValue) {
            IManufacturingOrder manufacturingOrder = ModelDAOHelper.DAOFactory.ManufacturingOrderDAO
              .FindById (request.ManufacturingOrderId.Value);
            if (null == manufacturingOrder) {
              log.ErrorFormat ("Get: " +
                               "No reason with ID {0}",
                               request.ManufacturingOrderId.Value);
              transaction.Commit ();
              return new ErrorDTO ("No work order with id " + request.ManufacturingOrderId.Value,
                                   ErrorStatus.WrongRequestParameter);
            }
            else {
              manufacturingOrderMachineAssociation = ModelDAOHelper.ModelFactory
                .CreateManufacturingOrderMachineAssociation (machine, manufacturingOrder, range);
            }
          }
          else {
            manufacturingOrderMachineAssociation = ModelDAOHelper.ModelFactory
              .CreateManufacturingOrderMachineAssociation (machine, null, range);
          }
          if (request.RevisionId.HasValue) {
            if (-1 == request.RevisionId.Value) { // auto-revision
              IRevision revision = ModelDAOHelper.ModelFactory.CreateRevision ();
              revision.Application = "Lem_AspService";
              revision.IPAddress = GetRequestRemoteIp ();
              ModelDAOHelper.DAOFactory.RevisionDAO.MakePersistent (revision);
              manufacturingOrderMachineAssociation.Revision = revision;
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
                manufacturingOrderMachineAssociation.Revision = revision;
              }
            }
          }
          
          ModelDAOHelper.DAOFactory.ManufacturingOrderMachineAssociationDAO
            .MakePersistent (manufacturingOrderMachineAssociation);
          
          transaction.Commit ();
        }
      }
      
      Debug.Assert (null != manufacturingOrderMachineAssociation);
      return new SaveModificationResponseDTO (manufacturingOrderMachineAssociation);
    }
    #endregion // Methods
  }
}
