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
  public class NewProject
  {
    readonly ILog log = LogManager.GetLogger<NewProject> ();

    /// <summary>
    /// Constructor
    /// </summary>
    public NewProject () { }

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
    /// Create a new work order
    /// </summary>
    /// <returns></returns>
    public IProject CreateProject ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ("CreateProject")) {
          IProject project;
          if (!string.IsNullOrEmpty (this.Name)) {
            project = ModelDAOHelper.ModelFactory.CreateProjectFromName (this.Name);
            project.Code = this.Code;
          }
          else if (!string.IsNullOrEmpty (this.Code)) {
            project = ModelDAOHelper.ModelFactory.CreateProjectFromCode (this.Code);
          }
          else {
            log.Error ($"CreateProject: name or code is mandatory");
            throw new InvalidOperationException ("name or code is mandatory in a project");
          }
          project.ExternalCode = this.ExternalCode;
          project.DocumentLink = this.DocumentLink;
          if (this.CustomerId is not null) {
            var customer = ModelDAOHelper.DAOFactory.CustomerDAO
              .FindById (this.CustomerId.Value);
            if (customer is null) {
              log.Error ($"CreateProject: the customer with id {this.CustomerId} does not exist");
              throw new InvalidOperationException ("invalid customer id");
            }
          }
          project = ModelDAOHelper.DAOFactory.ProjectDAO.MakePersistent (project);
          transaction.Commit ();
          return project;
        }
      }
    }
  }

  /// <summary>
  /// Input graphql type for a new work order
  /// </summary>
  public class NewProjectInputType : InputObjectGraphType<NewProject>
  {
    readonly ILog log = LogManager.GetLogger (typeof (NewProjectInputType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NewProjectInputType ()
    {
      Name = "NewProject";
      Field<string?> ("name", nullable: true);
      Field<string?> ("code", nullable: true);
      Field<string?> ("externalCode", nullable: true);
      Field<string?> ("documentLink", nullable: true);
      Field<IdGraphType, int?> ("customerId");
    }
  }
}
