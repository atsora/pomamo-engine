// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using GraphQL.Validation;
using GraphQL.Types;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GraphQL.Execution;
using Lemoine.Core.ExceptionManagement;
using Lemoine.Extensions.Web.Responses;
using Pulse.Graphql.Type;
using System.Threading.Tasks;
using GraphQL.SystemTextJson;
using Pulse.Graphql.InputType;

namespace Pulse.Graphql
{
  /// <summary>
  /// Dependency injection methods for GraphQL
  /// </summary>
  public static class GraphQLMiddlewareServices
  {
    static readonly ILog log = LogManager.GetLogger (typeof (GraphQLMiddlewareServices).FullName);

    static readonly string EXPOSE_EXCEPTION_STACK_TRACE_KEY = "GraphQL.ExceptionStackTrace";
    static readonly bool EXPOSE_EXCEPTION_STACK_TRACE_DEFAULT = false;

    /// <summary>
    /// Configure the services for GraphQL
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection ConfigureServices (IServiceCollection services)
    {
      bool exposeExceptionStackTrace = Lemoine.Info.ConfigSet.LoadAndGet (EXPOSE_EXCEPTION_STACK_TRACE_KEY, EXPOSE_EXCEPTION_STACK_TRACE_DEFAULT);

      services.AddGraphQL (builder => builder
        .ConfigureExecutionOptions (options => { options.EnableMetrics = true; options.UnhandledExceptionDelegate = ManageUnhandledException; })
        .AddSystemTextJson ()
        .AddSchema<PulseSchema> ()
        .AddDataLoader ()
        .AddErrorInfoProvider (new Pulse.Graphql.ErrorInfoProvider (exposeExceptionDetails: exposeExceptionStackTrace))
        .AddGraphTypes (typeof (Query).Assembly)
        );
      ;

      return services
        .AddSingleton<Lemoine.Model.DataStructure> ()
        .AddSingleton<IDocumentExecuter, DocumentExecuter> ()
        .AddSingleton<IGraphQLSerializer, GraphQLSerializer> ()
        .AddSingleton<IDataLoaderContextAccessor, DataLoaderContextAccessor> ()
        .AddSingleton<DataLoaderDocumentListener> ()

        .AddSingleton<Query> ()
        .AddSingleton<Mutation> ()

        .AddSingleton<NewJobInputType> ()
        .AddSingleton<NewProjectInputType> ()
        .AddSingleton<NewWorkOrderInputType> ()
        .AddSingleton<UpdateJobInputType> ()
        .AddSingleton<UpdateProjectInputType> ()
        .AddSingleton<UpdateWorkOrderInputType> ()

        .AddSingleton<ComponentGraphType> ()
        .AddSingleton<ComponentIntermediateWorkPieceGraphType> ()
        .AddSingleton<ComponentTypeGraphType> ()
        .AddSingleton<CustomerGraphType> ()
        .AddSingleton<DisplayableInterface> ()
        .AddSingleton<IntermediateWorkPieceGraphType> ()
        .AddSingleton<JobGraphType> ()
        .AddSingleton<MachineFilterGraphType> ()
        .AddSingleton<MachineGraphType> ()
        .AddSingleton<OperationDurationGraphType> ()
        .AddSingleton<OperationGraphType> ()
        .AddSingleton<OperationModelGraphType> ()
        .AddSingleton<OperationRevisionGraphType> ()
        .AddSingleton<OperationTypeGraphType> ()
        .AddSingleton<PartGraphType> ()
        .AddSingleton<ProjectGraphType> ()
        .AddSingleton<SequenceDurationGraphType> ()
        .AddSingleton<SequenceGraphType> ()
        .AddSingleton<SequenceKindGraphType> ()
        .AddSingleton<SequenceOperationModelGraphType> ()
        .AddSingleton<SimpleOperationGraphType> ()
        .AddSingleton<UtcDateTimeGraphType> ()
        .AddSingleton<UtcDateTimeRangeGraphType> ()
        .AddSingleton<WorkInfoGraphType> ()
        .AddSingleton<WorkInfoInterface> ()
        .AddSingleton<WorkOrderGraphType> ()
        .AddSingleton<WorkOrderProjectGraphType> ()
        .AddSingleton<WorkOrderStatusGraphType> ()
        .AddSingleton<ISchema, PulseSchema> ()
        ;
    }

    static void SetUnhandledExceptionContext (UnhandledExceptionContext ctx, string message, ErrorStatus errorStatus)
    {
      ctx.ErrorMessage = message;
      ctx.Exception.Data["ErrorMessage"] = message;
      ctx.Exception.Data["ErrorStatus"] = errorStatus.ToString ();
      if (!string.Equals (message, ctx.Exception.Message)) {
        ctx.Exception.Data["ErrorDetails"] = ctx.Exception.Message;
      }
    }

    static Task ManageUnhandledException (UnhandledExceptionContext ctx)
    {
      if (ctx.Exception is DataProcessingException ex) {
        log.Error ($"ManageUnhandledException: DataProcessingException with errorStatus={ex.ErrorStatus}", ex);
        SetUnhandledExceptionContext (ctx, ex.Message, ex.ErrorStatus);
      }
      else if (ExceptionTest.IsDatabaseConnectionError (ctx.Exception, log)) {
        log.Error ($"ManageUnhandledException: ", ctx.Exception);
        SetUnhandledExceptionContext (ctx, "Database connection error", ErrorStatus.DatabaseConnectionError);
      }
      else if (ExceptionTest.IsDatabaseException (ctx.Exception, log, out IDatabaseExceptionDetails details)) {
        log.Error ($"ManageUnhandledException: database exception, detail={details.Detail}", ctx.Exception);
        var errorStatus = ExceptionTest.IsTemporaryWithDelay (ctx.Exception, log)
          ? ErrorStatus.ProcessingDelay
          : ErrorStatus.TransientProcessError;
        SetUnhandledExceptionContext (ctx, "Database error, retrying soon", errorStatus);
      }
      else if (ExceptionTest.IsTemporaryWithDelay (ctx.Exception, log)) {
        log.Error ($"ManageUnhandledException: temporary with delay", ctx.Exception);
        SetUnhandledExceptionContext (ctx, "Transient error, retrying", ErrorStatus.ProcessingDelay);
      }
      else if (ExceptionTest.IsTemporary (ctx.Exception, log)) {
        log.Error ($"ManageUnhandledException: temporary", ctx.Exception);
        SetUnhandledExceptionContext (ctx, "Transient error, retrying", ErrorStatus.TransientProcessError);
      }
      else if ((ctx.Exception is ObjectDisposedException) || (ctx.Exception is InvalidOperationException)) { // TODO: InvalidOperationException ?
        log.Error ($"ManageUnhandledException: disposed or invalid (restarting ?)", ctx.Exception);
        SetUnhandledExceptionContext (ctx, "Transient error, retrying", ErrorStatus.TransientProcessError);
      }
      else if (ExceptionTest.IsNotError (ctx.Exception, log)) {
        log.Info ($"ManageUnhandledException: not an error", ctx.Exception);
        SetUnhandledExceptionContext (ctx, "Not a real error", ErrorStatus.TransientProcessError);
      }
      else {
        log.Fatal ($"ManageUnhandledException: other exception", ctx.Exception);
        SetUnhandledExceptionContext (ctx, "Unexpected problem, retrying", ErrorStatus.TransientProcessError);
      }
      return Task.CompletedTask;
    }

  }
}
