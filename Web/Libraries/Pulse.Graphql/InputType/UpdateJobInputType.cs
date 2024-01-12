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
  public class UpdateJob
  {
    readonly ILog log = LogManager.GetLogger<UpdateJob> ();

    string? m_name;
    bool m_nameSet = false;
    string? m_code;
    bool m_codeSet = false;
    string? m_externalCode;
    bool m_externalCodeSet = false;
    string? m_documentLink;
    bool m_documentLinkSet = false;
    int? m_customerId;
    bool m_customerIdSet = false;
    DateTime? m_deliveryDate;
    bool m_deliveryDateSet = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public UpdateJob () { }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; set; } = 0;

    /// <summary>
    /// Name
    /// </summary>
    public string? Name
    {
      get => m_name;
      set {
        m_name = value;
        m_nameSet = true;
      }
    }

    /// <summary>
    /// Is Name set ?
    /// </summary>
    public bool NameSet => m_nameSet;

    /// <summary>
    /// Code
    /// </summary>
    public string? Code
    {
      get => m_code;
      set {
        m_code = value;
        m_codeSet = true;
      }
    }

    /// <summary>
    /// Is Code set ?
    /// </summary>
    public bool CodeSet => m_codeSet;

    /// <summary>
    /// ExternalCode
    /// </summary>
    public string? ExternalCode
    {
      get => m_externalCode;
      set {
        m_externalCode = value;
        m_externalCodeSet = true;
      }
    }

    /// <summary>
    /// Is ExternalCode set ?
    /// </summary>
    public bool ExternalCodeSet => m_externalCodeSet;

    /// <summary>
    /// DocumentLink
    /// </summary>
    public string? DocumentLink
    {
      get => m_documentLink;
      set {
        m_documentLink = value;
        m_documentLinkSet = true;
      }
    }

    /// <summary>
    /// Is DocumentLink set ?
    /// </summary>
    public bool DocumentLinkSet => m_documentLinkSet;

    /// <summary>
    /// Id of associated <see cref="ICustomer"/>
    /// </summary>
    public int? CustomerId
    {
      get => m_customerId;
      set {
        m_customerId = value;
        m_customerIdSet = true;
      }
    }

    /// <summary>
    /// Is CustomerId set ?
    /// </summary>
    public bool CustomerIdSet => m_customerIdSet;

    /// <summary>
    /// Delivery date
    /// </summary>
    public DateTime? DeliveryDate
    {
      get => m_deliveryDate;
      set {
        m_deliveryDate = value;
        m_deliveryDateSet = true;
      }
    }

    /// <summary>
    /// Is delivery date set ?
    /// </summary>
    public bool DeliveryDateSet => m_deliveryDateSet;

    /// <summary>
    /// Id of associated <see cref="IJobStatus"/>
    /// 
    /// If null, do not make any change
    /// </summary>
    public int? StatusId { get; set; }

    /// <summary>
    /// Apply the change
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IJob Update ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("UpdateWorkOrder")) {
          var id = this.Id;
          var job = ModelDAOHelper.DAOFactory.JobDAO
            .FindById (id);
          if (job is null) {
            log.Error ($"Update: no job with id={id}");
            throw new InvalidOperationException ($"unknown job with id={id}");
          }
          if (this.NameSet) {
            job.Name = this.Name;
          }
          if (this.CodeSet) {
            job.Code = this.Code;
          }
          if (this.ExternalCodeSet) {
            job.ExternalCode = this.ExternalCode;
          }
          if (this.CustomerIdSet) {
            if (this.CustomerId is null) {
              job.Customer = null;
            }
            else {
              var customer = ModelDAOHelper.DAOFactory.CustomerDAO
                .FindById (this.CustomerId.Value);
              if (customer is null) {
                log.Error ($"Update: no customer with id={this.CustomerId}");
                throw new InvalidOperationException ($"unknown customer with id={this.CustomerId}");
              }
              job.Customer = customer;
            }
          }
          if (this.DeliveryDateSet) {
            job.DeliveryDate = this.DeliveryDate;
          }
          if (this.StatusId is not null) {
            var status = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO
              .FindById (this.StatusId.Value);
            if (status is null) {
              log.Error ($"Update: no work order status with id={this.StatusId}");
              throw new InvalidOperationException ($"unknown work order status with id={this.StatusId}");
            }
            job.Status = status;
          }
          ModelDAOHelper.DAOFactory.JobDAO.MakePersistent (job);
          transaction.Commit ();
          return job;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type to update a <see cref="Lemoine.Model.IJob" />
  /// </summary>
  public class UpdateJobInputType : InputObjectGraphType<UpdateJob>
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateJobInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UpdateJobInputType ()
    {
      Name = "UpdateJob";
      Field<NonNullGraphType<IdGraphType>, int> ("id");
      Field<string?> ("name", nullable: true);
      Field<string?> ("code", nullable: true);
      Field<string?> ("externalCode", nullable: true);
      Field<string?> ("documentLink", nullable: true);
      Field<IdGraphType, int?> ("customerId");
      Field<UtcDateTimeGraphType, DateTime?> ("deliveryDate");
      Field<IdGraphType, int?> ("statusId");
    }
  }
}
