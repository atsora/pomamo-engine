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
  /// Sequence time computation for standard g-codes
  /// </summary>
  public class SequenceTimeComputation
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceTimeComputation).FullName);

    double? m_x = null;
    double? m_y = null;
    double? m_z = null;
    TimeSpan m_sequenceTime = TimeSpan.FromSeconds (0);

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceTimeComputation (IStamper stamper, StampingData stampingData, IMilestoneStampLineCreator? milestoneStampLineCreator = null, double adjustingFactor = 1.0)
    {
      this.Stamper = stamper;
      this.StampingData = stampingData;
      this.MilestoneStampLineCreator = milestoneStampLineCreator;
      this.AdjustingFactor = adjustingFactor;
    }

    IMilestoneStampLineCreator? MilestoneStampLineCreator { get; }

    /// <summary>
    /// Adjusting factor
    /// </summary>
    public double AdjustingFactor = 1.0;

    /// <summary>
    /// Frequency trigger for the sequence milestones
    /// </summary>
    public TimeSpan MilestoneTriggerFrequency = TimeSpan.FromMinutes (1);

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
      if (!m_x.HasValue || !m_y.HasValue || !m_z.HasValue) {
        log.Info ("NotifyNewBlock: missing position");
        return;
      }

      TimeSpan machiningTime;
      if (this.StampingData.TryGet<double?> ("G-Motion", out var modalMotion) && (modalMotion is not null)) {
        if (0.0 == modalMotion) { // Rapid traverse
          // TODO: standard rapid traverse ?
          machiningTime = TimeSpan.FromSeconds (0);
        }
        else if (1.0 == modalMotion) { // Linear
          machiningTime = ComputeLinearMotion ();
        }
        else {
          log.Error ($"NotifyNewBlock: modalMotion {modalMotion} not implemented");
          throw new NotImplementedException ();
        }
      }
      else {
        log.Error ($"NotifyNewBlock: G-Motion is not defined");
        throw new InvalidOperationException ();
      }
      machiningTime = TimeSpan.FromSeconds (AdjustingFactor * machiningTime.TotalSeconds);
      var newSequenceTime = m_sequenceTime.Add (machiningTime);

      if (edit) {
        if (0 < this.MilestoneTriggerFrequency.Ticks) {
          var previousDivisor = (int)(m_sequenceTime.TotalSeconds / this.MilestoneTriggerFrequency.TotalSeconds);
          var nextDivisor = (int)(newSequenceTime.TotalSeconds / this.MilestoneTriggerFrequency.TotalSeconds);
          if (previousDivisor != nextDivisor) {
            AddSequenceMilestoneLine (newSequenceTime);
          }
        }
      }
      m_sequenceTime = newSequenceTime;

      this.Next?.NotifyNewBlock (edit, level);

      this.StampingData.TryGet<double?> ("X", out m_x);
      this.StampingData.TryGet<double?> ("Y", out m_y);
      this.StampingData.TryGet<double?> ("Z", out m_z);
    }

    void AddSequenceMilestoneLine (TimeSpan sequenceTime)
    {
      if (this.MilestoneStampLineCreator is null) {
        if (log.IsDebugEnabled) {
          log.Debug ("AddSequenceMilestoneLine: no milestone stamp line creator is defined");
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

    double GetFeedRateMode ()
    {
      if (this.StampingData.TryGet<double> ("G-FeedRateMode", out var feedRateMode)) {
        return feedRateMode;
      }
      else {
        return 94;
      }
    }

    double GetFeedrate ()
    {
      if (this.StampingData.TryGet<double> ("F", out var feedrate)) {
        return feedrate;
      }
      else {
        // TODO: ... default feedrate ?
        return 1.0;
      }
    }

    TimeSpan ComputeLinearMotion ()
    {
      if (!m_x.HasValue || !m_y.HasValue || !m_z.HasValue) {
        throw new InvalidOperationException ("Missing position");
      }

      var feedRateMode = GetFeedRateMode ();
      var feedrate = GetFeedrate ();
      if (feedRateMode == 93) {
        return TimeSpan.FromMinutes (1.0 / feedrate);
      }

      this.StampingData.TryGet<double> ("X", out var x);
      this.StampingData.TryGet<double> ("Y", out var y);
      this.StampingData.TryGet<double> ("Z", out var z);

      var dx = x - m_x.Value;
      var dy = y - m_y.Value;
      var dz = z - m_z.Value;
      var distance = Math.Sqrt (dx * dx + dy * dy + dz * dz);

      if (feedRateMode == 94) {
        return TimeSpan.FromMinutes (distance / feedrate);
      }
      else if (feedRateMode == 95) { // rev/min
        throw new NotImplementedException ();
      }

      throw new NotImplementedException ();
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
      throw new NotImplementedException ();
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StartSequence (SequenceKind sequenceKind)
    {
      m_sequenceTime = TimeSpan.FromSeconds (0);
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
      this.Next?.TriggerToolChange ();
    }
  }
}
