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
  /// WorkInfoInterface
  /// </summary>
  public class PartGraphType : ObjectGraphType<IPart>
  {
    readonly ILog log = LogManager.GetLogger (typeof (PartGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PartGraphType (DataStructure dataStructure)
    {
      Name = "Part";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<IdGraphType>> ("projectId")
        .Resolve (ctx => ctx.Source.Project.Id);
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      Field<ListGraphType<WorkInfoGraphType>, ICollection<IWorkOrder>> ("parents")
        .Resolve (ctx => ctx.Source.WorkOrders);
      Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IOperation>> ("children").Resolve (ctx => ctx.Source.ComponentIntermediateWorkPieces.Select (x => x.IntermediateWorkPiece.Operation));
      Field<NonNullGraphType<ComponentTypeGraphType>, IComponentType> ("type");
      Field<CustomerGraphType, ICustomer> ("customer");
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("creationDateTime");
      Field<NonNullGraphType<UtcDateTimeGraphType>, DateTime> ("reactivationDateTime");
      Field<UtcDateTimeGraphType, DateTime?> ("archiveDateTime");
      Field<IntermediateWorkPieceGraphType, IIntermediateWorkPiece> ("finalWorkPiece");
      Field<double> ("estimatedHours");
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<ComponentIntermediateWorkPieceGraphType>>>, ICollection<IComponentIntermediateWorkPiece>> ("componentIntermediateWorkPieces");

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }
  }
}
