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
  /// Graphql type for <see cref="IIntermediateWorkPiece"/>
  /// </summary>
  public class IntermediateWorkPieceGraphType : ObjectGraphType<IIntermediateWorkPiece>
  {
    readonly ILog log = LogManager.GetLogger (typeof (IntermediateWorkPieceGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public IntermediateWorkPieceGraphType (DataStructure dataStructure)
    {
      Name = "IntermediateWorkPiece";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      if (dataStructure.ProjectComponentIsPart) {
        Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IPart>> ("parents")
          .Resolve (ctx => ctx.Source.ComponentIntermediateWorkPieces.Select (x => x.Component.Part));
      }
      else {
        Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IComponent>> ("parents")
          .Resolve (ctx => ctx.Source.ComponentIntermediateWorkPieces.Select (x => x.Component));
      }
      Field<ListGraphType<WorkInfoGraphType>, IList<IOperation>> ("children")
        .Resolve (ctx => new List<IOperation> { ctx.Source.Operation });
      Field<int> ("operationQuantity", nullable: true);
      Field<double> ("weight", nullable: true);

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }
  }
}
