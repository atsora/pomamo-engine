// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using NServiceKit.ServiceInterface;

namespace Pulse.Web.CncAlarm
{
  /// <summary>
  /// Services to operation
  /// </summary>
  public class CncAlarmServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request CncAlarmAt
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncAlarmAtRequestDTO request)
    {
      return new CncAlarmAtService().Get(this.GetCacheClient(),
                                         base.RequestContext,
                                         base.Request,
                                         request);
    }

    /// <summary>
    /// Response to GET request CncAlarmColor
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncAlarmColorRequestDTO request)
    {
      return new CncAlarmColorService().Get(this.GetCacheClient(),
                                            base.RequestContext,
                                            base.Request,
                                            request);
    }
    
    /// <summary>
    /// Response to GET request CncAlarmCurrent
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncAlarmCurrentRequestDTO request)
    {
      return new CncAlarmCurrentService().Get(this.GetCacheClient(),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }

    /// <summary>
    /// Response to GET request CncAlarmRedStackLight
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CncAlarmRedStackLightRequestDTO request)
    {
      return new CncAlarmRedStackLightService ().Get (this.GetCacheClient (),
                                                      base.RequestContext,
                                                      base.Request,
                                                      request);
    }
  }
}
#endif // NSERVICEKIT
