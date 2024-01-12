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
  public class UpdateWorkOrder
  {
    readonly ILog log = LogManager.GetLogger<UpdateWorkOrder> ();

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
    public UpdateWorkOrder () { }

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
    /// Id of associated <see cref="IWorkOrderStatus"/>
    /// 
    /// If null, do not make any change
    /// </summary>
    public int? StatusId { get; set; }

    /// <summary>
    /// Apply the change
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IWorkOrder Update ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("UpdateWorkOrder")) {
          var id = this.Id;
          var workOrder = ModelDAOHelper.DAOFactory.WorkOrderDAO
            .FindById (id);
          if (workOrder is null) {
            log.Error ($"Update: no work order with id={id}");
            throw new InvalidOperationException ($"unknown work order with id={id}");
          }
          if (this.NameSet) {
            workOrder.Name = this.Name;
          }
          if (this.CodeSet) {
            workOrder.Code = this.Code;
          }
          if (this.ExternalCodeSet) {
            workOrder.ExternalCode = this.ExternalCode;
          }
          if (this.CustomerIdSet) {
            if (this.CustomerId is null) {
              workOrder.Customer = null;
            }
            else {
              var customer = ModelDAOHelper.DAOFactory.CustomerDAO
                .FindById (this.CustomerId.Value);
              if (customer is null) {
                log.Error ($"Update: no customer with id={this.CustomerId}");
                throw new InvalidOperationException ($"unknown customer with id={this.CustomerId}");
              }
              workOrder.Customer = customer;
            }
          }
          if (this.DeliveryDateSet) {
            workOrder.DeliveryDate = this.DeliveryDate;
          }
          if (this.StatusId is not null) {
            var status = ModelDAOHelper.DAOFactory.WorkOrderStatusDAO
              .FindById (this.StatusId.Value);
            if (status is null) {
              log.Error ($"Update: no work order status with id={this.StatusId}");
              throw new InvalidOperationException ($"unknown work order status with id={this.StatusId}");
            }
            workOrder.Status = status;
          }
          ModelDAOHelper.DAOFactory.WorkOrderDAO.MakePersistent (workOrder);
          transaction.Commit ();
          return workOrder;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type to update a <see cref="Lemoine.Model.IWorkOrder" />
  /// </summary>
  public class UpdateWorkOrderInputType : InputObjectGraphType<UpdateWorkOrder>
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateWorkOrderInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UpdateWorkOrderInputType ()
    {
      Name = "UpdateWorkOrder";
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
