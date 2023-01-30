// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// Extension to report any non-conformance
  /// </summary>
  public interface INonConformanceReportExtension
    : IInitializedByMachineExtension
  {
    /// <summary>
    /// Report a non-conformance related to a cycle
    /// </summary>
    /// <param name="deliverablePiece">not null</param>
    /// <param name="operationCycle">not null</param>
    /// <param name="reason"></param>
    /// <param name="details"></param>
    void ReportCycleNonConformance (IDeliverablePiece deliverablePiece, IOperationCycle operationCycle, INonConformanceReason reason, string details);
  }
}
