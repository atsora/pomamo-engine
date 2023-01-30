// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Extensions.Analysis.Detection
{
  /// <summary>
  /// 
  /// </summary>
  public interface ISequenceMilestoneDetection
  {
    /// <summary>
    /// Start a new sequence
    /// 
    /// If a milestone was already recorded for the same sequence, keep it 
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="sequence"></param>
    void StartSequence (DateTime dateTime, ISequence sequence = null);

    /// <summary>
    /// Set a sequence milestone
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="milestone"></param>
    /// <param name="sequence"></param>
    void SetSequenceMilestone (DateTime dateTime, TimeSpan milestone, ISequence sequence = null);

    /// <summary>
    /// Remove the last set sequence milestone
    /// </summary>
    void RemoveSequenceMilestone ();

    /// <summary>
    /// Tag a sequence completed
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="sequence"></param>
    void TagSequenceCompleted (DateTime dateTime, ISequence sequence = null);

    /// <summary>
    /// Check a sequence milestone is still valid with the new specified sequence
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="sequence">not null</param>
    void CheckSequence (DateTime dateTime, ISequence sequence);

    /// <summary>
    /// Cancel a sequence milestone
    /// </summary>
    /// <param name="dateTime"></param>
    void CancelSequenceMilestone (DateTime dateTime);
  }
}
