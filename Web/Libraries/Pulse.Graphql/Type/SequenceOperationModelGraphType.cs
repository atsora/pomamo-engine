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
  /// GraphQL type for a <see cref="Lemoine.Model.ISequenceOperationModel" />
  /// </summary>
  public class SequenceOperationModelGraphType : ObjectGraphType<ISequenceOperationModel>
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceOperationModelGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceOperationModelGraphType ()
    {
      Name = "SequenceOperationModel";
      Field<NonNullGraphType<SequenceGraphType>, ISequence> ("sequence");
      Field<NonNullGraphType<OperationModelGraphType>, IOperationModel> ("operationModel");
      Field<double> ("order", nullable: false);
      Field<int> ("pathNumber", nullable: true);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<SequenceDurationGraphType>>>, IEnumerable<ISequenceDuration>> ("durations");
    }
  }
}
