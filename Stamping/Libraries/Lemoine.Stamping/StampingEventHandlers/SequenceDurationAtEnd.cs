// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Conversion;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.Stamping.StampingEventHandlers
{
  /// <summary>
  /// Set the sequence duration for a sequence at its end
  /// </summary>
  public class SequenceDurationAtEnd
    : IStampingEventHandler
  {
    readonly ILog log = LogManager.GetLogger (typeof (SequenceDurationAtEnd).FullName);

    readonly StampingData m_stampingData;
    ISequence? m_lastStoredSequence = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public SequenceDurationAtEnd (IStamper stamper, StampingData stampingData)
    {
      this.Stamper = stamper;
      m_stampingData = stampingData;
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
    public void SetComment (string comment)
    {
      this.Next?.SetComment (comment);
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="v"></param>
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
    /// <param name="toolNumber"></param>
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
      StoreDurationForCurrentSequence ();
      this.Next?.StartSequence (sequenceKind);
    }

    void StoreDurationForCurrentSequence ()
    {
      try {
        var sequenceDuration = m_stampingData.SequenceDuration;
        if (!sequenceDuration.HasValue) {
          if (log.IsDebugEnabled) {
            log.Debug ($"StoreDurationForCurrentSequence: no active sequence duration");
          }
          return;
        }

        var sequence = m_stampingData.Sequence;
        if (sequence is null) {
          if (log.IsInfoEnabled) {
            log.Info ($"StoreDurationForCurrentSequence: no sequence was previously set although a duration={sequenceDuration} is set, skip it");
          }
          return;
        }

        if (m_lastStoredSequence == sequence) {
          if (log.IsDebugEnabled) {
            log.Debug ($"StoreDurationForCurrentSequence: the duration is already set");
          }
          if (log.IsInfoEnabled && sequence.EstimatedTime.HasValue && !sequence.EstimatedTime.Value.Equals (sequenceDuration.Value)) {
            log.Info ($"StoreDurationForCurrentSequence: already set to {sequence.EstimatedTime} VS new={sequenceDuration.Value}");
          }
          return;
        }

        if (log.IsWarnEnabled && sequence.EstimatedTime.HasValue && !sequence.EstimatedTime.Value.Equals (sequenceDuration.Value)) {
          log.Warn ($"StoreDurationForCurrentSequence: existing={sequence.EstimatedTime} VS new={sequenceDuration.Value}");
        }

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginTransaction ("Stamping.SequenceDurationAtEnd")) {
            ModelDAOHelper.DAOFactory.SequenceDAO.Lock (sequence);
            sequence.EstimatedTime = TimeSpan.FromSeconds (sequenceDuration.Value.TotalSeconds);
            ModelDAOHelper.DAOFactory.SequenceDAO.MakePersistent (sequence);
            transaction.Commit ();
          }
        }
        m_lastStoredSequence = sequence;
      }
      catch (Exception ex) {
        log.Error ($"StoreDurationForCurrentSequence: exception", ex);
        throw;
      }
    }

    /// <summary>
    /// <see cref="IStampingEventHandler"/>
    /// </summary>
    public void StopCycle ()
    {
      StoreDurationForCurrentSequence ();
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
      if (0 == level) {
        StoreDurationForCurrentSequence ();
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
    /// <param name="toolNumber"></param>
    public void TriggerToolChange (string toolNumber = "")
    {
      this.Next?.TriggerToolChange (toolNumber);
    }

  }
}
