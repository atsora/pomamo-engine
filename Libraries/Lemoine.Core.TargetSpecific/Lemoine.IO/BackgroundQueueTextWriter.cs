// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NET6_0_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.IO
{
  /// <summary>
  /// Text writer that sends the write actions to a <see cref="TaskBackgroundQueue"/>
  /// </summary>
  public class BackgroundQueueTextWriter
    : TextWriter
  {
    readonly ILog log = LogManager.GetLogger (typeof (BackgroundQueueTextWriter).FullName);

    readonly TextWriter m_textWriter;
    readonly TaskBackgroundQueue m_queue;

    /// <summary>
    /// Constructor
    /// </summary>
    public BackgroundQueueTextWriter (TextWriter textWriter, TaskBackgroundQueue queue)
    {
      m_textWriter = textWriter;
      m_queue = queue;
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    public override Encoding Encoding => m_textWriter.Encoding;

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    /// <param name="value"></param>
    public override void Write (char value)
    {
      m_queue.Add (() => m_textWriter.WriteAsync (value));
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    public override void Write (char[] buffer, int index, int count)
    {
      m_queue.Add (() => m_textWriter.WriteAsync (buffer, index, count));
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    /// <param name="value"></param>
    public override void Write (string value)
    {
      m_queue.Add (() => m_textWriter.WriteAsync (value));
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    public override void WriteLine ()
    {
      m_queue.Add (() => m_textWriter.WriteLineAsync ());
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override Task WriteAsync (char value)
    {
      m_queue.Add (() => m_textWriter.WriteAsync (value));
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="index"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public override Task WriteAsync (char[] buffer, int index, int count)
    {
      m_queue.Add (() => m_textWriter.WriteAsync (buffer, index, count));
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public override Task WriteAsync (string value)
    {
      m_queue.Add (() => m_textWriter.WriteAsync (value));
      return Task.CompletedTask;
    }

    /// <summary>
    /// <see cref="TextWriter"/>
    /// </summary>
    /// <returns></returns>
    public override Task WriteLineAsync ()
    {
      m_queue.Add (() => m_textWriter.WriteLineAsync ());
      return Task.CompletedTask;
    }
  }
}
#endif // NET6_0_OR_GREATER
