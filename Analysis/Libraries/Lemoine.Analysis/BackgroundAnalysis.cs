// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;
using System.Threading;
using Lemoine.Core.Log;
using Lemoine.Extensions.Analysis;
using Lemoine.Threading;

namespace Lemoine.Analysis
{
  /// <summary>
  /// Thread of an <see cref="IBackgroundAnalysisExtension"/>: it runs the extension in a loop,
  /// independently of the global analysis and of the analysis of the machines
  /// </summary>
  public sealed class BackgroundAnalysis : ThreadClass
  {
    readonly ILog log;
    readonly IBackgroundAnalysisExtension m_extension;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="extension">not null, initialized</param>
    public BackgroundAnalysis (IBackgroundAnalysisExtension extension)
    {
      Debug.Assert (null != extension);

      m_extension = extension;
      log = LogManager.GetLogger ($"{typeof (BackgroundAnalysis).FullName}.{extension.GetType ().Name}");
    }

    /// <summary>
    /// <see cref="ThreadClass"/>
    /// </summary>
    /// <returns></returns>
    public override ILog GetLogger () => log;

    /// <summary>
    /// <see cref="ThreadClass"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    protected override void Run (CancellationToken cancellationToken)
    {
      while (!cancellationToken.IsCancellationRequested) {
        SetActive ();
        try {
          m_extension.RunOnce (cancellationToken);
        }
        catch (OperationCanceledException) {
          throw;
        }
        catch (Exception ex) {
          log.Error ("Run: exception in the background extension, try again after the pause", ex);
        }
        SetActive ();
        this.Sleep (m_extension.Frequency, cancellationToken);
      }
    }
  }
}
