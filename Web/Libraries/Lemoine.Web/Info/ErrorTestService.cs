// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Extensions.Web.Services;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Web.Info
{
  /// <summary>
  /// Description of ErrorTestService
  /// </summary>
  public class ErrorTestService
    : GenericNoCacheService<ErrorTestRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ErrorTestService).FullName);

    /// <summary>
    /// 
    /// </summary>
    public ErrorTestService ()
    {
    }

    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache (ErrorTestRequestDTO request)
    {
      var status = (ErrorStatus)Enum.Parse (typeof (ErrorStatus), request.Status);
      return new ErrorDTO (request.ErrorMessage, status, request.Details);
    }
  }
}
