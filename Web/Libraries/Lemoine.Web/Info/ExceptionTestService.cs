// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Web.Info
{
  /// <summary>
  /// Description of ExceptionTestService
  /// </summary>
  public class ExceptionTestService
    : GenericNoCacheService<ExceptionTestRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ExceptionTestService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public ExceptionTestService ()
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override System.Threading.Tasks.Task<object> Get (ExceptionTestRequestDTO request)
    {
      throw new Exception ("Test");
    }
  }
}
