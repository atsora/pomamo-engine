// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.IO;
using Lemoine.Threading;

namespace Lemoine.Stamping.Stampers
{
  /// <summary>
  /// <see cref="IStamper"/> implementation using a <see cref="BackgroundQueueTextWriter"/> to write the data
  /// </summary>
  public sealed class QueueStamper
    : TextReader
    , IStamper
    , IDisposable
    , IAsyncDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (Stamper).FullName);

    bool m_disposed = false;
    readonly TextReader m_sourceReader;
    readonly TextWriter m_destinationWriter;
    readonly TaskBackgroundQueue m_queue = new TaskBackgroundQueue ();
    readonly TextWriter m_queueWriter;
    int m_currentPosition = 0;
    StringPipeBuffer m_pipeBuffer = new StringPipeBuffer ();
    volatile bool m_completed = false;
    bool m_allClosed = false;

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public StamperLineFeed LineFeed { get; set; }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public TextReader Reader => this;

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public CancellationTokenSource ParsingCancellation { get; } = new CancellationTokenSource ();

    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="stamperParametersProvider">To give this constructor a higher priority in dependency injection</param>
    public QueueStamper (IStampingFileFlow stamperFileFlow, IStamperParametersProvider? stamperParametersProvider = null)
      : this (stamperFileFlow.StamperInputFilePath, stamperFileFlow.StamperOutputFilePath)
    {
    }

    /// <summary>
    /// Alternative constructor
    /// </summary>
    /// <param name="stamperParametersProvider"></param>
    public QueueStamper (IStamperParametersProvider stamperParametersProvider)
      : this (stamperParametersProvider.InputFilePath, stamperParametersProvider.OutputFilePath)
    {
    }

    /// <summary>
    /// Constructor using file paths
    /// </summary>
    /// <param name="sourceFilePath"></param>
    /// <param name="destinationFilePath"></param>
    public QueueStamper (string sourceFilePath, string destinationFilePath)
      : this (new StreamReader (sourceFilePath), new StreamWriter (destinationFilePath))
    {
    }

    /// <summary>
    /// Constructor using generic text reader/writer
    /// </summary>
    public QueueStamper (TextReader sourceReader, TextWriter destinationWriter)
    {
      m_sourceReader = sourceReader;
      m_destinationWriter = destinationWriter;
      m_queueWriter = new BackgroundQueueTextWriter (m_destinationWriter, m_queue);
    }

    /// <summary>
    /// Activate the stamper (run background tasks if required)
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync (CancellationToken cancellationToken = default)
    {
      await m_queue.StartAsync (cancellationToken);
    }

    #region TextReader implementation
    /// <summary>
    /// Returns the next available character without actually reading it from
    /// the input stream. The current position of the TextReader is not changed by
    /// this operation. The returned value is -1 if no further characters are
    /// available.
    /// </summary>
    /// <returns></returns>
    public override int Peek ()
    {
      var c = m_sourceReader.Peek ();
      return c;
    }

    void AddToPipe (ReadOnlySpan<char> span)
    {
      m_pipeBuffer.Add (span);
    }

    async Task AddToPipeAsync (string s, CancellationToken cancellationToken = default)
    {
      await m_pipeBuffer.AddAsync (s, cancellationToken);
    }

    /// <summary>
    /// Reads the next character from the input stream. The returned value is
    /// -1 if no further characters are available.
    /// </summary>
    /// <returns></returns>
    public override int Read ()
    {
      var c = m_sourceReader.Read ();
      if (-1 != c) {
        char[] array = new char[1] { (char)c };
        AddToPipe ((Span<char>)array);
      }
      return c;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public override int Read (Span<char> buffer)
    {
      var n = m_sourceReader.Read (buffer);
      if (0 < n) {
        AddToPipe (buffer);
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override int Read (char[] buffer, int index, int count)
    {
      var n = m_sourceReader.Read (buffer, index, count);
      if (0 < n) {
        var bufferMemory = MemoryMarshal.CreateFromPinnedArray (buffer, index, n);
        AddToPipe (bufferMemory.Span);
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override async Task<int> ReadAsync (char[] buffer, int index, int count)
    {
      var n = await m_sourceReader.ReadAsync (buffer, index, count);
      if (0 < n) {
        var bufferMemory = MemoryMarshal.CreateFromPinnedArray (buffer, index, n);
        await AddToPipeAsync (bufferMemory.ToString ());
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async ValueTask<int> ReadAsync (Memory<char> buffer, CancellationToken cancellationToken = default)
    {
      var n = await m_sourceReader.ReadAsync (buffer, cancellationToken);
      if (0 < n) {
        await AddToPipeAsync (buffer.ToString (), cancellationToken);
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <returns></returns>
    public override int ReadBlock (Span<char> buffer)
    {
      var n = m_sourceReader.ReadBlock (buffer);
      if (0 < n) {
        AddToPipe (buffer);
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override int ReadBlock (char[] buffer, int index, int count)
    {
      var n = m_sourceReader.ReadBlock (buffer, index, count);
      if (0 < n) {
        var bufferMemory = MemoryMarshal.CreateFromPinnedArray (buffer, index, n);
        AddToPipe (bufferMemory.Span);
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async ValueTask<int> ReadBlockAsync (Memory<char> buffer, CancellationToken cancellationToken = default)
    {
      var n = await m_sourceReader.ReadBlockAsync (buffer, cancellationToken);
      if (0 < n) {
        await AddToPipeAsync (buffer.ToString (), cancellationToken);
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override async Task<int> ReadBlockAsync (char[] buffer, int index, int count)
    {
      var n = await m_sourceReader.ReadBlockAsync (buffer, index, count);
      if (0 < n) {
        var bufferMemory = MemoryMarshal.CreateFromPinnedArray (buffer, index, n);
        await AddToPipeAsync (bufferMemory.ToString ());
      }
      return n;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <returns></returns>
    public override string? ReadLine ()
    {
      var line = m_sourceReader.ReadLine ();
      if (!string.IsNullOrEmpty (line)) {
        AddToPipe (line.AsSpan ());
        AddToPipe (System.Environment.NewLine.AsSpan ());
      }
      return line;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <returns></returns>
    public override async Task<string?> ReadLineAsync ()
    {
      var line = await m_sourceReader.ReadLineAsync ();
      if (!string.IsNullOrEmpty (line)) {
        await AddToPipeAsync (line);
        await AddToPipeAsync (System.Environment.NewLine);
      }
      return line;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <returns></returns>
    public override string ReadToEnd ()
    {
      var s = m_sourceReader.ReadToEnd ();
      if (!string.IsNullOrEmpty (s)) {
        AddToPipe (s.AsSpan ());
      }
      return s;
    }

    /// <summary>
    /// <see cref="TextReader"/>
    /// </summary>
    /// <returns></returns>
    public override async Task<string> ReadToEndAsync ()
    {
      var s = await m_sourceReader.ReadToEndAsync ();
      if (!string.IsNullOrEmpty (s)) {
        await AddToPipeAsync (s);
      }
      return s;
    }
    #endregion // TextReader implementation

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public void Skip (int endPosition)
    {
      var numberOfCharacters = endPosition - m_currentPosition;
      m_pipeBuffer.Skip (numberOfCharacters);
      m_currentPosition = endPosition;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public async Task SkipAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      var numberOfCharacters = endPosition - m_currentPosition;
      await m_pipeBuffer.SkipAsync (numberOfCharacters, cancellationToken);
      m_currentPosition = endPosition;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public async Task SkipAsync (CancellationToken cancellationToken = default)
    {
      var skippedCharacters = await m_pipeBuffer.SkipAsync (cancellationToken);
      m_currentPosition += skippedCharacters;
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter
    /// </summary>
    public void Release (int endPosition)
    {
      var numberOfCharacters = endPosition - m_currentPosition;
      m_pipeBuffer.Release (m_queueWriter, numberOfCharacters);
      m_currentPosition = endPosition;
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter asynchronously
    /// </summary>
    public async Task ReleaseAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      var numberOfCharacters = endPosition - m_currentPosition;
      await m_pipeBuffer.ReleaseAsync (m_queueWriter, numberOfCharacters, cancellationToken);
      m_currentPosition = endPosition;
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter
    /// </summary>
    public void Release ()
    {
      var writtenCharacters = m_pipeBuffer.Release (m_queueWriter);
      m_currentPosition += writtenCharacters;
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter asynchronously
    /// </summary>
    public async Task ReleaseAsync (CancellationToken cancellationToken = default)
    {
      var writtenCharacters = await m_pipeBuffer.ReleaseAsync (m_queueWriter);
      m_currentPosition += writtenCharacters;
    }

    /// <summary>
    /// Add a line
    /// </summary>
    /// <param name="line"></param>
    public void AddLine (string line)
    {
      if (this.LineFeed.HasFlag (StamperLineFeed.Before)) {
        m_queueWriter.WriteLine ();
      }
      m_queueWriter.Write (line);
      if (this.LineFeed.HasFlag (StamperLineFeed.After)) {
        m_queueWriter.WriteLine ();
      }
    }

    /// <summary>
    /// Add a line asynchronously
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public async Task AddLineAsync (string line)
    {
      if (this.LineFeed.HasFlag (StamperLineFeed.Before)) {
        await m_queueWriter.WriteLineAsync ();
      }
      await m_queueWriter.WriteAsync (line);
      if (this.LineFeed.HasFlag (StamperLineFeed.After)) {
        await m_queueWriter.WriteLineAsync ();
      }
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    public void Complete ()
    {
      Release ();
      m_queue.Complete ();
      m_queue.WaitCompletion ();

      // For now, writing the end of the file is completed without using the queue
      char[] buffer = new char[0x1000];
      int numRead;
      while ((numRead = m_sourceReader.Read (buffer, 0, buffer.Length)) != 0) {
        m_destinationWriter.Write (buffer, 0, numRead);
      }
      m_destinationWriter.Flush ();
      m_completed = true;
    }

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task CompleteAsync (CancellationToken cancellationToken = default)
    {
      await ReleaseAsync (cancellationToken);
      m_queue.Complete ();
      await m_queue.WaitCompletionAsync (cancellationToken);

      // For now, writing the end of the file is completed without using the queue
      char[] buffer = new char[0x1000];
      int numRead;
      while ((numRead = await m_sourceReader.ReadAsync (buffer, 0, buffer.Length)) != 0) {
        await m_destinationWriter.WriteAsync (buffer, 0, numRead);
        cancellationToken.ThrowIfCancellationRequested ();
      }
      await m_destinationWriter.FlushAsync ();
      m_completed = true;
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
      if (m_allClosed) {
        return;
      }

      bool allSuccess = true;
      try {
        m_queueWriter.Dispose ();
      }
      catch (Exception ex) {
        log.Error ($"CloseAll: exception disposing the queue writer", ex);
        allSuccess = false;
      }
      try {
        m_pipeBuffer.Dispose ();
      }
      catch (Exception ex) {
        log.Error ($"CloseAll: exception disposing the pipe buffer", ex);
        allSuccess = false;
      }
      try {
        m_sourceReader.Close ();
      }
      catch (Exception ex) {
        log.Error ($"CloseAll: exception closing the source reader", ex);
        allSuccess = false;
      }
      try {
        m_destinationWriter.Close ();
      }
      catch (Exception ex) {
        log.Error ($"CloseAll: exception closing the destination writer", ex);
        allSuccess = false;
      }
      try {
        m_queue.Dispose ();
      }
      catch (Exception ex) {
        log.Error ($"CloseAll: exception disposing the queue", ex);
        allSuccess = false;
      }
      if (allSuccess) {
        m_allClosed = true;
      }
    }

    #region IDisposable implementation
    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose (bool disposing)
    {
      if (m_disposed) {
        return;
      }
      m_disposed = true;

      try {
        if (disposing) {
          CloseAll ();
          this.ParsingCancellation.Dispose ();
        }
      }
      finally {
        base.Dispose (disposing);
      }
    }
    #endregion // IDisposable implementation

    #region // IAsyncDisposable implementation
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync ()
    {
      try {
        if (!m_allClosed) {
          await m_queueWriter.DisposeAsync ();
          await m_pipeBuffer.DisposeAsync ();
          m_sourceReader.Close ();
          m_destinationWriter.Close ();
          m_queue.Dispose ();
        }
        this.ParsingCancellation.Dispose ();
      }
      finally {
        Dispose (false);
      }
      GC.SuppressFinalize (this);
    }
    #endregion // IAsyncDisposable implementation

  }
}
