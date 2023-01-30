// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Collections
{
  /// <summary>
  /// Linq
  /// </summary>
  public static class LinqExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (LinqExtensions).FullName);

    /// <summary>
    /// Convert an <see cref="IEnumerable{T}"/> to a <see cref="HashSet{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="comparer">null: default coparer</param>
    /// <returns></returns>
    public static HashSet<T> ToHashSet<T> (this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
    {
      return new HashSet<T> (source, comparer);
    }

    /// <summary>
    /// Check if all the elements of <see cref="IEnumerable{T}"/> are equal
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="comparer">null: default coparer</param>
    /// <returns></returns>
    public static bool Same<T> (this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
    {
      var hashSet = source.ToHashSet (comparer);
      return hashSet.Count () <= 1;
    }

    /// <summary>
    /// Return the unique element of source if all the elements are equal, else default(T)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="comparer">null: default coparer</param>
    /// <returns></returns>
    public static T UniqueOrDefault<T> (this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
    {
      if (source.Any ()) {
        if (source.Same (comparer)) {
          var first = source.First ();
          if (log.IsDebugEnabled) {
            log.Debug ($"UniqueOrDefault: all the elements are equal"); // Note: do not display the first element, else you may need to unproxy it
          }
          return first;
        }
        else {
          if (log.IsDebugEnabled) {
            log.Debug ($"UniqueOrDefault: not all the elements are equal, return null");
          }
          return default (T);
        }
      }
      else {
        if (log.IsDebugEnabled) {
          log.Debug ("UniqueOrDefault: no element => return true");
        }
        return default (T);
      }
    }

    /// <summary>
    /// Return the unique element of source if all the elements are equal, else default(T)
    /// </summary>
    /// <param name="source"></param>
    /// <param name="select"></param>
    /// <param name="comparer">null: default coparer</param>
    /// <returns></returns>
    public static U UniqueOrDefault<T, U> (this IEnumerable<T> source, Func<T, U> select, IEqualityComparer<U> comparer = null)
    {
      return source.Select (select).UniqueOrDefault (comparer);
    }
  }
}
