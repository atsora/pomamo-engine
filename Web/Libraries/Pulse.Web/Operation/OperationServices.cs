// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Services;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Services to operation
  /// </summary>
  public class OperationServices : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request ComponentSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ComponentSlotsRequestDTO request)
    {
      return new ComponentSlotsService ().Get (this.GetCacheClient (),
                                               base.RequestContext,
                                               base.Request,
                                               request);
    }

    /// <summary>
    /// Response to GET request CurrentSequence
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CurrentSequenceRequestDTO request)
    {
      return new CurrentSequenceService ().Get (this.GetCacheClient (),
                                                base.RequestContext,
                                                base.Request,
                                                request);
    }

    /// <summary>
    /// Response to GET request CycleProgress
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CycleProgressRequestDTO request)
    {
      return new CycleProgressService ().Get (this.GetCacheClient (),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }

    /// <summary>
    /// Response to GET request OperationProgress
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (OperationProgressRequestDTO request)
    {
      return new OperationProgressService ().Get (this.GetCacheClient (),
                                                  base.RequestContext,
                                                  base.Request,
                                                  request);
    }

    /// <summary>
    /// Response to GET request /Operation/Import
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ImportRequestDTO request)
    {
      return new ImportService ().Get (base.RequestContext,
                                              base.Request,
                                              request);
    }

    /// <summary>
    /// Response to GET request JobSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (JobSlotsRequestDTO request)
    {
      return new JobSlotsService ().Get (this.GetCacheClient (),
                                         base.RequestContext,
                                         base.Request,
                                         request);
    }

    /// <summary>
    /// Response to GET request MissingWorkInformation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (MissingWorkInformationRequestDTO request)
    {
      return new MissingWorkInformationService ().Get (this.GetCacheClient (),
                                                      base.RequestContext,
                                                      base.Request,
                                                      request);
    }

    /// <summary>
    /// Response to GET request OperationSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (OperationSlotsRequestDTO request)
    {
      return new OperationSlotsService ().Get (this.GetCacheClient (),
                                              base.RequestContext,
                                              base.Request,
                                              request);
    }

    /// <summary>
    /// Response to GET request Operation/PartProductionRange
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (PartProductionRangeRequestDTO request)
    {
      return new NServiceKitCachedService<PartProductionRangeRequestDTO> (new PartProductionRangeService ())
        .Get (this.GetCacheClient (),
              base.RequestContext,
              base.Request,
              request);
    }

    /// <summary>
    /// Response to GET request ProductionMachiningStatus
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (ProductionMachiningStatusRequestDTO request)
    {
      return new ProductionMachiningStatusService ().Get (this.GetCacheClient (),
                                                          base.RequestContext,
                                                          base.Request,
                                                          request);
    }

    /// <summary>
    /// Response to GET request SequenceSlots
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (SequenceSlotsRequestDTO request)
    {
      return new SequenceSlotsService ().Get (this.GetCacheClient (),
                                             base.RequestContext,
                                             base.Request,
                                             request);
    }
  }
}
#endif // NSERVICEKIT
