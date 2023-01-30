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
  /// GraphQL type for a <see cref="Lemoine.Model.IOperationType" />
  /// </summary>
  public class OperationTypeGraphType : ObjectGraphType<IOperationType>
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationTypeGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationTypeGraphType ()
    {
      Name = "OperationType";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("display");
      Field<string> ("code", nullable: true);
      Field<int> ("priority", nullable: true);

      Interface<DisplayableInterface> ();
    }
  }
}
