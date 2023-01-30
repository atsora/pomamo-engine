// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Attributes;
using Lemoine.Extensions.Web.Interfaces;

namespace Lemoine.AspTestContracts
{
  [Route("/", "GET")]
  [Route("/status", "GET")]
  [Route("/AspStatus", "GET")]
  public class AspStatusQuery : IReturn<string>
  {
    readonly ILog log = LogManager.GetLogger (typeof (AspStatusQuery).FullName);


  }

  public class AspStatusHandler : ICachedHandler
  {
    public async Task<AspStatusResponse> Get (AspStatusQuery query)
    {
      return await Task.Run<AspStatusResponse> ( () => new AspStatusResponse { Text = "Hello World"});
    }

    public TimeSpan GetCacheTimeOut (string pathQuery, object query, object response)
    {
      return TimeSpan.FromSeconds (30);
    }
  }

  public class AspStatusResponse
  {
    public string Text { get; set; }
  }
}
