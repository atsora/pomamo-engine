// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL.Types;
using Lemoine.Core.Log;

namespace Pulse.Graphql.Type
{
  /// <summary>
  /// WorkInfoInterface
  /// </summary>
  public class WorkInfoInterface: InterfaceGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkInfoInterface).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkInfoInterface ()
    {
      Name = "IWorkInfo";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("documentLink", nullable: true);
      Field<string> ("display");
      Field<ListGraphType<WorkInfoGraphType>> ("parents");
      Field<ListGraphType<WorkInfoGraphType>> ("children");
    }
  }
}
