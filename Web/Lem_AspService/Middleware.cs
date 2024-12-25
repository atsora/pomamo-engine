// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Atsora.Graphql;
using Lemoine.Core.Log;
using Lemoine.WebMiddleware;
using Microsoft.AspNetCore.Builder;
using Pulse.Graphql;

namespace Lem_AspService
{
  public static class Middleware
  {
    public static IApplicationBuilder InjectPulseMiddleware (this IApplicationBuilder builder)
    {
      return builder
        .UseMiddleware<ExceptionMiddleware> ()
        .UseMiddleware<MaintenanceMiddleware> ()
        .UseMiddleware<RequestLoggerMiddleware> ()
        .UseMiddleware<BodyRewindMiddleware> ()
        .UseAuthentication ()
        .InjectGraphQL ()
        .InjectHotChocolate ()
        .UseMiddleware<CacheMiddleware> ()
        .UseMiddleware<RequestAttemptMiddleware> ()
        .UseMiddleware<CustomRoutingMiddleware> ();
    }
  }
}
