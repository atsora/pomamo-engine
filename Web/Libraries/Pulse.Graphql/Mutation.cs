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
using Pulse.Graphql.InputType;
using Pulse.Graphql.Type;

namespace Pulse.Graphql
{
  /// <summary>
  /// GraphQL type for a <see cref="Lemoine.Model.IMachine" />
  /// </summary>
  public class Mutation : ObjectGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (Mutation).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Mutation ()
    {
      Name = "Mutation";

      Field<NonNullGraphType<WorkOrderGraphType>, IWorkOrder> ("createWorkOrder")
        .Argument<NonNullGraphType<NewWorkOrderInputType>> ("workOrder")
        .Resolve (ctx => ctx.GetArgument<NewWorkOrder> ("workOrder").CreateWorkOrder ());
      Field<NonNullGraphType<WorkOrderGraphType>, IWorkOrder> ("updateWorkOrder")
        .Argument<NonNullGraphType<UpdateWorkOrderInputType>> ("workOrder")
        .Resolve (ctx => ctx.GetArgument<UpdateWorkOrder> ("workOrder").Update ());
      Field<NonNullGraphType<ProjectGraphType>, IProject> ("createProject")
        .Argument<NonNullGraphType<NewProjectInputType>> ("project")
        .Resolve (ctx => ctx.GetArgument<NewProject> ("project").CreateProject ());
      Field<NonNullGraphType<ProjectGraphType>, IProject> ("updateProject")
        .Argument<NonNullGraphType<UpdateProjectInputType>> ("project")
        .Resolve (ctx => ctx.GetArgument<UpdateProject> ("project").Update ());
      Field<NonNullGraphType<JobGraphType>, IJob> ("createJob")
        .Argument<NonNullGraphType<NewJobInputType>> ("job")
        .Resolve (ctx => ctx.GetArgument<NewJob> ("job").CreateJob ());
      Field<NonNullGraphType<JobGraphType>, IJob> ("updateJob")
        .Argument<NonNullGraphType<UpdateJobInputType>> ("job")
        .Resolve (ctx => ctx.GetArgument<UpdateJob> ("job").Update ());
    }

  }
}
