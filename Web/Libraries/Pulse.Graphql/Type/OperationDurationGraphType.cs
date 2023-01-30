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

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// GraphQL type for a <see cref="Lemoine.Model.IOperationDuration" />
  /// </summary>
  public class OperationDurationGraphType : ObjectGraphType<IOperationDuration>
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationDurationGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationDurationGraphType ()
    {
      Name = "OperationDuration";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<OperationGraphType>, IOperation> ("operation");
      Field<OperationModelGraphType, IOperationModel> ("operationModel");
      Field<MachineFilterGraphType, IMachineFilter> ("machineFilter");
      Field<ListGraphType<NonNullGraphType<MachineGraphType>>, IEnumerable<IMachine>> ("machines")
        .Resolve (ctx => null);
      Field<NonNullGraphType<UtcDateTimeRangeGraphType>, UtcDateTimeRange> ("applicableRange");
      Field<FloatGraphType, double?> ("machining")
        .Resolve (ctx => ctx.Source.Machining?.TotalSeconds);
      Field<FloatGraphType, double?> ("loading")
        .Resolve (ctx => ctx.Source.Loading?.TotalSeconds);
      Field<FloatGraphType, double?> ("unloading")
        .Resolve (ctx => ctx.Source.Unloading?.TotalSeconds);
      Field<FloatGraphType, double?> ("setup")
        .Resolve (ctx => ctx.Source.Setup?.TotalSeconds);
      Field<FloatGraphType, double?> ("teardown")
        .Resolve (ctx => ctx.Source.Teardown?.TotalSeconds);
    }
  }
}
