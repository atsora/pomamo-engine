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
  /// GraphQL type for a <see cref="Lemoine.Model.IMachine" />
  /// </summary>
  public class MachineGraphType: ObjectGraphType<IMachine>
  {
    readonly ILog log = LogManager.GetLogger (typeof (MachineGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineGraphType ()
    {
      Name = "Machine";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name");
    }
  }
}
