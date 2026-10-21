// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Business.MachineMode
{
  /// <summary>
  /// Utilization rate of a machine in a past period, by machine observation state: the part
  /// of the time the machine was in a running machine mode while it was in that state
  ///
  /// The response only carries ids and computed values, so that it may outlive the session
  /// it was computed in
  /// </summary>
  [Serializable]
  public class UtilizationRateByObservationStateResponse
  {
    readonly IReadOnlyDictionary<int, double> m_rates;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="firstDay">first day of the period that was considered</param>
    /// <param name="lastDay">last day of the period that was considered</param>
    /// <param name="rates">
    /// utilization rate by machine observation state id, between 0 and 1, not null. Only the
    /// machine observation states that were observed long enough are part of it
    /// </param>
    /// <param name="overallRate">utilization rate whatever the machine observation state, null if there is no data</param>
    /// <param name="defaultProductionRate">rate of a production period when there is no data</param>
    /// <param name="defaultNotProductionRate">rate of a period that is not production when there is no data</param>
    public UtilizationRateByObservationStateResponse (DateTime firstDay, DateTime lastDay, IReadOnlyDictionary<int, double> rates, double? overallRate, double defaultProductionRate, double defaultNotProductionRate)
    {
      if (rates is null) {
        throw new ArgumentNullException (nameof (rates));
      }

      this.FirstDay = firstDay;
      this.LastDay = lastDay;
      m_rates = rates;
      this.OverallRate = overallRate;
      this.DefaultProductionRate = defaultProductionRate;
      this.DefaultNotProductionRate = defaultNotProductionRate;
    }

    /// <summary>
    /// First day of the period that was considered
    /// </summary>
    public DateTime FirstDay { get; }

    /// <summary>
    /// Last day of the period that was considered, included
    /// </summary>
    public DateTime LastDay { get; }

    /// <summary>
    /// Utilization rate by machine observation state id, between 0 and 1
    ///
    /// Only the machine observation states that were observed long enough in the period are
    /// part of it: the rate of a state that was hardly met is not significant
    /// </summary>
    public IReadOnlyDictionary<int, double> Rates => m_rates;

    /// <summary>
    /// Utilization rate in the period whatever the machine observation state, between 0 and 1
    ///
    /// null if there is no data in the period
    /// </summary>
    public double? OverallRate { get; }

    /// <summary>
    /// Utilization rate of a production period when there is no data in the period
    /// </summary>
    public double DefaultProductionRate { get; }

    /// <summary>
    /// Utilization rate of a period that is not production when there is no data in the period
    /// </summary>
    public double DefaultNotProductionRate { get; }

    /// <summary>
    /// Get the utilization rate of a machine observation state
    /// </summary>
    /// <param name="machineObservationStateId"></param>
    /// <returns>null if the machine observation state was not observed long enough in the period</returns>
    public double? GetRate (int machineObservationStateId)
    {
      return m_rates.TryGetValue (machineObservationStateId, out var rate)
        ? rate
        : (double?)null;
    }

    /// <summary>
    /// Get the utilization rate to expect in a period, always with a value:
    /// <list type="number">
    /// <item>the rate of its machine observation state, if it was observed long enough</item>
    /// <item>else the overall rate, if there is data in the period</item>
    /// <item>else the default rate of a production period, or of a period that is not
    /// production. A period whose production is not known takes the production one</item>
    /// </list>
    /// </summary>
    /// <param name="machineObservationStateId">null if not known</param>
    /// <param name="production">null if not known</param>
    /// <returns></returns>
    public double GetExpectedRate (int? machineObservationStateId, bool? production)
    {
      if (machineObservationStateId.HasValue) {
        var rate = GetRate (machineObservationStateId.Value);
        if (rate.HasValue) {
          return rate.Value;
        }
      }
      if (this.OverallRate.HasValue) {
        return this.OverallRate.Value;
      }
      return (production ?? true)
        ? this.DefaultProductionRate
        : this.DefaultNotProductionRate;
    }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () =>
      $"[UtilizationRateByObservationState {this.FirstDay:yyyy-MM-dd}-{this.LastDay:yyyy-MM-dd} overall={this.OverallRate} states={m_rates.Count}]";
  }
}
