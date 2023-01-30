// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NET6_0_OR_GREATER

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

namespace Lemoine.IO
{
  /// <summary>
  /// String pipe buffer
  /// </summary>
  public sealed class StringPipeBuffer
    : IDisposable
    , IAsyncDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (StringPipeBuffer).FullName);

    readonly Pipe m_pipe = new Pipe ();
    readonly Stream m_pipeReaderStream;
    int m_length = 0;
#if DEBUG_STRING_PIPE_BUFFER
    string m_debugLastString = "";
#endif // DEBUG_STRING_PIPE_BUFFER

    /// <summary>
    /// Constructor
    /// </summary>
    public StringPipeBuffer ()
    {
      m_pipeReaderStream = m_pipe.Reader.AsStream (true);
    }

    /// <summary>
    /// Add some data to the pipe
    /// </summary>
    /// <param name="span"></param>
    public void Add (ReadOnlySpan<char> span)
    {
      var bytes = span.Length * sizeof (char);
      var pipeSpan = m_pipe.Writer.GetSpan (bytes);
      var inBytes = MemoryMarshal.AsBytes (span);
      inBytes.CopyTo (pipeSpan);
      m_pipe.Writer.Advance (bytes);
      m_length += span.Length;
#if DEBUG_STRING_PIPE_BUFFER
      m_debugLastString = span.ToString ();
#endif // DEBUG_STRING_PIPE_BUFFER
    }

    /// <summary>
    /// Add some data to the pipe asynchronously
    /// </summary>
    /// <param name="s"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task AddAsync (string s, CancellationToken cancellationToken = default)
    {
      var bytes = System.Text.Encoding.Unicode.GetBytes (s);
      await m_pipe.Writer.WriteAsync (bytes.AsMemory (), cancellationToken);
      m_length += s.Length;
    }

    public async Task<FlushResult> FlushAsync (CancellationToken cancellationToken = default)
    {
      return await m_pipe.Writer.FlushAsync (cancellationToken);
    }

    /// <summary>
    /// Skip a specific number of characters
    /// </summary>
    /// <param name="numberOfCharacters"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Skip (int numberOfCharacters)
    {
      if (m_length < numberOfCharacters) {
        log.Fatal ($"Skip: number of characters {numberOfCharacters} is lesser than the pipe size {m_length}");
#if DEBUG_STRING_PIPE_BUFFER
        log.Fatal ($"Skip: last pushed string was {m_debugLastString}");
#endif // DEBUG_STRING_PIPE_BUFFER
        throw new InvalidOperationException ("Skip: try to skip more characters than present in the pipe");
      }

      try {
        Task.Run (() => m_pipe.Writer.FlushAsync ()).Wait ();
        byte[] buffer = new byte[1024];
        var remainingBytes = numberOfCharacters * sizeof (char);
        while (0 < remainingBytes) {
          var bufferCount = 1024 < remainingBytes ? 1024 : remainingBytes;
          var n = m_pipeReaderStream.Read (buffer, 0, bufferCount);
          if (0 < n) {
            remainingBytes -= n;
            var numberOfReadCharacters = n / sizeof (char);
            m_length -= n / sizeof (char);
#if DEBUG_STRING_PIPE_BUFFER
          if (0 == m_length) {
            log.Debug ($"Skip: new length is 0. Last pushed string is {m_debugLastString}");
          }
#endif // DEBUG_STRING_PIPE_BUFFER
          }
          else {
            break;
          }
        }
      }
      catch (Exception ex) {
        log.Error ($"Skip: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Skip a specific number of characters asynchronously
    /// </summary>
    public async Task SkipAsync (int numberOfCharacters, CancellationToken cancellationToken = default)
    {
      if (m_length < numberOfCharacters) {
        log.Fatal ($"SkipAsync: number of characters {numberOfCharacters} is lesser than the pipe size {m_length}");
#if DEBUG_STRING_PIPE_BUFFER
        log.Fatal ($"Skip: last pushed string was {m_debugLastString}");
#endif // DEBUG_STRING_PIPE_BUFFER
        throw new InvalidOperationException ("SkipAsync: try to skip more characters than present in the pipe");
      }

      try {
        await m_pipe.Writer.FlushAsync (cancellationToken);
        byte[] buffer = new byte[1024];
        var remainingBytes = numberOfCharacters * sizeof (char);
        cancellationToken.ThrowIfCancellationRequested ();
        while (0 < remainingBytes) {
          cancellationToken.ThrowIfCancellationRequested ();
          var bufferCount = 1024 < remainingBytes ? 1024 : remainingBytes;
          var n = await m_pipeReaderStream.ReadAsync (buffer, 0, bufferCount, cancellationToken);
          if (0 < n) {
            remainingBytes -= n;
            m_length -= n / sizeof (char);
#if DEBUG_STRING_PIPE_BUFFER
          if (0 == m_length) {
            log.Debug ($"SkipAsync: new length is 0. Last pushed string is {m_debugLastString}");
          }
#endif // DEBUG_STRING_PIPE_BUFFER
          }
          else {
            break;
          }
        }
        cancellationToken.ThrowIfCancellationRequested ();
      }
      catch (Exception ex) {
        log.Error ($"SkipAsync: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Skip the current buffer content asynchronously
    /// </summary>
    /// <returns>Number of skipped characters</returns>
    public async Task<int> SkipAsync (CancellationToken cancellationToken = default)
    {
      await m_pipe.Writer.FlushAsync (cancellationToken);
      // Note: because a stream is open, m_pipe.Reader.TryRead can't be used
      byte[] buffer = new byte[1024];
      int n;
      var skippedCharacters = 0;
      cancellationToken.ThrowIfCancellationRequested ();
      while ((0 < m_length) && (0 < (n = await m_pipeReaderStream.ReadAsync (buffer, 0, buffer.Length, cancellationToken)))) {
        cancellationToken.ThrowIfCancellationRequested ();
        var numberOfCharacters = n / sizeof (char);
        skippedCharacters += numberOfCharacters;
        m_length -= numberOfCharacters;
#if DEBUG_STRING_PIPE_BUFFER
        if (0 == m_length) {
          log.Debug ($"SkipAsync: new length is 0. Last pushed string is {m_debugLastString}");
        }
#endif // DEBUG_STRING_PIPE_BUFFER
      }
      cancellationToken.ThrowIfCancellationRequested ();
      return skippedCharacters;
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter
    /// </summary>
    /// <returns>Number of read/written characters</returns>
    public int Release (TextWriter textWriter, int numberOfCharacters)
    {
      if (m_length < numberOfCharacters) {
        log.Fatal ($"Release: number of requested characters {numberOfCharacters} is lesser than the pipe size {m_length}");
#if DEBUG_STRING_PIPE_BUFFER
        log.Fatal ($"Release: last pushed string was {m_debugLastString}");
#endif // DEBUG_STRING_PIPE_BUFFER
        throw new InvalidOperationException ("Release: try to release more characters than present in the pipe");
      }

      try {
        Task.Run (() => m_pipe.Writer.FlushAsync ()).Wait ();
        byte[] buffer = new byte[1024];
        var remainingBytes = numberOfCharacters * sizeof (char);
        var writtenCharacters = 0;
        while (0 < remainingBytes) {
          var bufferCount = 1024 < remainingBytes ? 1024 : remainingBytes;
          var n = m_pipeReaderStream.Read (buffer, 0, bufferCount);
          if (0 < n) {
            remainingBytes -= n;
            var s = System.Text.Encoding.Unicode.GetString (buffer, 0, n);
            textWriter.Write (s);
            writtenCharacters += s.Length;
            m_length -= s.Length;
#if DEBUG_STRING_PIPE_BUFFER
          if (0 == m_length) {
            log.Debug ($"Release: new length is 0. Last pushed string is {m_debugLastString}, last returned string is {s}");
          }
#endif // DEBUG_STRING_PIPE_BUFFER
          }
          else {
            break;
          }
        }
        return writtenCharacters;
      }
      catch (Exception ex) {
        log.Error ($"Release: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter asynchronously
    /// </summary>
    /// <returns>Number of read/written characters</returns>
    public async Task<int> ReleaseAsync (TextWriter textWriter, int numberOfCharacters, CancellationToken cancellationToken = default)
    {
      if (m_length < numberOfCharacters) {
        log.Fatal ($"ReleaseAsync: number of requested characters {numberOfCharacters} is lesser than the pipe size {m_length}");
#if DEBUG_STRING_PIPE_BUFFER
        log.Fatal ($"ReleaseAsync: last pushed string was {m_debugLastString}");
#endif // DEBUG_STRING_PIPE_BUFFER
        throw new InvalidOperationException ("ReleaseAsync: try to release more characters than present in the pipe");
      }

      try {
        await m_pipe.Writer.FlushAsync (cancellationToken);
        byte[] buffer = new byte[1024];
        var remainingBytes = numberOfCharacters * sizeof (char);
        var writtenCharacters = 0;
        cancellationToken.ThrowIfCancellationRequested ();
        while (0 < remainingBytes) {
          cancellationToken.ThrowIfCancellationRequested ();
          var bufferCount = 1024 < remainingBytes ? 1024 : remainingBytes;
          var n = await m_pipeReaderStream.ReadAsync (buffer, 0, bufferCount, cancellationToken);
          if (0 < n) {
            remainingBytes -= n;
            var s = System.Text.Encoding.Unicode.GetString (buffer, 0, n);
            await textWriter.WriteAsync (s);
            writtenCharacters += s.Length;
            m_length -= s.Length;
#if DEBUG_STRING_PIPE_BUFFER
          if (0 == m_length) {
            log.Debug ($"ReleaseAsync: new length is 0. Last pushed string is {m_debugLastString}, last returned string is {s}");
          }
#endif // DEBUG_STRING_PIPE_BUFFER
          }
          else {
            break;
          }
        }
        cancellationToken.ThrowIfCancellationRequested ();
        return writtenCharacters;
      }
      catch (Exception ex) {
        log.Error ($"ReleaseAsync: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter
    /// </summary>
    /// <returns>Number of read/written characters</returns>
    public int Release (TextWriter textWriter)
    {
      try {
        Task.Run (() => m_pipe.Writer.FlushAsync ()).Wait ();
        // Note: because a stream is open, m_pipe.Reader.TryRead can't be used
        byte[] buffer = new byte[1024];
        var writtenCharacters = 0;
        int n;
        while ((0 < m_length) && (0 < (n = m_pipeReaderStream.Read (buffer, 0, buffer.Length)))) {
          var s = System.Text.Encoding.Unicode.GetString (buffer, 0, n);
          textWriter.Write (s);
          writtenCharacters += s.Length;
          m_length -= s.Length;
#if DEBUG_STRING_PIPE_BUFFER
        if (0 == m_length) {
          log.Debug ($"Release: new length is 0. Last pushed string is {m_debugLastString}, last returned string is {s}");
        }
#endif // DEBUG_STRING_PIPE_BUFFER
        }
        return writtenCharacters;
      }
      catch (Exception ex) {
        log.Error ($"Release: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// Release the buffer content to write into the TextWriter asynchronously
    /// </summary>
    /// <returns>Number of read/written characters</returns>
    public async Task<int> ReleaseAsync (TextWriter textWriter, CancellationToken cancellationToken = default)
    {
      try {
        await m_pipe.Writer.FlushAsync (cancellationToken);
        // Note: because a stream is open, m_pipe.Reader.TryRead can't be used
        byte[] buffer = new byte[1024];
        var writtenCharacters = 0;
        int n;
        cancellationToken.ThrowIfCancellationRequested ();
        while ((0 < m_length) && (0 < (n = await m_pipeReaderStream.ReadAsync (buffer, 0, buffer.Length, cancellationToken)))) {
          cancellationToken.ThrowIfCancellationRequested ();
          var s = System.Text.Encoding.Unicode.GetString (buffer, 0, n);
          await textWriter.WriteAsync (s);
          writtenCharacters += s.Length;
          m_length -= s.Length;
#if DEBUG_STRING_PIPE_BUFFER
        if (0 == m_length) {
          log.Debug ($"ReleaseAsync: new length is 0. Last pushed string is {m_debugLastString}, last returned string is {s}");
        }
#endif // DEBUG_STRING_PIPE_BUFFER
        }
        cancellationToken.ThrowIfCancellationRequested ();
        return writtenCharacters;
      }
      catch (Exception ex) {
        log.Error ($"ReleaseAsync: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IDisposable"/>
    /// </summary>
    public void Dispose ()
    {
      m_pipeReaderStream.Dispose ();
    }

    /// <summary>
    /// <see cref="IAsyncDisposable"/>
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync ()
    {
      await m_pipeReaderStream.DisposeAsync ();
    }
  }
}
#endif // NET6_0_OR_GREATER
