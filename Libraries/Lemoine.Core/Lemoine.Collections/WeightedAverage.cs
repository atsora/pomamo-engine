// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETSTANDARD

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// WeightedAverage
  /// </summary>
  public static class WeightedAverageLinqExtension
  {
    static readonly ILog log = LogManager.GetLogger (typeof (WeightedAverageLinqExtension).FullName);

    /// <summary>
    /// Extension to an IEnumerable to compute a weighted average
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">not null</param>
    /// <param name="v">function to get the value</param>
    /// <param name="weight">function to get the weight of the value</param>
    /// <returns>(weighted average, new weight)</returns>
    public static (double, double) WeightedAverage<T> (this IEnumerable<T> collection, Func<T, double> v, Func<T, double> weight)
    {
      return collection
        .Aggregate ( (0.0, 0.0), (x, y) => AggregateWeightedAverage (x, (v (y), weight (y))));
    }

    /// <summary>
    /// Extension to an IEnumerable to compute a weighted average
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection">not null</param>
    /// <param name="v">function to get the value</param>
    /// <param name="weight">function to get the weight of the value</param>
    /// <returns>(weighted average, new weight)</returns>
    public static (double, TimeSpan) WeightedAverage<T> (this IEnumerable<T> collection, Func<T, double> v, Func<T, TimeSpan> weight)
    {
      var a = collection
        .Aggregate ((0.0, 0.0), (x, y) => AggregateWeightedAverage (x, (v (y), weight (y).TotalSeconds)));
      return (a.Item1, TimeSpan.FromSeconds (a.Item2));
    }

    static (double, double) AggregateWeightedAverage ((double, double) x, (double, double) y)
    {
      var newWeight = x.Item2 + y.Item2;
      if (0.0 == newWeight) {
        return (0.0, 0.0);
      }
      else {
        var newValue = (x.Item1 * x.Item2) / newWeight + (y.Item1 * y.Item2) / newWeight;
        return (newValue, newWeight);
      }
    }
  }
}

#endif // NETSTANDARD
