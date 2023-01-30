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
  /// StringToStringStamper
  /// </summary>
  public class StringToStringStamper
    : IStamper
    , IDisposable
  {
    readonly ILog log = LogManager.GetLogger (typeof (StringToStringStamper).FullName);

    readonly TextReader m_reader;
    readonly TextWriter m_writer;
    readonly Stamper m_readerWriter;

    /// <summary>
    /// Output string
    /// </summary>
    public string Output => m_writer?.ToString () ?? "";

    /// <summary>
    /// Constructor
    /// </summary>
    public StringToStringStamper (string input)
    {
      m_reader = new StringReader (input);
      m_writer = new StringWriter ();
      m_readerWriter = new Stamper (m_reader, m_writer);
    }

    public CancellationTokenSource ParsingCancellation => m_readerWriter.ParsingCancellation;

    /// <summary>
    /// <see cref="IStamper"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.StartAsync (cancellationToken);
    }

    public StamperLineFeed LineFeed
    {
      get => m_readerWriter.LineFeed;
      set {
        m_readerWriter.LineFeed = value;
      }
    }

    public TextReader Reader => m_readerWriter;

    public void AddLine (string line)
    {
      m_readerWriter.AddLine (line);
    }

    public async Task AddLineAsync (string line)
    {
      await m_readerWriter.AddLineAsync (line);
    }

    public void Skip (int endPosition)
    {
      m_readerWriter.Skip (endPosition);
      m_writer.Flush ();
    }

    public async Task SkipAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      await m_readerWriter.SkipAsync (endPosition, cancellationToken);
      await m_writer.FlushAsync ();
    }

    public async Task SkipAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.SkipAsync (cancellationToken);
      await m_writer.FlushAsync ();
    }

    public void Release (int endPosition)
    {
      m_readerWriter.Release (endPosition);
      m_writer.Flush ();
    }

    public async Task ReleaseAsync (int endPosition, CancellationToken cancellationToken = default)
    {
      await m_readerWriter.ReleaseAsync (endPosition, cancellationToken);
      await m_writer.FlushAsync ();
    }

    public void Release ()
    {
      m_readerWriter.Release ();
      m_writer.Flush ();
    }

    public async Task ReleaseAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.ReleaseAsync (cancellationToken);
      await m_writer.FlushAsync ();
    }

    public void Complete ()
    {
      m_readerWriter.Complete ();
    }

    public async Task CompleteAsync (CancellationToken cancellationToken = default)
    {
      await m_readerWriter.CompleteAsync (cancellationToken);
    }

    public bool Completed => m_readerWriter.Completed;

    public void CloseAll ()
    {
      m_writer.Flush ();

      m_readerWriter.Dispose ();
      m_writer.Dispose ();
      m_reader.Dispose ();
    }

    public void Dispose ()
    {
      CloseAll ();
    }
  }
}
