// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Microsoft.AspNetCore.Http;
using Lemoine.Core.ExceptionManagement;
using System.Net.Sockets;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Pulse.Graphql
{
  /// <summary>
  /// Keep the same database session during a graphql request
  /// </summary>
  public class GraphQLSession
  {
    readonly ILog log = LogManager.GetLogger<GraphQLSession> ();

    readonly RequestDelegate m_next;

    /// <summary>
    /// Constructor
    /// </summary>
    public GraphQLSession (RequestDelegate next)
    {
      m_next = next;
    }

    public async Task InvokeAsync (Microsoft.AspNetCore.Http.HttpContext context)
    {
      var path = context.Request.Path;
      if (path.HasValue && (path.Value?.StartsWith ("/graphql") ?? false)) {
        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          await m_next.Invoke (context);
        }
      }
      else {
        await m_next.Invoke (context);
      }
    }
  }
}
