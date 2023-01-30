// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Database;
using Lemoine.Model;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.CyclesWithRealEndFull
{
  public class OperationCycleFullExtension
    : Lemoine.Extensions.NotConfigurableExtension
    , IOperationCycleFullExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (OperationCycleFullExtension).FullName);

    public bool? IsFull (IOperationCycle operationCycle)
    {
      return operationCycle.End.HasValue
        && !operationCycle.Status.HasFlag (OperationCycleStatus.EndEstimated);
    }
  }
}
