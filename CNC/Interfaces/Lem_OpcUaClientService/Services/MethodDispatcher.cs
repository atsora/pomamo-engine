// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using System.Reflection;

namespace Lemoine.Cnc.OpcUaClientService.Services
{
  /// <summary>
  /// Map the method and property names of the requests to the members of <see cref="Lemoine.Cnc.OpcUaClient"/>
  /// </summary>
  public static class MethodDispatcher
  {
    /// <summary>
    /// Is the method a direct read, so a read that neither registers the parameter nor uses the cache ?
    /// </summary>
    /// <param name="method">nullable</param>
    public static bool IsDirectReadMethod (string? method) => method?.ToLowerInvariant () switch {
      "directread" or "directreadasync" or "directreaddouble" => true,
      _ => false
    };

    /// <summary>
    /// Is the method a write method ?
    /// </summary>
    /// <param name="method">nullable</param>
    public static bool IsWriteMethod (string? method) => method?.ToLowerInvariant () switch {
      "write" or "writeasync" or "set" => true,
      _ => false
    };

    /// <summary>
    /// Read the value of an already registered parameter
    /// </summary>
    /// <param name="client">not null</param>
    /// <param name="method">not null</param>
    /// <param name="parameter">not null</param>
    /// <returns>nullable</returns>
    /// <exception cref="UnknownMethodException">the method is not one of the get methods of the module</exception>
    public static object? InvokeGet (Lemoine.Cnc.OpcUaClient client, string method, string parameter) =>
      method.ToLowerInvariant () switch {
        "" or "get" => client.Get (parameter),
        "getbool" => client.GetBool (parameter),
        "getchar" => client.GetChar (parameter),
        "getbyte" => client.GetByte (parameter),
        "getint16" => client.GetInt16 (parameter),
        "getuint16" => client.GetUInt16 (parameter),
        "getint32" => client.GetInt32 (parameter),
        "getuint32" => client.GetUInt32 (parameter),
        "getint" => client.GetInt (parameter),
        "getuint" => client.GetUInt (parameter),
        "getint64" => client.GetInt64 (parameter),
        "getuint64" => client.GetUInt64 (parameter),
        "getfloat" => client.GetFloat (parameter),
        "getdouble" => client.GetDouble (parameter),
        "getstring" => client.GetString (parameter),
        _ => throw new UnknownMethodException (method)
      };

    /// <summary>
    /// Read a value without registering the parameter and without using the cache
    /// </summary>
    /// <param name="client">not null</param>
    /// <param name="method">not null</param>
    /// <param name="address">not null</param>
    /// <returns>nullable</returns>
    /// <exception cref="UnknownMethodException">the method is not one of the direct read methods</exception>
    public static async Task<object?> InvokeDirectReadAsync (Lemoine.Cnc.OpcUaClient client, string method, string address)
    {
      var result = await client.DirectReadAsync (address);
      return method.ToLowerInvariant () switch {
        "directread" or "directreadasync" => result,
        "directreaddouble" => Convert.ToDouble (result),
        _ => throw new UnknownMethodException (method)
      };
    }

    /// <summary>
    /// Read a property of the module
    /// </summary>
    /// <param name="client">not null</param>
    /// <param name="property">not null</param>
    /// <returns>nullable</returns>
    /// <exception cref="UnknownPropertyException">the property does not exist or cannot be read</exception>
    public static object? GetProperty (Lemoine.Cnc.OpcUaClient client, string property)
    {
      var propertyInfo = GetPropertyInfo (property);
      if (propertyInfo is null || !propertyInfo.CanRead) {
        throw new UnknownPropertyException (property);
      }
      return propertyInfo.GetValue (client);
    }

    /// <summary>
    /// Does the module have such a property, and can it be written ?
    /// </summary>
    /// <param name="property">not null</param>
    public static bool IsWritableProperty (string property)
    {
      var propertyInfo = GetPropertyInfo (property);
      return propertyInfo is not null && propertyInfo.CanWrite && propertyInfo.SetMethod is not null && propertyInfo.SetMethod.IsPublic;
    }

    /// <summary>
    /// Set a property of the module
    /// </summary>
    /// <param name="client">not null</param>
    /// <param name="property">not null</param>
    /// <param name="v">not null</param>
    /// <exception cref="UnknownPropertyException">the property does not exist or cannot be written</exception>
    public static void SetProperty (Lemoine.Cnc.OpcUaClient client, string property, object v)
    {
      if (!IsWritableProperty (property)) {
        throw new UnknownPropertyException (property);
      }
      var propertyInfo = GetPropertyInfo (property)!;
      propertyInfo.SetValue (client, Convert.ChangeType (v, propertyInfo.PropertyType, System.Globalization.CultureInfo.InvariantCulture));
    }

    static PropertyInfo? GetPropertyInfo (string property) =>
      typeof (Lemoine.Cnc.OpcUaClient)
        .GetProperty (property, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
  }
}
