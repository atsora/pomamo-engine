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
  /// GraphQL type for a <see cref="IOperationRevision" />
  /// </summary>
  public class OperationRevisionGraphType : ObjectGraphType<IOperationRevision>
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationRevisionGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationRevisionGraphType ()
    {
      Name = "OperationRevision";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("display", nullable: false);
      Field<int?> ("number", nullable: true);
      Field<string> ("description", nullable: true);
      Field<NonNullGraphType<ListGraphType<NonNullGraphType<OperationModelGraphType>>>, IEnumerable<IOperationModel>> ("operationModels");

      Interface<DisplayableInterface> ();
    }
  }
}
