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
  /// 
  /// Delay the write of any stamp when Fanuc Hpcc (High Precision Contour Control)
  /// AICC (AI Contour Control), AIAPC (AI Advanced Preview Control) or Advanced Preview Control
  /// is on
  /// </summary>
  public class FanucHpcc
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (FanucHpcc).FullName);

    double m_g05p = 0.0;
    double m_g051q = 0.0;
    double m_g08p = 0.0;
    TimeSpan m_machiningTime = TimeSpan.Zero;

    /// <summary>
    /// Constructor
    /// </summary>
    public FanucHpcc (IStamper stamper)
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
      this.Next?.NotifyNewBlock (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetComment (string message)
    {
      this.Next?.SetComment (message);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetData (string key, object v)
    {
      bool reset = false;
      switch (key) {
      case "G05P":
        reset = (0 == m_g051q + m_g08p) && (0 < m_g05p) && (0 == (double)v);
        m_g05p = (double)v;
        break;
      case "G05.1Q":
        reset = (0 == m_g05p + m_g08p) && (0 < m_g051q) && (0 == (double)v);
        m_g051q = (double)v;
        break;
      case "G08P":
        reset = (0 == m_g05p + m_g051q) && (0 < m_g08p) && (0 == (double)v);
        m_g08p = (double)v;
        break;
      }

      this.Next?.SetData (key, v);

      if (reset && (0 < m_machiningTime.Ticks)) {
        this.Next?.SetMachiningTime (m_machiningTime);
        m_machiningTime = TimeSpan.Zero;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetMachiningTime (TimeSpan duration)
    {
      m_machiningTime = m_machiningTime.Add (duration);
      if (0 == m_g05p + m_g051q + m_g08p) {
        this.Next?.SetMachiningTime (m_machiningTime);
        m_machiningTime = TimeSpan.Zero;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetNextToolNumber (string toolNumber)
    {
      this.Next?.SetNextToolNumber (toolNumber);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartCycle ()
    {
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      if (0 < m_machiningTime.Ticks) {
        if (log.IsDebugEnabled) {
          log.Debug ($"StartSequence: reset the internal machining time {m_machiningTime} because of a new sequence start");
        }
        m_machiningTime = TimeSpan.Zero;
      }
      this.Next?.StartSequence (sequenceKind);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      this.Next?.StopCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartProgram (bool edit, int level)
    {
      this.Next?.StartProgram (edit, level);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void EndProgram (bool edit, int level, bool endOfFile)
    {
      this.Next?.EndProgram (edit, level, endOfFile);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void ResumeProgram (bool edit, int level)
    {
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
      this.Next?.TriggerMachining ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void TriggerToolChange (string toolNumber = "")
    {
      this.Next?.TriggerToolChange (toolNumber: toolNumber);
    }
  }
}
