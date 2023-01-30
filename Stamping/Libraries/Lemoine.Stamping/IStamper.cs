// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lemoine.Stamping
{
  /// <summary>
  /// Property whether a line feed must be inserted before or after adding a line
  /// </summary>
  [Flags]
  public enum StamperLineFeed
  { 
    /// <summary>
    /// Before the added line
    /// </summary>
    Before = 1,
    /// <summary>
    /// After the added line
    /// </summary>
    After = 2,
  }

  /// <summary>
  /// IStamper interface
  /// </summary>
  public interface IStamper: IDisposable
  {
    /// <summary>
    /// Line feed (before/after) when a new line is added
    /// </summary>
    public StamperLineFeed LineFeed { get; set; }

    /// <summary>
    /// Activate the stamper (run background tasks if required)
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync (CancellationToken cancellationToken = default);

    /// <summary>
    /// Skip a specific number of characters in the buffer content
    /// that must not be written into the TextWriter
    /// </summary>
    /// <param name="endPosition"></param>
    public void Skip (int endPosition);

    /// <summary>
    /// Skip a specific number of characters in the buffer content asynchronously
    /// that must not be written into the TextWriter
    /// </summary>
    /// <param name="endPosition"></param>
    public Task SkipAsync (int endPosition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Skip what has already been read asynchronously
    /// </summary>
    /// <param name="endPosition"></param>
    public Task SkipAsync (CancellationToken cancellationToken = default);

    /// <summary>
    /// Release the buffer content to write into the TextWriter
    /// </summary>
    /// <param name="endPosition"></param>
    public void Release (int endPosition);

    /// <summary>
    /// Release the buffer content to write into the TextWriter asynchronously
    /// </summary>
    /// <param name="endPosition"></param>
    public Task ReleaseAsync (int endPosition, CancellationToken cancellationToken = default);

    /// <summary>
    /// Release the buffer content to write into the TextWriter
    /// </summary>
    public void Release ();

    /// <summary>
    /// Release the buffer content to write into the TextWriter asynchronously
    /// </summary>
    public Task ReleaseAsync (CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a line
    /// </summary>
    /// <param name="line"></param>
    public void AddLine (string line);

    /// <summary>
    /// Add a line asynchronously
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public Task AddLineAsync (string line);

    /// <summary>
    /// Set the stamping task as completed (finish to copy the file if required)
    /// </summary>
    public void Complete ();

    /// <summary>
    /// Set the stamping task as completed asynchronously (finish to copy the file if required)
    /// </summary>
    /// <returns></returns>
    public Task CompleteAsync (CancellationToken cancellationToken = default);

    /// <summary>
    /// Is the stamping task completed ?
    /// </summary>
    public bool Completed { get; }

    /// <summary>
    /// Token source for parsing cancellation
    /// </summary>
    public CancellationTokenSource ParsingCancellation { get; }

    /// <summary>
    /// Text reader
    /// </summary>
    public System.IO.TextReader Reader { get; }

    /// <summary>
    /// Close the files or reader/writer
    /// </summary>
    public void CloseAll ();
  }
}
