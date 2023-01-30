// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lemoine.Collections
{
  /// <summary>
  /// Conversion extensions
  /// </summary>
  public static class ConversionExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ConversionExtensions).FullName);

    /// <summary>
    /// Convert an object to a key/value pair
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="obj">not null</param>
    /// <returns></returns>
    public static KeyValuePair<TKey, TValue> ToKeyValuePair<TKey, TValue> (this Object obj)
    {
      if (null == obj) {
        log.ErrorFormat ("ToKeyValuePair: obj is null");
        throw new ArgumentNullException ("obj");
      }

      var type = obj.GetType ();
      if (type.IsGenericType && type == typeof (KeyValuePair<TKey, TValue>)) {
        return new KeyValuePair<TKey, TValue> (
                                                (TKey)type.GetProperty ("Key").GetValue (obj, null),
                                                (TValue)type.GetProperty ("Value").GetValue (obj, null)
                                             );
      }

      log.ErrorFormat ("ToKeyValuePair: obj is not a KeyValue pair");
      var message = string.Format ("obj argument must be of type KeyValuePair<{0},{1}>",
        typeof(TKey).FullName, typeof(TValue).FullName);
      throw new ArgumentException (message);
    }
  }
}
