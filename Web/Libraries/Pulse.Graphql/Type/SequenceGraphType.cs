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
  /// GraphQL type for a <see cref="Lemoine.Model.ISequence" />
  /// </summary>
  public class SequenceGraphType : ObjectGraphType<ISequence>
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceGraphType ()
    {
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<OperationTypeGraphType>, IOperation> ("operation");
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationModelGraphType>>>, IEnumerable<IOperationModel>> ("models")
        .Resolve (ctx => ctx.Source.GetOperationModels ());
      Field<string> ("name", nullable: true);
      Field<string> ("description", nullable: true);
      Field<string> ("code", nullable: true)
        .Resolve (ctx => null);
      Field<string> ("toolNumber", nullable: true);
      Field<bool> ("autoOnly", nullable: false);
      Field<int?> ("operationStep", nullable: true);
      Field<NonNullGraphType<SequenceKindGraphType>, SequenceKind> ("kind");
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<SequenceOperationModelGraphType>>>, IEnumerable<ISequenceOperationModel>> ("sequenceOperationModels")
        .Resolve (ctx => ctx.Source.GetSequenceOperationModels ());
      Field<double?> ("defaultModelDuration", nullable: true)
        .Resolve (ctx => ctx.Source.EstimatedTimeSeconds);
    }
  }
}
