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
  /// Class to create a <see cref="IProject"/>
  /// </summary>
  public class UpdateProject
  {
    readonly ILog log = LogManager.GetLogger<UpdateProject> ();

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

    /// <summary>
    /// Constructor
    /// </summary>
    public UpdateProject () { }

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
    /// Apply the change
    /// </summary>
    /// <param name="ctx"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IProject Update ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("UpdateProject")) {
          var id = this.Id;
          var project = ModelDAOHelper.DAOFactory.ProjectDAO
            .FindById (id);
          if (project is null) {
            log.Error ($"Update: no project with id={id}");
            throw new InvalidOperationException ($"unknown project with id={id}");
          }
          if (this.NameSet) {
            project.Name = this.Name;
          }
          if (this.CodeSet) {
            project.Code = this.Code;
          }
          if (this.ExternalCodeSet) {
            project.ExternalCode = this.ExternalCode;
          }
          if (this.CustomerIdSet) {
            if (this.CustomerId is null) {
              project.Customer = null;
            }
            else {
              var customer = ModelDAOHelper.DAOFactory.CustomerDAO
                .FindById (this.CustomerId.Value);
              if (customer is null) {
                log.Error ($"Update: no customer with id={this.CustomerId}");
                throw new InvalidOperationException ($"unknown customer with id={this.CustomerId}");
              }
              project.Customer = customer;
            }
          }
          ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (project);
          transaction.Commit ();
          return project;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type to update a <see cref="Lemoine.Model.IProject" />
  /// </summary>
  public class UpdateProjectInputType : InputObjectGraphType<UpdateProject>
  {
    readonly ILog log = LogManager.GetLogger (typeof (UpdateProjectInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UpdateProjectInputType ()
    {
      Name = "UpdateProject";
      Field<NonNullGraphType<IdGraphType>, int> ("id");
      Field<string?> ("name", nullable: true);
      Field<string?> ("code", nullable: true);
      Field<string?> ("externalCode", nullable: true);
      Field<string?> ("documentLink", nullable: true);
      Field<IdGraphType, int?> ("customerId");
    }
  }
}
