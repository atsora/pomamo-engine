// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;

namespace Pulse.Web.MachineMode
{
  /// <summary>
  /// Services to machine modes
  /// </summary>
  public class MachineModeServices: NServiceKit.ServiceInterface.Service

  {
    /// <summary>
    /// Response to GET request CurrentMachineMode
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CurrentMachineModeRequestDTO request)
    {
      return new CurrentMachineModeService().Get (this.GetCacheClient(),
                                                  base.RequestContext,
                                                  base.Request,
                                                  request);
    }

    /// <summary>
    /// Response to GET request MachineModeCategoryLegend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MachineModeCategoryLegendRequestDTO request)
    {
      return new MachineModeCategoryLegendService().Get (this.GetCacheClient(),
                                                         base.RequestContext,
                                                         base.Request,
                                                         request);
    }

    /// <summary>
    /// Response to GET request MachineModeColorLegend
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MachineModeColorLegendRequestDTO request)
    {
      return new MachineModeColorLegendService().Get (this.GetCacheClient(),
                                                      base.RequestContext,
                                                      base.Request,
                                                      request);
    }

    /// <summary>
    /// Response to GET request MachineModeColorSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MachineModeColorSlotsRequestDTO request)
    {
      return new MachineModeColorSlotsService().Get (this.GetCacheClient(),
                                                     base.RequestContext,
                                                     base.Request,
                                                     request);
    }

    /// <summary>
    /// Response to GET request RunningSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (RunningSlotsRequestDTO request)
    {
      return new RunningSlotsService().Get (this.GetCacheClient(),
                                            base.RequestContext,
                                            base.Request,
                                            request);
    }

    /// <summary>
    /// Response to GET request Utilization
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (UtilizationRequestDTO request)
    {
      return new UtilizationService().Get (this.GetCacheClient(),
                                           base.RequestContext,
                                           base.Request,
                                           request);
    }

    /// <summary>
    /// Response to GET request UtilizationTarget
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (UtilizationTargetRequestDTO request)
    {
      return new UtilizationTargetService().Get (this.GetCacheClient(),
                                                 base.RequestContext,
                                                 base.Request,
                                                 request);
    }
  }
}
#endif // NSERVICEKIT
