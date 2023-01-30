// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Lemoine.Web.Info
{
  /// <summary>
  /// Info services
  /// </summary>
  public class InfoServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request Test
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (TestRequestDTO request)
    {
      return new TestService().Get (this.GetCacheClient(),
                                    base.RequestContext,
                                    base.Request,
                                    request);
    }

    /// <summary>
    /// Response to GET request ErrorTest
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ErrorTestRequestDTO request)
    {
      return new ErrorTestService ().Get (this.GetCacheClient (),
                                          base.RequestContext,
                                          base.Request,
                                          request);
    }

    /// <summary>
    /// Response to GET request ExceptionTest
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ExceptionTestRequestDTO request)
    {
      return new ExceptionTestService ().Get (this.GetCacheClient (),
                                    base.RequestContext,
                                    base.Request,
                                    request);
    }
  }
}
#endif // NSERVICEKIT
