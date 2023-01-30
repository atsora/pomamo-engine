// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Diagnostics;

namespace Lemoine.Collections
{
  /// <summary>
  /// Description of Interval.
  /// </summary>
  public class Interval<T> : System.Tuple<T, T>, IComparable<Interval<T>>
    where T : IComparable<T>
  {

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public Interval (T first, T second) : base(first, second)
    {
      Debug.Assert(first.CompareTo(second) <= 0);
    }
    #endregion // Constructors

    #region Methods

    /// <summary>
    /// CompareTo: (a,b) LT (c,d) iff a LT c OR ((a == c) AND b LT d)
    /// </summary>
    public int CompareTo (Interval<T> otherInterval)
    {
      int cmpFirst = this.Item1.CompareTo(otherInterval.Item1);
      return (cmpFirst == 0) ? this.Item2.CompareTo(otherInterval.Item2) : cmpFirst;
    }
    #endregion // Methods
  }
}
