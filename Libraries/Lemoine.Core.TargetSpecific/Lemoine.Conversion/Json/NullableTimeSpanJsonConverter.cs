// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

#if NETCOREAPP3_1_OR_GREATER

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.Conversion.Json
{
  /// <summary>
  /// TimeSpanJsonConverter
  /// </summary>
  public class NullableTimeSpanJsonConverter : JsonConverter<TimeSpan?>
  {
    readonly ILog log = LogManager.GetLogger (typeof (TimeSpanJsonConverter).FullName);

    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override TimeSpan? Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var s = reader.GetString ();
      if (string.IsNullOrEmpty (s)) {
        return null;
      }
      else {
        return TimeSpan.Parse (s);
      }
    }

    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write (Utf8JsonWriter writer, TimeSpan? value, JsonSerializerOptions options) => writer.WriteStringValue (value?.ToString () ?? "");
  }
}

#endif // NETCOREAPP3_1_OR_GREATER
