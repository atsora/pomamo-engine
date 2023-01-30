// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Collections
{
  /// <summary>
  /// Class to serialize a Tuple
  /// </summary>
  [Serializable]
  public class SerializableTuple<T, U>
  {
    Tuple<T, U> m_implementation;

    /// <summary>
    /// Constructor with no parameter, needed for the serialization
    /// </summary>
    public SerializableTuple ()
    {
      m_implementation = new Tuple<T, U> (default (T), default (U));
    }

    /// <summary>
    /// Build a pair (first,second)
    /// </summary>
    /// <param name="first"></param>
    /// <param name="second"></param>
    public SerializableTuple (T first, U second)
    {
      m_implementation = new Tuple<T, U> (first, second);
    }

    /// <summary>
    /// First element of pair
    /// </summary>
    public T Item1
    {
      get
      {
        return m_implementation.Item1;
      }
      set
      {
        m_implementation = new Tuple<T, U> (value, this.Item2);
      }
    }

    /// <summary>
    /// Second element of pair
    /// </summary>
    public U Item2
    {
      get
      {
        return m_implementation.Item2;
      }
      set
      {
        m_implementation = new Tuple<T, U> (this.Item1, value);
      }
    }

    /// <summary>
    /// Equality
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals (object obj)
    {
      return m_implementation.Equals (obj);
    }

    /// <summary>
    /// Hash
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode ()
    {
      return m_implementation.GetHashCode ();
    }
  }
}
