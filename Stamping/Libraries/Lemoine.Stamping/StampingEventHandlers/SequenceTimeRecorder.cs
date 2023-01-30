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
  /// Sequence time recorder:
  /// <item>record the machining time per sequence</item>
  /// <item>add milestone lines</item>
  /// </summary>
  public class SequenceTimeRecorder
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceTimeRecorder).FullName);

    TimeSpan m_sequenceTime = TimeSpan.FromSeconds (0);
    TimeSpan? m_sequenceMilestone = null;
    readonly ITimeConfig m_timeConfig;

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceTimeRecorder (IStamper stamper, StampingData stampingData, IMilestoneStampLineCreator milestoneStampLineCreator, ITimeConfig timeConfig)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
      this.MilestoneStampLineCreator = milestoneStampLineCreator;
      m_timeConfig = timeConfig;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceTimeRecorder (IStamper stamper, StampingData stampingData, ITimeConfig timeConfig)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
      this.MilestoneStampLineCreator = null;
      m_timeConfig = timeConfig;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceTimeRecorder (IStamper stamper, StampingData stampingData, IMilestoneStampLineCreator milestoneStampLineCreator)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
      this.MilestoneStampLineCreator = milestoneStampLineCreator;
      m_timeConfig = new Lemoine.Stamping.TimeConfigs.TimeConfig ();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceTimeRecorder (IStamper stamper, StampingData stampingData)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
      this.MilestoneStampLineCreator = null;
      m_timeConfig = new Lemoine.Stamping.TimeConfigs.TimeConfig ();
    }

    StampingData StampingData { get; }

    IMilestoneStampLineCreator? MilestoneStampLineCreator { get; }

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
      if (edit && m_sequenceMilestone.HasValue) {
        AddSequenceMilestoneLine (m_sequenceMilestone.Value);
        m_sequenceMilestone = null;
      }

      this.Next?.NotifyNewBlock (edit, level);
    }

    void AddSequenceMilestoneLine (TimeSpan sequenceTime)
    {
      if (MilestoneStampLineCreator is null) {
        if (log.IsDebugEnabled) {
          log.Debug ("AddSequenceMilestoneLine: no milestone stamp line creator");
        }
      }
      else {
        string? line;
        if (this.StampingData.TryGet ("SequenceStampId", out int sequenceStampId)) {
          line = MilestoneStampLineCreator.CreateMilestoneStampLine (sequenceTime, sequenceStampId);
        }
        else {
          line = MilestoneStampLineCreator.CreateMilestoneStampLine (sequenceTime);
        }
        if (line is not null) {
          this.Stamper.AddLine (line);
        }
      }
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
      var adjustedDuration = TimeSpan.FromSeconds (duration.TotalSeconds * m_timeConfig.GetTimeFactor ());
      var newSequenceTime = m_sequenceTime.Add (adjustedDuration);

      var milestoneTriggerFrequency = m_timeConfig.GetMilestoneTriggerFrequency ();
      if (0 < milestoneTriggerFrequency.Ticks) {
        var previousDivisor = (int)(m_sequenceTime.TotalSeconds / milestoneTriggerFrequency.TotalSeconds);
        var nextDivisor = (int)(newSequenceTime.TotalSeconds / milestoneTriggerFrequency.TotalSeconds);
        if (previousDivisor != nextDivisor) {
          m_sequenceMilestone = newSequenceTime;
        }
      }
      m_sequenceTime = newSequenceTime;

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
      this.Next?.StartCycle ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      RecordSequenceDuration ();
      m_sequenceMilestone = null;
      m_sequenceTime = TimeSpan.FromSeconds (0);
      this.Next?.StartSequence (sequenceKind);
      ResetSequenceDuration ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      RecordSequenceDuration ();
      this.Next?.StopCycle ();
      ResetSequenceDuration ();
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
      if (edit) {
        RecordSequenceDuration ();
      }
      this.Next?.EndProgram (edit, level, endOfFile);
      if (edit && (!endOfFile || (0 != level))) {
        ResetSequenceDuration ();
      }
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
      this.Next?.TriggerToolChange ();
    }

    void RecordSequenceDuration ()
    {
      var sequenceDuration = StampingData.SequenceDuration;
      if (sequenceDuration.HasValue) {
        if ((0 != m_sequenceTime.Ticks) && !sequenceDuration.Value.Equals (m_sequenceTime)) {
          log.Error ($"RecordSequenceDuration: stored={sequenceDuration} VS new={m_sequenceTime}");
        }
        StampingData.SequenceDuration = m_sequenceTime;
      }
      else { // !sequenceDuration.HasValue
        if (0 == m_sequenceTime.Ticks) {
          log.Debug ($"RecordSequenceDuration: sequence time is 0s");
        }
        else {
          StampingData.SequenceDuration = m_sequenceTime;
        }
      }
    }

    void ResetSequenceDuration ()
    {
      StampingData.SequenceDuration = null;
    }
  }
}
