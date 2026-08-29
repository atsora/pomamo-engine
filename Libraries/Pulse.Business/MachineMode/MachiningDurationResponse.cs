// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Machining duration of a machine in a range, with the date/time it was counted up to
  ///
  /// The machining duration is read from the reason slots and from the machine activity
  /// summaries, which only exist where the activity analysis has run: a range that goes
  /// past that point is only counted up to it, and MaxDateTime says where
  /// </summary>
  [Serializable]
  public class MachiningDurationResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="duration">machining duration that was counted</param>
    /// <param name="maxDateTime">
    /// date/time the duration was counted up to, null when the whole requested range
    /// was counted
    /// </param>
    public MachiningDurationResponse (TimeSpan duration, DateTime? maxDateTime)
    {
      this.Duration = duration;
      this.MaxDateTime = maxDateTime;
    }

    /// <summary>
    /// Machining duration that was counted in the range
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Date/time the machining duration was counted up to, when the whole requested range
    /// could not be counted: the activity of the machine is not analysed after it
    ///
    /// null when the whole requested range was counted, and the duration is therefore final
    /// </summary>
    public DateTime? MaxDateTime { get; }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => this.MaxDateTime.HasValue
      ? $"[MachiningDuration {this.Duration} up to {this.MaxDateTime.Value}]"
      : $"[MachiningDuration {this.Duration}]";
  }
}
