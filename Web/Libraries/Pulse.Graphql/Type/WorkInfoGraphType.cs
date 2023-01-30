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
  /// WorkInfo union graphql type
  /// </summary>
  public class WorkInfoGraphType: UnionGraphType
  {
    readonly ILog log = LogManager.GetLogger (typeof (WorkInfoGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public WorkInfoGraphType ()
    {
      Name = "WorkInfo";
      Type<WorkOrderGraphType> ();
      Type<ProjectGraphType> ();
      Type<JobGraphType> ();
      Type<ComponentGraphType> ();
      Type<PartGraphType> ();
      Type<OperationGraphType> ();
      Type<SimpleOperationGraphType> ();
    }
  }
}
