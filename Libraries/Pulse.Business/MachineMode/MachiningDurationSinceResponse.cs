// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Machining duration of a machine since a date/time, with the date/time up to which
  /// it was computed
  ///
  /// The machining duration only grows: a new response is built from the previous one by
  /// adding the machining duration of the range that was analysed since, which is why the
  /// date/time the duration stops at is part of the response
  /// </summary>
  [Serializable]
  public class MachiningDurationSinceResponse
  {
    static readonly ILog log = LogManager.GetLogger (typeof (MachiningDurationSinceResponse).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="since">start of the counted period</param>
    /// <param name="until">
    /// date/time the duration stops at. Not before since: what happened after it is not
    /// analysed yet
    /// </param>
    /// <param name="duration">machining duration in [since, until)</param>
    public MachiningDurationSinceResponse (DateTime since, DateTime until, TimeSpan duration)
    {
      if (until < since) {
        log.Fatal ($"MachiningDurationSinceResponse: until {until} is before since {since}");
        throw new ArgumentException ("until is before since", nameof (until));
      }

      this.Since = since;
      this.Until = until;
      this.Duration = duration;
    }

    /// <summary>
    /// Start of the counted period
    /// </summary>
    public DateTime Since { get; }

    /// <summary>
    /// Date/time the duration stops at
    ///
    /// What happened after it is not analysed yet: asking again later returns the same
    /// duration plus the machining duration of the range that was analysed in between
    /// </summary>
    public DateTime Until { get; }

    /// <summary>
    /// Machining duration in the range
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Range the duration covers, empty when nothing was analysed since
    /// </summary>
    public UtcDateTimeRange Range => (this.Since < this.Until)
      ? new UtcDateTimeRange (this.Since, this.Until)
      : new UtcDateTimeRange ();

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[MachiningDurationSince {this.Since}-{this.Until} {this.Duration}]";
  }
}
