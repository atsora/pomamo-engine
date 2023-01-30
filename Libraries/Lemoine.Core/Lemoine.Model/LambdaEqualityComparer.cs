// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Equality comparer class from two lambda expressions
  /// </summary>
  public class LambdaEqualityComparer<T>
    : IEqualityComparer<T>
  {
    readonly Func<T, T, bool> m_equals;
    readonly Func<T, int> m_getHashCode;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="equals"></param>
    /// <param name="getHashCode"></param>
    public LambdaEqualityComparer (Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
      m_equals = equals;
      m_getHashCode = getHashCode;
    }

    /// <summary>
    /// <see cref="IEqualityComparer{T}"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Equals (T x, T y)
    {
      return m_equals (x, y);
    }

    /// <summary>
    /// <see cref="IEqualityComparer{T}"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode (T obj)
    {
      return m_getHashCode (obj);
    }
  }
}
