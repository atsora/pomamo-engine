// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// Start cycle stamp writer: use the stamping data "StartCycleStampId" and the IStampLineCreator
  /// to add a new line with the start cycle stamp
  /// </summary>
  public class StartCycleStampWriter
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (StartCycleStampWriter).FullName);

    static readonly TimeSpan MIN_RESET_TIME = TimeSpan.FromSeconds (5);

    readonly IStartCycleStampLineCreator m_stampLineCreator;
    TimeSpan m_machiningTime = TimeSpan.FromTicks (0);
    bool m_resetDone = false;

    /// <summary>
    /// Constructor
    /// </summary>
    public StartCycleStampWriter (IStamper stamper, StampingData stampingData, IStartCycleStampLineCreator stampLineCreator)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
      m_stampLineCreator = stampLineCreator;
    }

    StampingData StampingData { get; }

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
    /// <param name="comment"></param>
    public void SetComment (string comment)
    {
      this.Next?.SetComment (comment);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void SetData (string key, object v)
    {
      this.Next?.SetData (key, v);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="duration"></param>
    public void SetMachiningTime (TimeSpan duration)
    {
      if (!m_resetDone) {
        m_machiningTime = m_machiningTime.Add (duration);
      }
      this.Next?.SetMachiningTime (duration);
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
      if (this.StampingData.TryGet ("StartCycleStampId", out int stampId)) {
        var line = m_stampLineCreator.CreateStartCycleStampLine (stampId);
        this.Stamper.AddLine (line);
        m_machiningTime = TimeSpan.FromSeconds (0);
      }
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      if (!m_resetDone && (MIN_RESET_TIME <= m_machiningTime)) {
        ResetStamp ();
      }
      this.Next?.StartSequence (sequenceKind);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      if (!m_resetDone) {
        ResetStamp ();
      }
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
      if (edit && (0 == level) && !endOfFile && !m_resetDone) {
        ResetStamp ();
      }
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
      this.Next?.TriggerToolChange (toolNumber);
    }

    void ResetStamp ()
    {
      var line = m_stampLineCreator.CreateStartCycleStampLine (0);
      this.Stamper.AddLine (line);
      m_resetDone = true;
    }
  }
}
