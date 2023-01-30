// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphQL;
using GraphQL.DataLoader;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using GraphQL.SystemTextJson;
using GraphQL.Validation;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.Types;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Builder;

namespace Pulse.Graphql
{
  /// <summary>
  /// Middleware methods
  /// </summary>
  public static class Middleware
  {
    /// <summary>
    /// Inject the middleware layers for GraphQL
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IApplicationBuilder InjectGraphQL (this IApplicationBuilder builder)
    {
      return builder
        .UseMiddleware<GraphQLSession> ()
        .UseGraphQL<PulseSchema> ()
        .UseGraphQLGraphiQL ();
// .UseGraphQLPlayground ()
        ;
    }
  }
}
