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
  /// Graphql type for a <see cref="ICustomer"/>
  /// </summary>
  public class CustomerGraphType : ObjectGraphType<ICustomer>
  {
    readonly ILog log = LogManager.GetLogger (typeof (CustomerGraphType).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public CustomerGraphType ()
    {
      Name = "Customer";
      Field<NonNullGraphType<IdGraphType>> ("id");
      Field<string> ("name", nullable: true);
      Field<string> ("code", nullable: true);
      Field<string> ("externalCode", nullable: true);
      Field<string> ("display");

      Interface<DisplayableInterface> ();
    }
  }
}
