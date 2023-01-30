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
using Microsoft.Extensions.DependencyInjection;
using Pulse.Graphql.Type;

namespace Pulse.Graphql
{
  /*
type Machine {
  id: ID!
  name: String!
}

type Query
{
  machine(id: ID!): Machine!
}
  */

  /// <summary>
  /// GraphQL schema
  /// </summary>
  public class PulseSchema : Schema, ISchema
  {
    readonly ILog log = LogManager.GetLogger (typeof (PulseSchema).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PulseSchema (IServiceProvider serviceProvider)
      : base(serviceProvider)
    {
      Query = serviceProvider.GetRequiredService<Query> ();
      Mutation = serviceProvider.GetRequiredService<Mutation> ();

      this.RegisterTypeMapping<DateTime?, UtcDateTimeGraphType> ();
      this.RegisterTypeMapping<DateTime, NonNullGraphType<UtcDateTimeGraphType>> ();
      this.RegisterTypeMapping<IWorkOrderStatus, WorkOrderStatusGraphType> ();
      this.RegisterTypeMapping<IWorkOrder, WorkOrderGraphType> ();

      // ValueConverter.Register<DateTime, string> (v => );
    }
  }
}
