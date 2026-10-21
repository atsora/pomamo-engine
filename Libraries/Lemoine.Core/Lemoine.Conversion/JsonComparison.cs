// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

#if !NET40

using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lemoine.Core.Log;

namespace Lemoine.Conversion
{
  /// <summary>
  /// Compare JSON texts by their content
  /// </summary>
  public static class JsonComparison
  {
    static readonly ILog log = LogManager.GetLogger (typeof (JsonComparison).FullName);

    /// <summary>
    /// Test if two JSON texts have the same content
    ///
    /// The white spaces and the order of the object properties are not considered,
    /// the order of the array items is.
    ///
    /// A null text is only equal to another null text.
    /// A text that is not a valid JSON is only equal to the same text
    /// </summary>
    /// <param name="json1"></param>
    /// <param name="json2"></param>
    /// <returns></returns>
    public static bool AreEqual (string json1, string json2)
    {
      if (string.Equals (json1, json2, StringComparison.Ordinal)) {
        return true;
      }
      if (json1 is null || json2 is null) {
        return false;
      }

      try {
        var node1 = JsonNode.Parse (json1);
        var node2 = JsonNode.Parse (json2);
        return JsonNode.DeepEquals (node1, node2);
      }
      catch (Exception ex) when (ex is JsonException || ex is ArgumentException) {
        // ArgumentException: duplicate property names in an object
        log.Error ($"AreEqual: {json1} or {json2} is not a valid JSON", ex);
        return false;
      }
    }
  }
}

#endif // !NET40
