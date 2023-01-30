// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Microsoft.AspNetCore.Builder;

namespace Lem_CncCoreService.Asp
{
  public static class Middleware
  {
    public static IApplicationBuilder InjectRoutingLogic (this IApplicationBuilder builder)
    {
      builder.Map ("/get", InjectGetRoutingLogic);
      builder.Map ("/set", InjectSetRoutingLogic);
      builder.Map ("/data", InjectDataRoutingLogic);
      builder.Map ("/xml", InjectPostRoutingLogic);
      builder.Map ("/createApiKey", InjectCreateApiKeyRoutingLogic);

      return builder;
    }

    static void InjectGetRoutingLogic (IApplicationBuilder builder)
    {
      builder.UseMiddleware<CheckApiKeyMiddleware> ();
      builder.UseMiddleware<GetMiddleware> ();
    }

    static void InjectSetRoutingLogic (IApplicationBuilder builder)
    {
      builder.UseMiddleware<CheckApiKeyMiddleware> ();
      builder.UseMiddleware<SetMiddleware> ();
    }

    static void InjectDataRoutingLogic (IApplicationBuilder builder)
    {
      builder.UseMiddleware<CheckApiKeyMiddleware> ();
      builder.UseMiddleware<DataMiddleware> ();
    }

    static void InjectPostRoutingLogic (IApplicationBuilder builder)
    {
      builder.UseMiddleware<CheckApiKeyMiddleware> ();
      builder.UseMiddleware<XmlPostMiddleware> ();
    }

    static void InjectCreateApiKeyRoutingLogic (IApplicationBuilder builder)
    {
      builder.UseMiddleware<CreateApiKeyMiddleware> ();
    }
  }
}
