// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.Stampers
{
  /// <summary>
  /// <see cref="IStamper"/> implementation when nothing is written
  /// </summary>
  public sealed class ReadOnlyStamper
    : IStamper, IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (ReadOnlyStamper).FullName);

    readonly TextReader m_sourceReader;
    volatile bool m_completed = false;
    bool m_closed = false;

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public StamperLineFeed LineFeed { get; set; }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public TextReader Reader => m_sourceReader;

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public CancellationTokenSource ParsingCancellation { get; } = new CancellationTokenSource ();

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="stamperParametersProvider">To give this constructor a higher priority in dependency injection</param>
    public ReadOnlyStamper (IStampingFileFlow stamperFileFlow, IStamperParametersProvider? stamperParametersProvider = null)
      : this (stamperFileFlow.StamperInputFilePath)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="stamperParametersProvider"></param>
    public ReadOnlyStamper (IStamperParametersProvider stamperParametersProvider)
      : this (stamperParametersProvider.InputFilePath)
    {
    }

    /// <summary>
    /// Constructor using a file path
    /// </summary>
    /// <param name="sourceFilePath"></param>
    public ReadOnlyStamper (string sourceFilePath)
      : this (new StreamReader (sourceFilePath))
    {
    }

    /// <summary>
    /// Constructor using generic text reader
    /// </summary>
    public ReadOnlyStamper (TextReader sourceReader)
    {
      m_sourceReader = sourceReader;
    }

    /// <summary>
    /// Activate the stamper (run background tasks if required)
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync (CancellationToken cancellationToken = default)
    { 
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="line"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void AddLine (string line)
    {
      log.Error ($"AddLine: not implemented, line={line}");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Task AddLineAsync (string line)
    {
      log.Error ($"AddLineAsync: not implemented, line={line}");
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="endPosition"></param>
    public void Skip (int endPosition)
    {
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="endPosition"></param>
    public Task SkipAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="endPosition"></param>
    public Task SkipAsync (CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="endPosition"></param>
    public void Release (int endPosition)
    {
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public void Release ()
    {
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="endPosition"></param>
    /// <returns></returns>
    public Task ReleaseAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task ReleaseAsync (CancellationToken cancellationToken = default)
    {
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public void Complete ()
    {
      m_completed = true;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task CompleteAsync (CancellationToken cancellationToken = default)
    {
      m_completed = true;
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public bool Completed => m_completed;

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public void CloseAll ()
    {
      if (m_closed) {
        return;
      }
      m_sourceReader.Close ();
      m_closed = true;
    }

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      CloseAll ();
      this.ParsingCancellation.Dispose ();
    }
    #endregion // IDisposable implementation
  }
}
