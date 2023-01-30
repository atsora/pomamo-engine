// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Lemoine.Web.Misc
{
  /// <summary>
  /// Miscellaneous Services
  /// </summary>
  public class MiscServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request for EmailSend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get(EmailSendRequestDTO request)
    {
      return new EmailSendService().Get (base.RequestContext,
                                         base.Request,
                                         request);
    }
    
    /// <summary>
    /// Response to POST request EmailSend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Post (EmailSendPostRequestDTO request)
    {
      return new EmailSendService ().Post (request,
                                           this.Request);
    }
  }
}
#endif // NSERVICEKIT
