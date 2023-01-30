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
  /// Class to create a <see cref="IJob"/>
  /// </summary>
  public class NewJob
  {
    readonly ILog log = LogManager.GetLogger<NewJob> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public NewJob () { }

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
    /// Associated quantity
    /// </summary>
    public int? Quantity { get; set; }

    /// <summary>
    /// Create a new work order
    /// </summary>
    /// <returns></returns>
    public IJob CreateJob ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("CreateJob")) {
          var workOrderStatusId = this.StatusId ?? 1; // 1: Undefined
          var workOrderStatus = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO
            .FindById (workOrderStatusId);
          if (workOrderStatus is null) {
            log.Error ($"CreateWorkOrder: work order status with Id={workOrderStatusId} does not exist");
            throw new InvalidOperationException ("Invalid work order status");
          }
          IJob job;
          if (!string.IsNullOrEmpty (this.Name)) {
            job = ModelDAOHelper.ModelFactory.CreateJobFromName  (workOrderStatus, this.Name);
            job.Code = this.Code;
          }
          else if (!string.IsNullOrEmpty (this.Code)) {
            job = ModelDAOHelper.ModelFactory.CreateJobFromCode (workOrderStatus, this.Code);
          }
          else {
            log.Error ($"CreateJob: name or code is mandatory");
            throw new InvalidOperationException ("name or code is mandatory in a job");
          }
          job.ExternalCode = this.ExternalCode;
          job.DocumentLink = this.DocumentLink;
          if (this.CustomerId is not null) {
            var customer = ModelDAOHelper.DAOFactory.CustomerDAO
              .FindById (this.CustomerId.Value);
            if (customer is null) {
              log.Error ($"CreateJob: the customer with id {this.CustomerId} does not exist");
              throw new InvalidOperationException ("invalid customer id");
            }
          }
          job.DeliveryDate = this.DeliveryDate;
          job = ModelDAOHelper.DAOFactory.JobDAO.MakePersistent (job);
          if (this.Quantity is null) {
            log.Info ($"CreateJob: the quantity is not set, consider the default value");
          }
          else {
            var workOrderProject = ModelDAOHelper.DAOFactory.WorkOrderProjectDAO
              .Get (job.WorkOrder, job.Project);
            workOrderProject.Quantity = this.Quantity ?? 1;
            ModelDAOHelper.DAOFactory.WorkOrderProjectDAO
              .MakePersistent (workOrderProject);
          }
          transaction.Commit ();
          return job;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type for a new work order
  /// </summary>
  public class NewJobInputType : InputObjectGraphType<NewJob>
  {
    readonly ILog log = LogManager.GetLogger (typeof (NewJobInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NewJobInputType ()
    {
      Name = "NewJob";
      Field<string?> ("name", nullable: true);
      Field<string?> ("code", nullable: true);
      Field<string?> ("externalCode", nullable: true);
      Field<string?> ("documentLink", nullable: true);
      Field<IdGraphType, int?> ("customerId");
      Field<UtcDateTimeGraphType, DateTime?> ("deliveryDate");
      Field<IdGraphType, int?> ("statusId"); // Default: 1. Undefined
      Field<int?> ("quantity", nullable: true);
    }
  }
}
