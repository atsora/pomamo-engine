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
  /// GraphQL type for a <see cref="IComponentIntermediateWorkPiece" />
  /// </summary>
  public class ComponentIntermediateWorkPieceGraphType : ObjectGraphType<IComponentIntermediateWorkPiece>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ComponentIntermediateWorkPieceGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ComponentIntermediateWorkPieceGraphType ()
    {
      Name = "ComponentIntermediateWorkPiece";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<NonNullGraphType<ComponentGraphType>, IComponent> ("component");
      Field<NonNullGraphType<IntermediateWorkPieceGraphType>, IIntermediateWorkPiece> ("intermediateWorkPiece");
      Field<string> ("code", nullable: true);
      Field<int> ("order", nullable: true);
    }
  }
}
