// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.DataLoader;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="IWorkOrder"/>
  /// </summary>
  public class WorkOrderGraphType : ObjectGraphType<IWorkOrder>
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkOrderGraphType).FullName);

    readonly DataStructure m_dataStructure;

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkOrderGraphType (IDataLoaderContextAccessor accessor, DataStructure dataStructure)
    {
      m_dataStructure = dataStructure;

      Name = "WorkOrder";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      Field<ListGraphType<WorkInfoGraphType>> ("parents").Resolve (ctx => null);
      if (m_dataStructure.ProjectComponentIsPart) {
        Field<ListGraphType<WorkInfoGraphType>, ICollection<IPart>> ("children")
          .Resolve (ctx => ctx.Source.Parts);
      }
      else {
        Field<ListGraphType<WorkInfoGraphType>, ICollection<IProject>> ("children")
          .Resolve (ctx => ctx.Source.Projects);
      }
      Field<CustomerGraphType, ICustomer> ("customer");
      Field<UtcDateTimeGraphType, DateTime?> ("deliveryDate");
      Field<NonNullGraphType<WorkOrderStatusGraphType>, IWorkOrderStatus> ("status");
      /* Using a dataLoader and caching the values
        .ResolveAsync (ctx => {
          var loader = accessor?.Context?.GetOrAddBatchLoader<int, IWorkOrderStatus> ("GetWorkOrderStatusById", ModelDAOHelper.DAOFactory.WorkOrderStatusDAO.GetWorkOrderStatusesByIdAsync) ?? throw new InvalidOperationException ();
          return loader.LoadAsync (ctx.Source.Status.Id);
        });
      */
      //      Field<NonNullGraphType<ListGraphType<NonNullGraphType<WorkOrderProjectType>>>> ("workOrderProjects").Resolve (ctx => ctx.Source.); // TODO: Projects

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }



  }
}
