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
  /// GraphQL type for a <see cref="IOperationModel" />
  /// </summary>
  public class OperationModelGraphType : ObjectGraphType<IOperationModel>
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationModelGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationModelGraphType ()
    {
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<OperationGraphType>, IOperation> ("operation");
      Field<NonNullGraphType<OperationRevisionGraphType>, IOperationRevision> ("revision");
      Field<NonNullGraphType<StringGraphType>> ("display");
      Field<string> ("description", nullable: true);
      Field<UtcDateTimeGraphType, DateTime?> ("archiveDateTime");
      Field<string> ("code", nullable: true);
      Field<NonNullGraphType<BooleanGraphType>> ("default");
      Field<string> ("cadModelName", nullable: true);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<SequenceOperationModelGraphType>>>> ("sequenceOperationModels");
      Field<NonNullGraphType<ListGraphType<IntGraphType>>, IEnumerable<int?>> ("pathNumbers")
        .Resolve (ctx => ctx.Source.GetPathNumbers ());
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationDurationGraphType>>>, IEnumerable<IOperationDuration>> ("durations")
        .Resolve (ctx => ctx.Source.GetDurations ());

      Interface<DisplayableInterface> ();
    }
  }
}
