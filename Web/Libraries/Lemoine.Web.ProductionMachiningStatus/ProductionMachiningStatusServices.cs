// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;
using NServiceKit.ServiceInterface;
using Lemoine.Web.ProductionMachiningStatus.CurrentMachineStateTemplateOperation;

namespace Lemoine.Web.ProductionMachiningStatus
{
  /// <summary>
  /// Description of ProductionMachiningStatusServices.
  /// </summary>
  public class ProductionMachiningStatusServices: NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// Response to GET request GetCurrentMachineStateTemplateOperation
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (CurrentMachineStateTemplateOperationRequestDTO request)
    {
      return new CurrentMachineStateTemplateOperationService().Get (this.GetCacheClient(),
                                                                    base.RequestContext,
                                                                    base.Request,
                                                                    request);
    }
  }
}
#endif // NSERVICEKIT
