// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// DataSetter
  /// </summary>
  public class LogEvents
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (LogEvents).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LogEvents (IStamper stamper)
    {
      this.Stamper = stamper;
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStampingEventHandler? Next { get; set; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public IStamper Stamper { get; }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void NotifyNewBlock (bool edit, int level)
    {
      if (log.IsTraceEnabled) {
        log.Trace ($"NotifyNewBlock: edit={edit} level={level}");
      }
      this.Next?.NotifyNewBlock (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="comment"></param>
    public void SetComment (string comment)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"SetComment: {comment}");
      }
      this.Next?.SetComment (comment);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void SetData (string key, object v)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"SetData: {key}={v}");
      }
      this.Next?.SetData (key, v);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="duration"></param>
    public void SetMachiningTime (TimeSpan duration)
    {
      if (log.IsTraceEnabled) {
        log.Trace ($"SetMachiningTime: {duration}");
      }
      this.Next?.SetMachiningTime (duration);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="toolNumber"></param>
    public void SetNextToolNumber (string toolNumber)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"SetNextToolNumber {toolNumber}");
      }
      this.Next?.SetNextToolNumber (toolNumber);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartCycle ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"StartCycle");
      }
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"StartSequence: kind={sequenceKind}");
      }
      this.Next?.StartSequence (sequenceKind);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"StopCycle");
      }
      this.Next?.StopCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartProgram (bool edit, int level)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"StartProgram: edit={edit} level={level}");
      }
      this.Next?.StartProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void EndProgram (bool edit, int level, bool endOfFile)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"EndProgram: edit={edit} level={level} endOfFile={endOfFile}");
      }
      this.Next?.EndProgram (edit, level, endOfFile);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void ResumeProgram (bool edit, int level)
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"ResumeProgram: edit={edit} level={level}");
      }
      this.Next?.ResumeProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SuspendProgram (bool optional = false, string details = "")
    {
      this.Next?.SuspendProgram (optional, details);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerMachining ()
    {
      if (log.IsTraceEnabled) {
        log.Trace ($"TriggerMachining");
      }
      this.Next?.TriggerMachining ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="toolNumber"></param>
    public void TriggerToolChange (string toolNumber = "")
    {
      if (log.IsDebugEnabled) {
        log.Debug ($"TriggerToolChange: {toolNumber}");
      }
      this.Next?.TriggerToolChange (toolNumber);
    }

  }
}
