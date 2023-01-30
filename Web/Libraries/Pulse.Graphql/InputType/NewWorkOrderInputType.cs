// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Pulse.Graphql.Type;

namespace Pulse.Graphql.InputType
{
  /// <summary>
  /// Class to create a <see cref="IWorkOrder"/>
  /// </summary>
  public class NewWorkOrder
  {
    readonly ILog log = LogManager.GetLogger<NewWorkOrder> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public NewWorkOrder () { }

    /// <summary>
    /// Name
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// External code
    /// </summary>
    public string? ExternalCode { get; set; }

    /// <summary>
    /// Document link
    /// </summary>
    public string? DocumentLink { get; set; }

    /// <summary>
    /// Id of associated <see cref="ICustomer"/>
    /// </summary>
    public int? CustomerId { get; set; }

    /// <summary>
    /// Delivery date
    /// </summary>
    public DateTime? DeliveryDate { get; set; }

    /// <summary>
    /// Id of associated <see cref="IWorkOrderStatus"/>
    /// </summary>
    public int? StatusId { get; set; }

    /// <summary>
    /// Id of associated <see cref="IProject"/>
    /// </summary>
    public int? ProjectId { get; set; }

    /// <summary>
    /// Quantity for the associated <see cref="IProject"/>
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Create a new work order
    /// </summary>
    /// <returns></returns>
    public IWorkOrder CreateWorkOrder ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("CreateWorkOrder")) {
          var workOrderStatusId = this.StatusId ?? 1; // 1: Undefined
          var workOrderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO
            .FindById (workOrderStatusId);
          if (workOrderStatus is null) {
            log.Error ($"CreateWorkOrder: work order status with Id={workOrderStatusId} does not exist");
            throw new InvalidOperationException ("Invalid work order status");
          }
          IWorkOrder workOrder;
          if (!string.IsNullOrEmpty (this.Name)) {
            workOrder = ModelDAOHelper.ModelFactory.CreateWorkOrder (workOrderStatus, this.Name);
            workOrder.Code = this.Code;
          }
          else if (!string.IsNullOrEmpty (this.Code)) {
            workOrder = ModelDAOHelper.ModelFactory.CreateWorkOrderFromCode (workOrderStatus, this.Code);
          }
          else {
            log.Error ($"CreateWorkOrder: name or code is mandatory");
            throw new InvalidOperationException ("name or code is mandatory in a work order");
          }
          workOrder.ExternalCode = this.ExternalCode;
          workOrder.DocumentLink = this.DocumentLink;
          if (this.CustomerId is not null) {
            var customer = ModelDAOHelper.DAOFactory.CustomerDAO
              .FindById (this.CustomerId.Value);
            if (customer is null) {
              log.Error ($"CreateWorkOrder: the customer with id {this.CustomerId} does not exist");
              throw new InvalidOperationException ("invalid customer id");
            }
          }
          workOrder.DeliveryDate = this.DeliveryDate;
          IProject? project = null;
          if (this.ProjectId is not null) {
            project = ModelDAOHelper.DAOFactory.ProjectDAO
              .FindById (this.ProjectId.Value);
            if (project is null) {
              log.Error ($"CreateWorkOrder: the project with id {this.ProjectId} does not exist");
              throw new InvalidOperationException ("invalid project id");
            }
            project.AddWorkOrder (workOrder);
          }
          else if (this.Quantity.HasValue) {
            log.Warn ($"CreateWorkOrder: the quantity is set but not the project id, omit it");
          }
          workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (workOrder);
          if (project is not null) {
            if (this.Quantity is null) {
              log.Warn ($"CreateWorkOrder: the quantity is not set while the project id is set, consider the default value");
            }
            else {
              var workOrderProject = ModelDAOHelper.DAOFactory.WorkOrderProjectDAO
                .Get (workOrder, project);
              workOrderProject.Quantity = this.Quantity ?? 1;
              ModelDAOHelper.DAOFactory.WorkOrderProjectDAO
                .MakePersistent (workOrderProject);
            }
            var workOrderProjectUpdate = ModelDAOHelper.ModelFactory
              .CreateWorkOrderProjectUpdate (workOrder, project, WorkOrderProjectUpdateModificationType.NEW);
            ModelDAOHelper.DAOFactory.WorkOrderProjectUpdateDAO
              .MakePersistent (workOrderProjectUpdate);
          }
          transaction.Commit ();
          return workOrder;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type for a new work order
  /// </summary>
  public class NewWorkOrderInputType : InputObjectGraphType<NewWorkOrder>
  {
    readonly ILog log = LogManager.GetLogger (typeof (NewWorkOrderInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NewWorkOrderInputType ()
    {
      Name = "NewWorkOrder";
      Field<string?> ("name", nullable: true);
      Field<string?> ("code", nullable: true);
      Field<string?> ("externalCode", nullable: true);
      Field<string?> ("documentLink", nullable: true);
      Field<IdGraphType, int?> ("customerId");
      Field<UtcDateTimeGraphType, DateTime?> ("deliveryDate");
      Field<IdGraphType, int?> ("statusId"); // Default: 1. Undefined
      Field<IdGraphType, int?> ("projectId");
      Field<int?> ("quantity", nullable: true);
    }
  }
}
