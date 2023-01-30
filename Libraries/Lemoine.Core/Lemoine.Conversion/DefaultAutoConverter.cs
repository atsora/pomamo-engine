// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Conversion
{
  /// <summary>
  /// DefaultAutoConverter
  /// </summary>
  public class DefaultAutoConverter
    : IAutoConverter
  {
    readonly ILog log = LogManager.GetLogger (typeof (DefaultAutoConverter).FullName);

    #region Constructors
    /// <summary>
    /// Constructor
    /// </summary>
    public DefaultAutoConverter ()
    {
    }
    #endregion // Constructors

    #region IAutoConverter implementation
    /// <summary>
    /// <see cref="IAutoConverter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="datavalue"></param>
    /// <returns></returns>
    public T ConvertAuto<T> (object datavalue)
    {
      return (T)ConvertAuto (datavalue, typeof (T));
    }

    /// <summary>
    /// <see cref="IAutoConverter"/>
    /// </summary>
    /// <param name="datavalue"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public virtual object ConvertAuto (object datavalue, Type type)
    {
      return ConvertAuto (datavalue, type, false);
    }

    /// <summary>
    /// Convert 
    /// </summary>
    /// <param name="datavalue"></param>
    /// <param name="type"></param>
    /// <param name="skipErrorLog"></param>
    /// <returns></returns>
    public virtual object ConvertAuto (object datavalue, Type type, bool skipErrorLog)
    {
      if (datavalue.GetType ().Equals (type)) {
        if (log.IsDebugEnabled) {
          log.Debug ($"ConvertAuto: datavalue {datavalue} is already of type {type} => there is no need to convert it, return it");
        }
        return datavalue;
      }

      if (type == typeof (object)) {
        if (log.IsDebugEnabled) {
          log.Debug ("ConvertAuto: object needed => nothing to do, return it");
        }
        return datavalue;
      }

      if (type.IsInterface && type.IsInstanceOfType (datavalue)) {
        log.Debug ($"ConvertAuto: {type} is instance of type {datavalue.GetType ()}");
        return datavalue;
      }

      if (type.IsEnum) {
        log.Debug ($"ConvertAuto: check enum {type} is instance of type {datavalue.GetType ()}");
        try {
          return Enum.Parse (type, datavalue.ToString ());
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: {datavalue} is not a valid enum of type {type}", ex);
          throw;
        }
      }

      // Newtonsoft json object
      if (datavalue is Newtonsoft.Json.Linq.JObject) {
        try {
          return ((Newtonsoft.Json.Linq.JObject)datavalue).ToObject (type);
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: Newtonsoft Jobject deserialization error for {datavalue} into type {type}", ex);
        }
      }

      if (datavalue.GetType ().Equals (typeof (Nullable<int>))) {
        try {
          return ConvertAuto (((Nullable<int>)datavalue).Value, type, skipErrorLog);
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion from int? to {type} failed for {datavalue}", ex);
          throw;
        }
      }
      else if (datavalue.GetType ().Equals (typeof (Nullable<long>))) {
        try {
          return ConvertAuto (((Nullable<long>)datavalue).Value, type, skipErrorLog);
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion from long? to {type} failed for {datavalue}", ex);
          throw;
        }
      }
      else if (datavalue.GetType ().Equals (typeof (Nullable<double>))) {
        try {
          return ConvertAuto (((Nullable<double>)datavalue).Value, type, skipErrorLog);
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion from double? to {type} failed for {datavalue}", ex);
          throw;
        }
      }
      else if (datavalue.GetType ().Equals (typeof (Nullable<bool>))) {
        try {
          return ConvertAuto (((Nullable<bool>)datavalue).Value, type, skipErrorLog);
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion from bool? to {type} failed for {datavalue}", ex);
          throw;
        }
      }
      else if (datavalue.GetType ().Equals (typeof (Nullable<uint>))) {
        try {
          return ConvertAuto (((Nullable<uint>)datavalue).Value, type, skipErrorLog);
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion from uint? to {type} failed for {datavalue}", ex);
          throw;
        }
      }
      else if (type == typeof (Nullable<bool>)) {
        try {
          return (Nullable<bool>)Convert.ChangeType (datavalue, typeof (bool));
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion of {datavalue } into {type} failed", ex);
          throw;
        }
      }
      else if (type == typeof (Nullable<int>)) {
        try {
          return (Nullable<int>)Convert.ChangeType (datavalue, typeof (int));
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion of {datavalue } into {type} failed", ex);
          throw;
        }
      }
      else if (type == typeof (Nullable<double>)) {
        try {
          return (Nullable<double>)Convert.ChangeType (datavalue, typeof (double));
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion of {datavalue } into {type} failed", ex);
          throw;
        }
      }
      else if (type == typeof (Nullable<long>)) {
        try {
          return (Nullable<long>)Convert.ChangeType (datavalue, typeof (long));
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion of {datavalue } into {type} failed", ex);
          throw;
        }
      }
      else if (type == typeof (Nullable<uint>)) {
        try {
          return (Nullable<uint>)Convert.ChangeType (datavalue, typeof (uint));
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion of {datavalue } into {type} failed", ex);
          throw;
        }
      }
      else if (type == typeof (IDictionary<string, object>)) { // (string,...) to (string,...) dictionary
        try {
          if (datavalue is IEnumerable<KeyValuePair<string, int>> intKeyValues) {
            return ConvertDictionary (intKeyValues, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
          else if (datavalue is IEnumerable<KeyValuePair<string, double>> enumerable1) {
            return ConvertDictionary (enumerable1, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
          else if (datavalue is IEnumerable<KeyValuePair<string, string>> enumerable2) {
            return ConvertDictionary (enumerable2, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
          else if (datavalue is IEnumerable<KeyValuePair<string, bool>> enumerable3) {
            return ConvertDictionary (enumerable3, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
          else if (datavalue is IEnumerable<KeyValuePair<string, long>> enumerable4) {
            return ConvertDictionary (enumerable4, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
          else if (datavalue is IEnumerable<KeyValuePair<string, uint>> enumerable5) {
            return ConvertDictionary (enumerable5, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
          else if (datavalue is IEnumerable<KeyValuePair<string, Newtonsoft.Json.Linq.JToken>> enumerable6) {
            return ConvertDictionary (enumerable6, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
          else if (datavalue is IEnumerable<KeyValuePair<string, object>> enumerable7) {
            return ConvertDictionary (enumerable7, a => ConvertAuto (a, typeof (object), skipErrorLog));
          }
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: conversion of {datavalue } into {type} failed", ex);
          throw;
        }
      }

      if (datavalue is IEnumerable<KeyValuePair<string, Newtonsoft.Json.Linq.JToken>> enumerable) {
        try {
          if (type == typeof (IDictionary<string, long>)) {
            return ConvertDictionary (enumerable, a => ConvertAuto (a, typeof (long), true));
          }
          else if (type == typeof (IDictionary<string, int>)) {
            return ConvertDictionary (enumerable, a => ConvertAuto (a, typeof (int), true));
          }
          else if (type == typeof (IDictionary<string, double>)) {
            return ConvertDictionary (enumerable, a => ConvertAuto (a, typeof (double), true));
          }
          else if (type == typeof (IDictionary<string, string>)) {
            return ConvertDictionary (enumerable, a => ConvertAuto (a, typeof (string), true));
          }
          else if (type == typeof (IDictionary<string, bool>)) {
            return ConvertDictionary (enumerable, a => ConvertAuto (a, typeof (bool), true));
          }
          else if (type == typeof (IDictionary<string, uint>)) {
            return ConvertDictionary (enumerable, a => ConvertAuto (a, typeof (uint), true));
          }
          else if (type == typeof (IDictionary<string, object>)) {
            return ConvertDictionary (enumerable, a => ConvertAuto (a, typeof (object), true));
          }
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: Json IDictionary conversion of {datavalue} into {type} failed", ex);
        }
      }
      else if (datavalue is IEnumerable<Newtonsoft.Json.Linq.JToken> jtokens) {
        try {
          if (type == typeof (IEnumerable<long>)) {
            return ConvertEnumerable (jtokens, a => ConvertAuto (a, typeof (long), true));
          }
          else if (type == typeof (IEnumerable<int>)) {
            return ConvertEnumerable (jtokens, a => ConvertAuto (a, typeof (int), true));
          }
          else if (type == typeof (IEnumerable<double>)) {
            return ConvertEnumerable (jtokens, a => ConvertAuto (a, typeof (double), true));
          }
          else if (type == typeof (IEnumerable<string>)) {
            return ConvertEnumerable (jtokens, a => ConvertAuto (a, typeof (string), true));
          }
          else if (type == typeof (IEnumerable<bool>)) {
            return ConvertEnumerable (jtokens, a => ConvertAuto (a, typeof (bool), true));
          }
          else if (type == typeof (IEnumerable<uint>)) {
            return ConvertEnumerable (jtokens, a => ConvertAuto (a, typeof (uint), true));
          }
          else if (type == typeof (IEnumerable<object>)) {
            return ConvertEnumerable (jtokens, a => ConvertAuto (a, typeof (object), true));
          }
        }
        catch (Exception ex) {
          log.Warn ($"ConvertAuto: Json IEnumerable conversion of {datavalue} into {type} failed", ex);
        }
      }

      if (datavalue.GetType ().Equals (typeof (string))) {
        // Try the parse methods
        // Note: string to int and long is managed by Convert.ChangeType
        if (type == typeof (double)) {
          try {
            return double.Parse (datavalue.ToString (), System.Globalization.CultureInfo.InvariantCulture);
          }
          catch (Exception) { }
        }
        else if (type == typeof (TimeSpan)) {
          try {
            return TimeSpan.Parse (datavalue.ToString ());
          }
          catch (Exception) { }
        }
        else if (type == typeof (DateTime)) {
          if (datavalue?.ToString ()?.EndsWith ("Z") ?? false) {
            try {
              return DateTime.ParseExact (datavalue.ToString (), "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture).ToUniversalTime ();
            }
            catch (Exception) { }
          }
          try {
            return DateTime.ParseExact (datavalue.ToString (), "yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
          }
          catch (Exception) { }
          try {
            return DateTime.ParseExact (datavalue.ToString (), "o", System.Globalization.CultureInfo.InvariantCulture);
          }
          catch (Exception) { }
        }
      }

      // Try with a constructor
      foreach (var constructor in type.GetConstructors ()) {
        var parameters = constructor.GetParameters ();
        if (1 == parameters.Length) {
          var parameterType = parameters[0].ParameterType;
          if (parameterType.Equals (type)) {
            continue;
          }
          object parameter;
          try {
            parameter = ConvertAuto (datavalue, parameterType, true);
          }
          catch (Exception ex) {
            log.Debug ($"ConvertAuto: conversion of {datavalue} to {parameterType} failed", ex);
            continue;
          }
          try {
            return constructor.Invoke (new object[] { parameter });
          }
          catch (Exception ex) {
            log.Warn ($"ConvertAuto: calling a specific constructor of type {type} on {parameter} failed", ex);
          }
        }
      }

      // Check if there is a ConvertToXxx method
      {
        var methodName = $"ConvertTo{type.Name}";
        var methodInfo = type.GetMethod (methodName);
        if (null != methodInfo) {
          if (methodInfo.GetParameters ().Any ()) {
            log.Error ($"ConvertAuto: ConvertTo method {methodName} contains parameters");
          } 
          else if (!methodInfo.ReturnType.Equals (type)) {
            log.Error ($"ConvertAuto: return type {methodInfo.ReturnType} of {methodName} does not match {type}");
          }
          else {
            try {
              return methodInfo.Invoke (datavalue, new object[] { });
            }
            catch (Exception ex) {
              log.Warn ($"ConvertAuto: method {methodName} failed", ex);
            }
          }
        }
      }

      try { // Try Convert.ChangeType
        return Convert.ChangeType (datavalue, type);
      }
      catch (Exception ex) {
        log.Warn ($"ConvertAuto: conversion of {datavalue} into {type}  with Convert.ChangeType failed", ex);
      }

      // Try to convert IEnumerable<T> or IList<T>
      try {
        if (type.IsGenericType) {
          var genericTypeDefinition = type.GetGenericTypeDefinition ();
          if (genericTypeDefinition.Equals (typeof (IEnumerable<object>).GetGenericTypeDefinition ())
            || genericTypeDefinition.Equals (typeof (IList<object>).GetGenericTypeDefinition ())
            || genericTypeDefinition.Equals (typeof (List<object>).GetGenericTypeDefinition ())) {
            var subType = type.GetGenericArguments ()[0];
            var returnType = typeof(List <>).MakeGenericType (new Type[] { subType });
            var list = Activator.CreateInstance (returnType);
            var addMethodInfo = returnType.GetMethod ("Add");
            foreach (var item in (System.Collections.IEnumerable)datavalue) {
              addMethodInfo.Invoke (list, new object[] { ConvertAuto (item, subType) });
            }
            return list;
          }
        }
      }
      catch (Exception ex) {
        log.Warn ($"ConvertAuto: IEnumerable conversion of {datavalue} into {type} failed", ex);
      }

      // Try to serialize and deserialize
      try {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject (datavalue);
        var converted = Newtonsoft.Json.JsonConvert.DeserializeObject (json, type);
        return converted;
      }
      catch (Exception ex) {
        if (!skipErrorLog) {
          log.Error ($"ConvertAuto: latest attempt of conversion of {datavalue} into {type} with Json serialization failed", ex);
          log.Error ($"ConvertAuto: conversion error at {System.Environment.StackTrace}");
        }
        else if (log.IsDebugEnabled) {
          log.Debug ($"ConvertAuto: latest attempt of conversion of {datavalue} into {type} with Json serialization failed", ex);
        }
        throw new InvalidCastException ($"ConvertAuto to {type} failed", ex);
      }
    }

    IDictionary<TKeyResult, TValueResult> ConvertDictionary<TKey, TValue, TKeyResult, TValueResult> (IEnumerable<KeyValuePair<TKey, TValue>> d, Func<TKey, TKeyResult> keyConvert, Func<TValue, TValueResult> valueConvert)
    {
      return d.ToDictionary<KeyValuePair<TKey, TValue>, TKeyResult, TValueResult> (a => keyConvert (a.Key), a => valueConvert (a.Value));
    }

    IDictionary<TKey, TValueResult> ConvertDictionary<TKey, TValue, TValueResult> (IEnumerable<KeyValuePair<TKey, TValue>> d, Func<TValue, TValueResult> valueConvert)
    {
      return ConvertDictionary<TKey, TValue, TKey, TValueResult> (d, a => a, valueConvert);
    }

    IEnumerable<U> ConvertEnumerable<T, U> (IEnumerable<T> x, Func<T, U> convert)
    {
      return x.Select (a => convert (a));
    }

    /// <summary>
    /// <see cref="IAutoConverter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="x"></param>
    /// <returns></returns>
    public bool IsCompatible<T> (object x)
    {
      return IsCompatible (x, typeof (T));
    }

    /// <summary>
    /// <see cref="IAutoConverter"/>
    /// 
    /// By default, just try to convert it !
    /// </summary>
    /// <param name="x"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public virtual bool IsCompatible (object x, Type t)
    {
      try {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsCompatible: check {x} and {t} are compatible");
        }
        ConvertAuto (x, t);
        if (log.IsDebugEnabled) {
          log.Debug ($"IsCompatible: {x} and {t} are compatible");
        }
        return true;
      }
      catch (Exception ex) {
        if (log.IsDebugEnabled) {
          log.Debug ($"IsCompatible: {x} is not compatible with {t}", ex);
        }
        return false;
      }
    }
    #endregion // IAutoConverter implementation

  }
}
