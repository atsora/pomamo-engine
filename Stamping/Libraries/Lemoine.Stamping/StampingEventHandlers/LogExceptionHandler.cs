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
  /// ExceptionHandler
  /// </summary>
  public class LogExceptionHandler
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (LogExceptionHandler).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LogExceptionHandler (IStamper stamper)
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
      try {
        this.Next?.NotifyNewBlock (edit, level);
      }
      catch (Exception ex) {
        log.Fatal ($"NotifyNewBlock: exception, edit={edit} level={level}", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetComment (string comment)
    {
      try {
        this.Next?.SetComment (comment);
      }
      catch (Exception ex) {
        log.Fatal ($"SetComment: comment={comment}, exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
    public void SetData (string key, object v)
    {
      try {
        this.Next?.SetData (key, v);
      }
      catch (Exception ex) {
        log.Fatal ($"SetData: {key}={v}, exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="duration"></param>
    public void SetMachiningTime (TimeSpan duration)
    {
      try {
        this.Next?.SetMachiningTime (duration);
      }
      catch (Exception ex) {
        log.Fatal ($"SetMachiningTime: duration={duration}, exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="toolNumber"></param>
    public void SetNextToolNumber (string toolNumber)
    {
      try {
        this.Next?.SetNextToolNumber (toolNumber);
      }
      catch (Exception ex) {
        log.Fatal ($"SetNextToolNumber: toolNumber={toolNumber}, exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartCycle ()
    {
      try {
        this.Next?.StartCycle ();
      }
      catch (Exception ex) {
        log.Fatal ("StartCycle: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      try {
        this.Next?.StartSequence (sequenceKind);
      }
      catch (Exception ex) {
        log.Fatal ($"StartSequence: sequenceKind={sequenceKind} exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      try {
        this.Next?.StopCycle ();
      }
      catch (Exception ex) {
        log.Fatal ("StopCycle: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartProgram (bool edit, int level)
    {
      try {
        this.Next?.StartProgram (edit, level);
      }
      catch (Exception ex) {
        log.Fatal ($"StartProgram: exception, edit={edit} level={level}", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void EndProgram (bool edit, int level, bool endOfFile)
    {
      try {
        this.Next?.EndProgram (edit, level, endOfFile);
      }
      catch (Exception ex) {
        log.Fatal ($"EndProgram: exception, edit={edit} level={level} bool={endOfFile}", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void ResumeProgram (bool edit, int level)
    {
      try {
        this.Next?.ResumeProgram (edit, level);
      }
      catch (Exception ex) {
        log.Fatal ($"ResumeProgram: exception, edit={edit} level={level}", ex);
        throw;
      }
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
      try {
        this.Next?.TriggerMachining ();
      }
      catch (Exception ex) {
        log.Fatal ("TriggerMachining: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="toolNumber"></param>
    public void TriggerToolChange (string toolNumber = "")
    {
      try {
        this.Next?.TriggerToolChange (toolNumber);
      }
      catch (Exception ex) {
        log.Fatal ($"TriggerToolChange: toolNumber={toolNumber}, exception", ex);
        throw;
      }
    }

  }
}
