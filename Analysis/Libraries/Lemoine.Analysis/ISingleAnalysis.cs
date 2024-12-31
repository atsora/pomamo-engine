// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Interface for a single analysis
  /// </summary>
  public interface ISingleAnalysis
  {
    /// <summary>
    /// Initalization
    /// </summary>
    void Initialize ();

    /// <summary>
    /// Run the analysis once
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="maxDateTime"></param>
    /// <param name="minTime">per sub-analysis minimum time (for example per machine module)</param>
    /// <param name="numberOfItems">Number of items to consider if applicable. If null, consider a configuration key</param>
    /// <returns>if false, interrupt the state machine</returns>
    bool RunOnce (CancellationToken cancellationToken, DateTime maxDateTime, TimeSpan minTime, int? numberOfItems = null);
  }
}
