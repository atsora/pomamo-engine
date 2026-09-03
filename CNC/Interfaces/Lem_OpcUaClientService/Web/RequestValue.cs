// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: GPL-2.0

using System.Globalization;

namespace Lemoine.Cnc.OpcUaClientService.Web
{
  /// <summary>
  /// Extract the value of a /set request from its query string
  ///
  /// The type of the value is given by the name of the query parameter that carries it:
  /// long, int, double, boolean or string. The generic name v is a string.
  /// </summary>
  public static class RequestValue
  {
    /// <summary>
    /// Extract the value to write from the query string of a request
    /// </summary>
    /// <param name="query">not null</param>
    /// <param name="v">the extracted value</param>
    /// <returns>a value was found</returns>
    public static bool TryParse (IQueryCollection query, out object v)
    {
      ArgumentNullException.ThrowIfNull (query);

      if (query.TryGetValue ("long", out var longValue) && long.TryParse (longValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var l)) {
        v = l;
        return true;
      }
      if (query.TryGetValue ("int", out var intValue) && int.TryParse (intValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i)) {
        v = i;
        return true;
      }
      if (query.TryGetValue ("double", out var doubleValue) && double.TryParse (doubleValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var d)) {
        v = d;
        return true;
      }
      if (query.TryGetValue ("boolean", out var booleanValue) && bool.TryParse (booleanValue, out var b)) {
        v = b;
        return true;
      }
      if (query.TryGetValue ("string", out var stringValue)) {
        v = stringValue.ToString ();
        return true;
      }
      if (query.TryGetValue ("v", out var genericValue)) {
        v = genericValue.ToString ();
        return true;
      }

      v = "";
      return false;
    }
  }
}
