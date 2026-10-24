// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Business.Operation
{
  /// <summary>
  /// Where a production rate comes from, see <see cref="OperationProductionRate"/>
  /// </summary>
  public enum OperationProductionRateSource
  {
    /// <summary>
    /// The cycles of the operation in the past production periods
    /// </summary>
    History,
    /// <summary>
    /// The standard cycle duration, with the goal of type QuantityVsProductionCycleDuration
    /// </summary>
    Goal,
    /// <summary>
    /// The standard cycle duration, with the default factor
    /// </summary>
    Default
  }

  /// <summary>
  /// How long one cycle of an operation takes in a production period, stops included
  /// </summary>
  [Serializable]
  public class OperationProductionRateResponse
  {
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="cycleDuration">strictly positive production time per cycle</param>
    /// <param name="source"></param>
    public OperationProductionRateResponse (TimeSpan cycleDuration, OperationProductionRateSource source)
    {
      this.CycleDuration = cycleDuration;
      this.Source = source;
    }

    /// <summary>
    /// Production time one cycle takes, stops included: the time the machine spends in
    /// production periods to make one cycle
    /// </summary>
    public TimeSpan CycleDuration { get; }

    /// <summary>
    /// Where the rate comes from
    /// </summary>
    public OperationProductionRateSource Source { get; }

    /// <summary>
    /// <see cref="object.ToString"/>
    /// </summary>
    /// <returns></returns>
    public override string ToString () => $"[OperationProductionRate {this.CycleDuration} from {this.Source}]";
  }
}
