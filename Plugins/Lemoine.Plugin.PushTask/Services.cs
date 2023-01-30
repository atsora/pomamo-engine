// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using Lemoine.Extensions.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lemoine.Plugin.PushTask
{
  /// <summary>
  /// Note: this class can be removed once NServiceKit is removed
  /// </summary>
  public class Services
    : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request for EmailSend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (PushTaskRequestDTO request)
    {
      return new NServiceKitSaveService<PushTaskRequestDTO> (new PushTaskService ())
        .Get (base.RequestContext, base.Request, request);
    }
  }
}
#endif // NSERVICEKIT
