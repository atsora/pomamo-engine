// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.Misc
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
    public object Get (SwitchCatchUpModeRequestDTO request)
    {
      return new SwitchCatchUpModeService ().Get (base.RequestContext,
                                                  base.Request,
                                                  request);
    }
  }
}
#endif // NSERVICEKIT
