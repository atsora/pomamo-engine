// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NSERVICEKIT
using System;

using NServiceKit.ServiceInterface;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Services;

namespace Pulse.Web.OperationCycle
{
  /// <summary>
  /// Services to operation
  /// </summary>
  public class OperationCycleServices : NServiceKit.ServiceInterface.Service
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (OperationCycleAtRequestDTO request)
    {
      return new OperationCycleAtService ().GetWithoutCache (request);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public object Get (OperationCycleSlotsRequestDTO request)
    {
      return new OperationCycleSlotsService ().GetWithoutCache (request);
    }
  }
}
#endif // NSERVICEKIT
