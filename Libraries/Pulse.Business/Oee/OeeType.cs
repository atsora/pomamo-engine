// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Business.Oee
{
  /// <summary>
  /// Kind of overall equipment effectiveness rate.
  ///
  /// The three rates are all the ratio between the effective production time
  /// (weighted by the production rate) and a reference time.
  /// They only differ by the machine observation states that are part of the reference time.
  /// </summary>
  public enum OeeType
  {
    /// <summary>
    /// Overall Equipment Effectiveness, known in French as TRS
    ///
    /// Only the machine observation states with the
    /// <see cref="CapacityLevel.ExpectedProduction"/> capacity level are considered
    /// </summary>
    Oee = 0,
    /// <summary>
    /// Overall Operations Effectiveness, known in French as TRG
    ///
    /// The machine observation states with the <see cref="CapacityLevel.ExpectedProduction"/>
    /// or the <see cref="CapacityLevel.Open"/> capacity levels are considered
    /// </summary>
    Ooe = 1,
    /// <summary>
    /// Total Effective Equipment Performance, known in French as TRE
    ///
    /// All the machine observation states are considered
    /// </summary>
    Teep = 2,
  }

  /// <summary>
  /// Extensions to <see cref="OeeType"/>
  /// </summary>
  public static class OeeTypeExtensions
  {
    static readonly string USE_IS_PRODUCTION_FALLBACK_KEY = "Business.Oee.UseIsProductionFallback";
    static readonly bool USE_IS_PRODUCTION_FALLBACK_DEFAULT = true;

    static readonly ILog log = LogManager.GetLogger (typeof (OeeTypeExtensions).FullName);

    /// <summary>
    /// Try to convert a string into a <see cref="OeeType"/>.
    ///
    /// Both the English acronyms (OEE / OOE / TEEP) and the French ones (TRS / TRG / TRE)
    /// are accepted, whatever the case
    /// </summary>
    /// <param name="s"></param>
    /// <param name="oeeType"></param>
    /// <returns>false if <paramref name="s"/> does not correspond to any known rate</returns>
    public static bool TryParse (string s, out OeeType oeeType)
    {
      oeeType = OeeType.Oee;

      if (string.IsNullOrEmpty (s)) {
        return false;
      }

      switch (s.Trim ().ToLowerInvariant ()) {
      case "oee":
      case "trs":
        oeeType = OeeType.Oee;
        return true;
      case "ooe":
      case "trg":
        oeeType = OeeType.Ooe;
        return true;
      case "teep":
      case "tre":
        oeeType = OeeType.Teep;
        return true;
      default:
        log.Error ($"TryParse: {s} does not correspond to any known rate");
        return false;
      }
    }

    /// <summary>
    /// Capacity level of a machine observation state.
    ///
    /// The capacity level is optional in the configuration. When it is not set,
    /// it is by default deduced from the IsProduction property, so that the rates
    /// remain consistent with the production periods of a machine:
    /// a production machine observation state is considered as
    /// <see cref="CapacityLevel.ExpectedProduction"/>, else as <see cref="CapacityLevel.Closed"/>.
    /// This fall-back can be turned off with the Business.Oee.UseIsProductionFallback config key,
    /// then null is returned instead.
    /// </summary>
    /// <param name="machineObservationState">not null</param>
    /// <returns>null if the capacity level is unknown</returns>
    public static CapacityLevel? GetCapacityLevel (this IMachineObservationState machineObservationState)
    {
      if (machineObservationState is null) {
        log.Fatal ("GetCapacityLevel: machineObservationState is null");
        throw new ArgumentNullException ("machineObservationState");
      }

      if (machineObservationState.CapacityLevel.HasValue) {
        return machineObservationState.CapacityLevel.Value;
      }

      if (!Lemoine.Info.ConfigSet.LoadAndGet (USE_IS_PRODUCTION_FALLBACK_KEY, USE_IS_PRODUCTION_FALLBACK_DEFAULT)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"GetCapacityLevel: no capacity level for {machineObservationState.Id} and no fall-back");
        }
        return null;
      }

      return machineObservationState.IsProduction
        ? CapacityLevel.ExpectedProduction
        : CapacityLevel.Closed;
    }

    /// <summary>
    /// Is a machine observation state part of the reference time of the specified rate ?
    /// </summary>
    /// <param name="oeeType"></param>
    /// <param name="machineObservationState">not null</param>
    /// <returns></returns>
    public static bool IsIncluded (this OeeType oeeType, IMachineObservationState machineObservationState)
    {
      if (OeeType.Teep == oeeType) { // All the machine observation states
        return true;
      }

      var capacityLevel = machineObservationState.GetCapacityLevel ();
      if (!capacityLevel.HasValue) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsIncluded: unknown capacity level for machine observation state {machineObservationState.Id} => exclude it");
        }
        return false;
      }

      switch (oeeType) {
      case OeeType.Oee:
        return CapacityLevel.ExpectedProduction == capacityLevel.Value;
      case OeeType.Ooe:
        return (CapacityLevel.ExpectedProduction == capacityLevel.Value)
          || (CapacityLevel.Open == capacityLevel.Value);
      default:
        log.Fatal ($"IsIncluded: unexpected rate {oeeType}");
        return false;
      }
    }
  }
}
