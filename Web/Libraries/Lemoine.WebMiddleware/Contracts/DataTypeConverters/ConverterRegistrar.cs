// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Lemoine.WebMiddleware.Contracts.DataTypeConverters.DefaultConverters;

namespace Lemoine.WebMiddleware.Contracts.DataTypeConverters
{
  public class ConverterRegistrar
  {
    ILog log = LogManager.GetLogger<ConverterRegistrar> ();

    readonly IDictionary<Type, Func<string, object?>> m_delegates = new Dictionary<Type, Func<string, object?>> ();

    public ConverterRegistrar ()
    {
      RegisterDefaultConveters ();
    }

    private void RegisterDefaultConveters ()
    {
      m_delegates[typeof (string)] = s => s;
      m_delegates[typeof (sbyte)] = s => ParseConverter<sbyte> (s, sbyte.Parse, 0);
      m_delegates[typeof (short)] = s => ParseConverter<short> (s, short.Parse, 0);
      m_delegates[typeof (int)] = s => ParseConverter<int> (s, int.Parse, 0);
      m_delegates[typeof (long)] = s => ParseConverter<long> (s, long.Parse, 0);
      m_delegates[typeof (byte)] = s => ParseConverter<byte> (s, byte.Parse, 0);
      m_delegates[typeof (ushort)] = s => ParseConverter<ushort> (s, ushort.Parse, 0);
      m_delegates[typeof (uint)] = s => ParseConverter<uint> (s, uint.Parse, 0);
      m_delegates[typeof (ulong)] = s => ParseConverter<ulong> (s, ulong.Parse, 0);
      m_delegates[typeof (float)] = s => ParseConverter<float> (s, float.Parse, 0);
      m_delegates[typeof (double)] = s => ParseConverter<double> (s, double.Parse, 0);
      m_delegates[typeof (bool)] = ConvertToGeneric<bool> (BoolConverter);

      m_delegates[typeof (int?)] = NullableConverter<int>;
      m_delegates[typeof (long?)] = NullableConverter<long>;
      m_delegates[typeof (double?)] = NullableConverter<double>;
      m_delegates[typeof (bool?)] = NullableConverter<bool>;

      m_delegates[typeof (IList<int>)] = ListConverter<int>;
      m_delegates[typeof (IList<long>)] = ListConverter<long>;
      m_delegates[typeof (IList<string>)] = ListConverterClass<string>;
    }

    Func<string, object?> ConvertToGeneric<T> (Func<string, T> converter)
    {
      return x => (object?)converter (x);
    }

    Func<string, T> ConvertToSpecificNotNullableType<T> (Func<string, object?> converter)
    {
      return s =>
      {
        var r = converter (s);
        if (null == r) {
          log.Fatal ($"ConvertToSpecificType<T>: unexpected null value returned");
          throw new NullReferenceException ($"Unexpected null value for a not nullable type");
        }
        else {
          return (T)r;
        }
      };
    }

    public Func<string, object?> GetConverter (Type t)
    {
      Func<string, object?>? d;
      if (m_delegates.TryGetValue (t, out d)) {
        if (null == d) {
          log.Fatal ($"GetConverter: converter for type {t} is null");
          throw new NullReferenceException ($"Unexpected null converter for type {t}");
        }
        else {
          return d;
        }
      }
      return s => ConvertAuto (t, s);
    }

    object? ConvertAuto (Type t, string s)
    {
      try {
        return Enum.Parse (t, s);
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ("ConvertAuto: not a valid Enum", ex);
        }
      }
      return Convert.ChangeType (s, t);
    }

    public Func<string, T> GetConverterNotNullable<T> ()
    {
      return s =>
      {
        var converter = GetConverter (typeof (T));
        var r = converter (s);
        if (null == r) {
          log.Fatal ($"GetConverter<T>: unexpected null value returned");
          throw new NullReferenceException ($"Unexpected null value for a not nullable type");
        }
        else {
          return (T)r;
        }
      };
    }

    public void AddRegistration<T> (Func<string, object?> converter)
    {
      AddRegistration (typeof (T), converter);
    }

    public void AddRegistration (Type t, Func<string, object?> converter)
    {
      m_delegates[t] = converter;
    }

    internal object? ParseConverter<T> (string str, Func<string, T> parse, T exceptionDefault) where T : struct
    {
      try {
        return parse (str);
      }
      catch (Exception ex) {
        log.Error ($"ParseConverter: parse exception for {str} type {typeof(T)}", ex);
        return exceptionDefault;
      }
    }

    internal object? NullableConverter<T> (string str) where T : struct
    {
      if (string.IsNullOrEmpty (str)) {
        return (T?)null;
      }
      else {
        var converter = GetConverter (typeof (T));
        return (T?)converter.Invoke (str);
      }
    }

    internal IList<T> ListConverter<T> (string str) where T : struct
    {
      return (IList<T>)str.Split (',')
        .Where (s => !string.IsNullOrEmpty (s))
        .Select (x =>
        {
          var converter = GetConverterNotNullable<T> ();
          return (T)converter (x);
        })
        .ToList ();
    }

    internal IList<T> ListConverterClass<T> (string str) where T : class
    {
      return (IList<T>)str.Split (',', StringSplitOptions.RemoveEmptyEntries)
        .Where (s => !string.IsNullOrEmpty (s))
        .Select (x =>
        {
          var converter = GetConverterNotNullable<T> ();
          return (T)converter (x);
        })
        .ToList ();
    }
  }
}
