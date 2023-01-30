// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;

using Lemoine.Core.Log;

namespace Lem_LoopingService
{
  /// <summary>
  /// Main class of service Lem_LoopingService
  /// </summary>
  public sealed class LoopingService
    : Lemoine.Threading.IThreadService, IDisposable
  {
    #region Members
    readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource ();
    bool m_disposed = false;
    #endregion

    static readonly ILog log = LogManager.GetLogger(typeof (LoopingService).FullName);
    
    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public LoopingService()
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// <see cref="IThreadService"/>
    /// </summary>
    public void Initialize ()
    {
      InitializeThreads (CancellationToken.None);
    }

    void InitializeThreads (CancellationToken cancellationToken)
    {
      using (var linkedCancellationTokenSource = CancellationTokenSource
        .CreateLinkedTokenSource (m_cancellationTokenSource.Token, cancellationToken)) {
        var linkedToken = linkedCancellationTokenSource.Token;

        while (!linkedToken.IsCancellationRequested) {
          log.Debug ("Loop: one more time");
          linkedToken.WaitHandle.WaitOne (TimeSpan.FromSeconds (10));
        }

      }
    }

    /// <summary>
    /// Stop this service.
    /// </summary>
    public void OnStop()
    {
      m_cancellationTokenSource?.Cancel ();
    }
    #endregion // Methods

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    ~LoopingService () => Dispose (false);

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      Dispose (true);
      GC.SuppressFinalize (this);
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    /// <param name="disposing"></param>
    void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }

      if (disposing) {
        // Dispose here the managed classes
      }

      m_disposed = true;
    }
    #endregion // IDisposable implementation
  }
}
