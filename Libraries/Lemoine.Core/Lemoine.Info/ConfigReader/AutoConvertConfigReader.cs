// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Lemoine.Model;

namespace Lemoine.Info.ConfigReader
{
  /// <summary>
  /// Config reader to automatically convert a string to an object of a specific type
  /// 
  /// Thread safe if the stringConfigReader is thread safe
  /// </summary>
  public class AutoConvertConfigReader
    : AutoConvertConfigReader<IStringConfigReader>
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="stringConfigReader"></param>
    public AutoConvertConfigReader (IStringConfigReader stringConfigReader)
      : base (stringConfigReader)
    {
    }
    #endregion // Constructors
  }

  /// <summary>
  /// Config reader to automatically convert a string to an object of a specific type
  /// 
  /// Thread safe if the stringConfigReader is thread safe
  /// </summary>
  public class AutoConvertConfigReader<TStringConfigReader>
  : IGenericConfigReader
  where TStringConfigReader : IStringConfigReader
  {
    readonly TStringConfigReader m_stringConfigReader;

    static readonly ILog log = LogManager.GetLogger (typeof (AutoConvertConfigReader<TStringConfigReader>).FullName);

    /// <summary>
    /// Associated string config reader
    /// </summary>
    protected TStringConfigReader StringConfigReader => m_stringConfigReader;

    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="stringConfigReader"></param>
    public AutoConvertConfigReader (TStringConfigReader stringConfigReader)
    {
      m_stringConfigReader = stringConfigReader;
    }

    /// <summary>
    /// Get the raw data (in string format)
    /// </summary>
    /// <returns></returns>
    public string GetString (string key)
    {
      return m_stringConfigReader.GetString (key);
    }

    /// <summary>
    /// <see cref="IGenericConfigReader" />
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    /// <exception cref="ConfigKeyNotFoundException">The configuration key is not found</exception>
    /// <exception cref="InvalidCastException">Cast exception</exception>
    public T Get<T> (string key)
    {
      string stringData = GetString (key);
      return Convert<T> (stringData);
    }

    /// <summary>
    /// Make the conversion
    /// </summary>
    /// <param name="stringData"></param>
    /// <returns></returns>
    public static T Convert<T> (string stringData)
    {
      if (typeof (T).IsAssignableFrom (typeof (string))) {
        return (T)(object)stringData;
      }
      else if (typeof (T).IsAssignableFrom (typeof (bool))) {
        return (T)(object)!(string.Equals (stringData, "FALSE", StringComparison.InvariantCultureIgnoreCase)
                              || string.Equals (stringData, "0")
                              || string.IsNullOrEmpty (stringData));
      }
      else if (typeof (T).IsAssignableFrom (typeof (int?))) {
        if (string.IsNullOrEmpty (stringData)) {
          return default (T);
        }
        else {
          return (T)(object)int.Parse (stringData);
        }
      }
      else if (typeof (T).IsAssignableFrom (typeof (int))) {
        return (T)(object)int.Parse (stringData);
      }
      else if (typeof (T).IsAssignableFrom (typeof (long?))) {
        if (string.IsNullOrEmpty (stringData)) {
          return default (T);
        }
        else {
          return (T)(object)long.Parse (stringData);
        }
      }
      else if (typeof (T).IsAssignableFrom (typeof (long))) {
        return (T)(object)long.Parse (stringData);
      }
      else if (typeof (T).IsAssignableFrom (typeof (double?))) {
        if (string.IsNullOrEmpty (stringData)) {
          return default (T);
        }
        else {
          return (T)(object)double.Parse (stringData, System.Globalization.CultureInfo.InvariantCulture);
        }
      }
      else if (typeof (T).IsAssignableFrom (typeof (double))) {
        return (T)(object)double.Parse (stringData, System.Globalization.CultureInfo.InvariantCulture);
      }
      else if (typeof (T).IsAssignableFrom (typeof (TimeSpan))) {
        return (T)(object)TimeSpan.Parse (stringData);
      }
      else if (typeof (T).IsAssignableFrom (typeof (TimeSpan?))) {
        if (string.IsNullOrEmpty (stringData)) {
          return default (T);
        }
        else {
          return (T)(object)TimeSpan.Parse (stringData);
        }
      }
      else if (typeof (T).IsAssignableFrom (typeof (DateTime))) {
        return (T)(object)DateTime.Parse (stringData);
      }
      else if (typeof (T).IsAssignableFrom (typeof (Bound<DateTime>))) {
        if (stringData.Equals ("-oo", StringComparison.InvariantCultureIgnoreCase)) {
          return (T)(object)Bound.CreateLowerBound<DateTime> (null);
        }
        else if (stringData.Equals ("+oo", StringComparison.InvariantCultureIgnoreCase)) {
          return (T)(object)Bound.CreateUpperBound<DateTime> (null);
        }
        else {
          return (T)(object)Bound.CreateLowerBound<DateTime> (DateTime.Parse (stringData));
        }
      }
      else if (typeof (T).IsAssignableFrom (typeof (LowerBound<DateTime>))) {
        if (string.IsNullOrEmpty (stringData) || stringData.Equals ("-oo", StringComparison.InvariantCultureIgnoreCase)) {
          return (T)(object)new LowerBound<DateTime> (null);
        }
        else {
          return (T)(object)new LowerBound<DateTime> (DateTime.Parse (stringData));
        }
      }
      else if (typeof (T).IsAssignableFrom (typeof (UpperBound<DateTime>))) {
        if (string.IsNullOrEmpty (stringData) || stringData.Equals ("+oo", StringComparison.InvariantCultureIgnoreCase)) {
          return (T)(object)new UpperBound<DateTime> (null);
        }
        else {
          return (T)(object)new UpperBound<DateTime> (DateTime.Parse (stringData));
        }
      }
      else {
        log.Error ($"ConvertData: conversion to type {typeof (T)} is not supported");
        throw new InvalidCastException ();
      }
    }
  }
}
