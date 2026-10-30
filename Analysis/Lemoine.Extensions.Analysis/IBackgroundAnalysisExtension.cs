// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;

namespace Lemoine.Extensions.Analysis
{
  /// <summary>
  /// Extension that runs in a background thread of the analysis service, with its own loop
  ///
  /// It is not synchronized with the global analysis nor with the analysis of the machines:
  /// it is made for slow processes that must not delay them (calls to a LLM, for example).
  /// It must only write data that the other analyses do not write
  /// </summary>
  public interface IBackgroundAnalysisExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Initialize the extension
    /// </summary>
    /// <returns>false: the extension is not active, no thread is started for it</returns>
    bool Initialize ();

    /// <summary>
    /// Pause between two runs
    /// </summary>
    TimeSpan Frequency { get; }

    /// <summary>
    /// Run once
    /// </summary>
    /// <param name="cancellationToken"></param>
    void RunOnce (CancellationToken cancellationToken);
  }
}
