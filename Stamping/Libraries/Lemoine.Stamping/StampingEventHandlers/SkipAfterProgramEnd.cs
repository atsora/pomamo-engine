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
  /// Implementation of a <see cref="IStampingEventHandler"/>
  /// </summary>
  public class SkipAfterProgramEnd
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (SkipAfterProgramEnd).FullName);

    int m_programLevel = 0;
    int m_activeProgramLevel = 0;

    /// <summary>
    /// Constructor
    /// </summary>
    public SkipAfterProgramEnd (IStamper stamper)
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
      if (IsActive ()) {
        this.Next?.NotifyNewBlock (edit, level);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetComment (string message)
    {
      if (IsActive ()) {
        this.Next?.SetComment (message);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetData (string key, object v)
    {
      if (IsActive ()) {
        this.Next?.SetData (key, v);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetMachiningTime (TimeSpan duration)
    {
      if (IsActive ()) {
        this.Next?.SetMachiningTime (duration);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetNextToolNumber (string toolNumber)
    {
      if (IsActive ()) {
        this.Next?.SetNextToolNumber (toolNumber);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartCycle ()
    {
      if (IsActive ()) {
        this.Next?.StartCycle ();
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      if (IsActive ()) {
        this.Next?.StartSequence (sequenceKind);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      if (IsActive ()) {
        this.Next?.StopCycle ();
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartProgram (bool edit, int level)
    {
      if (IsActive ()) {
        this.Next?.StartProgram (edit, level);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void EndProgram (bool edit, int level, bool endOfFile)
    {
      this.Next?.EndProgram (edit, level, endOfFile);
      m_activeProgramLevel = level - 1;
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="edit"></param>
    /// <param name="level"></param>
    public void ResumeProgram (bool edit, int level)
    {
      this.Next?.ResumeProgram (edit, level);
      m_programLevel = level;
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerMachining ()
    {
      if (IsActive ()) {
        this.Next?.TriggerMachining ();
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SuspendProgram (bool optional = false, string details = "")
    {
      if (IsActive ()) {
        this.Next?.SuspendProgram (optional, details);
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerToolChange (string toolNumber = "")
    {
      if (IsActive ()) {
        this.Next?.TriggerToolChange (toolNumber: toolNumber);
      }
    }

    bool IsActive ()
    {
      return m_programLevel == m_activeProgramLevel;
    }
  }
}
