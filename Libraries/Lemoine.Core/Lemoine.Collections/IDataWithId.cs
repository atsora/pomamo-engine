// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lemoine.Collections
{
  /// <summary>
  /// Interface for all the models that are uniquely identified by their integer ID
  /// </summary>
  public interface IDataWithId<ID>
  {
    /// <summary>
    /// ID
    /// </summary>
    ID Id { get; }
  }

  /// <summary>
  /// Interface for all the models that are uniquely identified by an integer ID
  /// </summary>
  public interface IDataWithId : IDataWithId<int>
  {
  }

  /// <summary>
  /// <see cref="IEqualityComparer{T}"/> that is using the Id of a class
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="ID"></typeparam>
  public class EqualityComparerFromId<T, ID>
    : IEqualityComparer<T>
    where T : class
  {
    readonly Func<T, ID> m_getId;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="getId">not null</param>
    public EqualityComparerFromId (Func<T, ID> getId)
    {
      Debug.Assert (null != getId);

      m_getId = getId;
    }

    /// <summary>
    /// <see cref="IEqualityComparer{T}"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool Equals (T x, T y)
    {
      if (x is null) {
        return y is null;
      }
      if (y is null) {
        return false;
      }
      return m_getId (x).Equals (m_getId (y));
    }

    /// <summary>
    /// <see cref="IEqualityComparer{T}"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int GetHashCode (T obj)
    {
      return obj is null
        ? int.MinValue
        : m_getId (obj).GetHashCode ();
    }
  }

  /// <summary>
  /// <see cref=" IEqualityComparer{T}"/> that is based on Id which is an int
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class EqualityComparerDataWithId<T>
    : EqualityComparerDataWithId<T, int>
    , IEqualityComparer<T>
    where T : class, IDataWithId
  {
  }

  /// <summary>
  /// <see cref="IEqualityComparer{T}"/> that is based on an ID
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="ID"></typeparam>
  public class EqualityComparerDataWithId<T, ID>
    : EqualityComparerFromId<T, ID>
    , IEqualityComparer<T>
    where T : class, IDataWithId<ID>
  {
    /// <summary>
    /// Constructor
    /// </summary>
    public EqualityComparerDataWithId ()
      : base (x => x.Id)
    {
    }
  }
}
