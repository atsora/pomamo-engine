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
  /// GraphQL type for a <see cref="Lemoine.Model.ISequenceDuration" />
  /// </summary>
  public class SequenceDurationGraphType : ObjectGraphType<ISequenceDuration>
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceDurationGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceDurationGraphType ()
    {
      Name = "SequenceDuration";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<SequenceOperationModelGraphType>, ISequenceOperationModel> ("sequenceOperationModel");
      Field<MachineFilterGraphType, IMachineFilter> ("machineFilter");
      Field<ListGraphType<NonNullGraphType<MachineGraphType>>, IEnumerable<IMachine>> ("machines")
        .Resolve (ctx => null);
      Field<NonNullGraphType<UtcDateTimeRangeGraphType>, UtcDateTimeRange> ("applicableRange");
      Field<NonNullGraphType<FloatGraphType>, double> ("estimatedDuration")
        .Resolve (ctx => ctx.Source.EstimatedDuration.TotalSeconds);
    }
  }
}
