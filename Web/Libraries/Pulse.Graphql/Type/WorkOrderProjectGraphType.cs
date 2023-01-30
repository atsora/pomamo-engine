// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Types;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// Graphql type for <see cref="IWorkOrderProject"/>
  /// </summary>
  public class WorkOrderProjectGraphType : ObjectGraphType<IWorkOrderProject>
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkOrderProjectGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkOrderProjectGraphType ()
    {
      Name = "WorkOrderProject";
      Field<NonNullGraphType<WorkOrderGraphType>, IWorkOrder> ("workOrder");
      Field<NonNullGraphType<ProjectGraphType>, IProject> ("project");
      Field<int> ("quantity", nullable: true);
    }
  }
}
