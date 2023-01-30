// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.MachineStateTemplate
{
  /// <summary>
  /// Services to machine state templates
  /// </summary>
  public class MachineStateTemplateServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request NextMachineStateTemplate
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (NextMachineStateTemplateRequestDTO request)
    {
      return new NextMachineStateTemplateService().Get (this.GetCacheClient(),
                                                        base.RequestContext,
                                                        base.Request,
                                                        request);
    }
    
    /// <summary>
    /// Response to GET request MachineStateTemplates
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MachineStateTemplatesRequestDTO request)
    {
      return new MachineStateTemplatesService().Get (this.GetCacheClient(),
                                                     base.RequestContext,
                                                     base.Request,
                                                     request);
    }

    /// <summary>
    /// Response to GET request MachineStateTemplateSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MachineStateTemplateSlotsRequestDTO request)
    {
      return new MachineStateTemplateSlotsService().Get (this.GetCacheClient(),
                                                         base.RequestContext,
                                                         base.Request,
                                                         request);
    }

    /// <summary>
    /// Response to GET request ObservationStateSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ObservationStateSlotsRequestDTO request)
    {
      return new ObservationStateSlotsService().Get (this.GetCacheClient(),
                                                     base.RequestContext,
                                                     base.Request,
                                                     request);
    }
  }
}
#endif // NSERVICEKIT
