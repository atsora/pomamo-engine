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
  /// Graphql type for <see cref="ISimpleOperation"/>
  /// </summary>
  public class SimpleOperationGraphType : ObjectGraphType<ISimpleOperation>
  {
    readonly ILog log = LogManager.GetLogger (typeof (SimpleOperationGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public SimpleOperationGraphType (DataStructure dataStructure)
    {
      Name = "SimpleOperation";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      if (dataStructure.ProjectComponentIsPart) {
        Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IPart>> ("parents").Resolve (ctx => ctx.Source.IntermediateWorkPiece.ComponentIntermediateWorkPieces.Select (y => y.Component.Part));
      }
      else {
        Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IComponent>> ("parents").Resolve (ctx => ctx.Source.IntermediateWorkPiece.ComponentIntermediateWorkPieces.Select (y => y.Component));
      }
      Field<ListGraphType<WorkInfoGraphType>> ("children").Resolve (ctx => null);
      Field<string> ("longdisplay");
      Field<string> ("shortdisplay");
      Field<NonNullGraphType<OperationTypeGraphType>, IOperationType> ("type");
      Field<int> ("quantity", nullable: true);
      Field<double> ("weight", nullable: true);
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("creationDateTime");
      Field<UtcDateTimeGraphType, DateTime?> ("archiveDateTime");
      // ISimpleOperation carries none of the four fields below: they all belong to the
      // operation it wraps, the way durations does further down. Without an explicit
      // resolver they are looked up by name on ISimpleOperation, resolve to null, and a
      // non-null field turns that into an error
      Field<NonNullGraphType<OperationRevisionGraphType>, IOperationRevision> ("activeRevision")
        .Resolve (ctx => ctx.Source.Operation.ActiveRevision);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationRevisionGraphType>>>, IEnumerable<IOperationRevision>> ("revisions") // TODO: Get revisions, from a list of operations with the same name
        .Resolve (ctx => ctx.Source.Operation.Revisions);
      Field<NonNullGraphType<OperationModelGraphType>, IOperationModel> ("defaultActiveModel")
        .Resolve (ctx => ctx.Source.Operation.DefaultActiveModel);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<IntermediateWorkPieceGraphType>>>, IEnumerable<IIntermediateWorkPiece>> ("intermediateWorkPieces")
        .Resolve (ctx => new[] { ctx.Source.IntermediateWorkPiece });
      Field<double?> ("defaultActiveMachiningDuration", nullable: true)
        .Resolve (ctx => ctx.Source.MachiningDuration?.TotalSeconds);
      Field<double?> ("defaultActiveLoadingDuration", nullable: true)
        .Resolve (ctx => ctx.Source.LoadingDuration?.TotalSeconds);
      Field<double?> ("defaultActiveUnloadingDuration", nullable: true)
        .Resolve (ctx => ctx.Source.UnloadingDuration?.TotalSeconds);
      Field<double?> ("defaultActiveSetupDuration", nullable: true)
        .Resolve (ctx => ctx.Source.SetUpDuration?.TotalSeconds);
      Field<double?> ("defaultActiveTeardownDuration", nullable: true)
        .Resolve (ctx => ctx.Source.TearDownDuration?.TotalSeconds);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationDurationGraphType>>>, ICollection<IOperationDuration>> ("durations")
        .Resolve (ctx => ctx.Source.Operation.Durations);

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }
  }
}
