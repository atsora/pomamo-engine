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
  /// Graphql type for <see cref="IOperation\"/>
  /// </summary>
  public class OperationGraphType : ObjectGraphType<IOperation>
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationGraphType (DataStructure dataStructure)
    {
      Name = "Operation";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<BooleanGraphType>, bool> ("isSimpleOperation")
        .Resolve (ctx => dataStructure.IntermediateWorkPieceOperationIsSimpleOperation);
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      if (dataStructure.ProjectComponentIsPart) {
        Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IPart>> ("parents").Resolve (ctx => ctx.Source.IntermediateWorkPieces.SelectMany (x => x.ComponentIntermediateWorkPieces.Select (y => y.Component.Part)));
      }
      else {
        Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IComponent>> ("parents").Resolve (ctx => ctx.Source.IntermediateWorkPieces.SelectMany (x => x.ComponentIntermediateWorkPieces.Select (y => y.Component)));
      }
      Field<ListGraphType<WorkInfoGraphType>> ("children").Resolve (ctx => null);
      Field<string> ("longdisplay");
      Field<string> ("shortdisplay");
      Field<NonNullGraphType<OperationTypeGraphType>, IOperationType> ("type");
      if (dataStructure.IntermediateWorkPieceOperationIsSimpleOperation) {
        Field<IntGraphType, int?> ("quantity")
          .Resolve (ctx => ctx.Source.SimpleOperation.Quantity);
        Field<FloatGraphType, double?> ("weight")
          .Resolve (ctx => ctx.Source.SimpleOperation.Weight);
      }
      else {
        Field<IntGraphType, int?> ("quantity")
          .Resolve (ctx => ctx.Source.GetTotalNumberOfIntermediateWorkPieces ());
        Field<FloatGraphType, double?> ("weight")
          .Resolve (ctx => null);
      }
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("creationDateTime");
      Field<UtcDateTimeGraphType, DateTime?> ("archiveDateTime");
      Field<NonNullGraphType<OperationRevisionGraphType>, IOperationRevision> ("activeRevision");
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationRevisionGraphType>>>, IEnumerable<IOperationRevision>> ("revisions"); // TODO: Get revisions, from a list of operations with the same name
      Field<NonNullGraphType<OperationModelGraphType>, IOperationModel> ("defaultActiveModel");
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<IntermediateWorkPieceGraphType>>>, ICollection<IIntermediateWorkPiece>> ("intermediateWorkPieces");
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
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationDurationGraphType>>>, ICollection<IOperationDuration>> ("durations");

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }
  }
}
