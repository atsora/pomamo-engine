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
  /// Graphql type for <see cref="IComponent"/>
  /// </summary>
  public class ComponentGraphType : ObjectGraphType<IComponent>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ComponentGraphType).FullName);

    readonly DataStructure m_dataStructure;

    /// <summary>
    /// Constructor
    /// </summary>
    public ComponentGraphType (DataStructure dataStructure)
    {
      m_dataStructure = dataStructure;

      Name = "Component";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      if (dataStructure.WorkOrderProjectIsJob) {
        Field<ListGraphType<WorkInfoGraphType>, IList<IJob>> ("parents").Resolve (ctx => new List<IJob> { ctx.Source.Project.Job });
      }
      else {
        Field<ListGraphType<WorkInfoGraphType>, IList<IProject>> ("parents").Resolve (ctx => new List<IProject> { ctx.Source.Project });
      }
      Field<ListGraphType<WorkInfoGraphType>, IEnumerable<IOperation>> ("children").Resolve (ctx => ctx.Source.ComponentIntermediateWorkPieces.Select (x => x.IntermediateWorkPiece.Operation));
      Field<NonNullGraphType<ComponentTypeGraphType>, IComponentType> ("type");
      Field<IntermediateWorkPieceGraphType, IIntermediateWorkPiece> ("finalWorkPiece");
      Field<double> ("estimatedHours");
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<ComponentIntermediateWorkPieceGraphType>>>, ICollection<IComponentIntermediateWorkPiece>> ("componentIntermediateWorkPieces");
      Field<ProjectGraphType, IProject> ("project");

      Interface<DisplayableInterface> ();
      Interface<WorkInfoInterface> ();
    }
  }
}
