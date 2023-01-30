// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Core.Log;
using Newtonsoft.Json;

namespace Lemoine.Conversion.JavaScript
{
  /// <summary>
  /// TimeSpan converter for javascript
  /// </summary>
  public class TimeSpanConverter: JsonConverter<TimeSpan>
  {
    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="serializer"></param>
    public override void WriteJson (JsonWriter writer, TimeSpan value, JsonSerializer serializer)
    {
      writer.WriteValue (value.ToString ());
    }

    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="objectType"></param>
    /// <param name="existingValue"></param>
    /// <param name="hasExistingValue"></param>
    /// <param name="serializer"></param>
    /// <returns></returns>
    public override TimeSpan ReadJson (JsonReader reader, Type objectType, TimeSpan existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
      return TimeSpan.Parse ((string)reader.Value);
    }
  }
}
