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
  /// GraphQL type for a <see cref="Lemoine.Model.IComponentType" />
  /// </summary>
  public class ComponentTypeGraphType : ObjectGraphType<IComponentType>
  {
    readonly ILog log = LogManager.GetLogger (typeof (ComponentTypeGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ComponentTypeGraphType ()
    {
      Name = "ComponentType";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("display");
      Field<string> ("code", nullable: true);

      Interface<DisplayableInterface> ();
    }
  }
}
