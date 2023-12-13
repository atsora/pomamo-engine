// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lemoine.Model
{
  /// <summary>
  /// Stack light structure for Json serialization
  /// </summary>
  public struct StackLightJson
  {
    /// <summary>
    /// Red color: null / "Off" / "On" / "Flashing"
    /// </summary>
    [JsonInclude]
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Red;

    /// <summary>
    /// Yellow color: null / "Off" / "On" / "Flashing"
    /// </summary>
    [JsonInclude]
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Yellow;

    /// <summary>
    /// Red color: null / "Off" / "On" / "Flashing"
    /// </summary>
    [JsonInclude]
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Green;

    /// <summary>
    /// Blue color: null / "Off" / "On" / "Flashing"
    /// </summary>
    [JsonInclude]
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Blue;

    /// <summary>
    /// White color: null / "Off" / "On" / "Flashing"
    /// </summary>
    [JsonInclude]
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string White;

    /// <summary>
    /// Constructor
    /// </summary>
    public StackLightJson ()
      : this (StackLight.None)
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="stackLight"></param>
    public StackLightJson (StackLight stackLight)
    {
      this.Red = stackLight.GetStatus (StackLightColor.Red).ToStringIfAcquired ();
      this.Yellow = stackLight.GetStatus (StackLightColor.Yellow).ToStringIfAcquired ();
      this.Green = stackLight.GetStatus (StackLightColor.Green).ToStringIfAcquired ();
      this.Blue = stackLight.GetStatus (StackLightColor.Blue).ToStringIfAcquired ();
      this.White = stackLight.GetStatus (StackLightColor.White).ToStringIfAcquired ();
    }

    /// <summary>
    /// Convert a <see cref="StackLightJson"/> to <see cref="StackLight" />
    /// </summary>
    /// <returns></returns>
    public StackLight ConvertToStackLight ()
    {
      return new StackLight ()
        .Set (StackLightColor.Red, (StackLightStatus)Enum.Parse (typeof (StackLightStatus), this.Red ?? "NotAcquired"))
        .Set (StackLightColor.Yellow, (StackLightStatus)Enum.Parse (typeof (StackLightStatus), this.Yellow ?? "NotAcquired"))
        .Set (StackLightColor.Green, (StackLightStatus)Enum.Parse (typeof (StackLightStatus), this.Green ?? "NotAcquired"))
        .Set (StackLightColor.Blue, (StackLightStatus)Enum.Parse (typeof (StackLightStatus), this.Blue ?? "NotAcquired"))
        .Set (StackLightColor.White, (StackLightStatus)Enum.Parse (typeof (StackLightStatus), this.White ?? "NotAcquired"))
        ;
    }
  }

  /// <summary>
  /// Stack light status
  /// </summary>
  public enum StackLightStatus // On two bits
  {
    /// <summary>
    /// No stack light of this color was acquired
    /// </summary>
    NotAcquired = 0,
    /// <summary>
    /// The stack light is acquired but off (not lit)
    /// </summary>
    Off = 1,
    /// <summary>
    /// The stack light is solid-on (continous)
    /// </summary>
    On = 2,
    /// <summary>
    /// The stack light is flashing
    /// </summary>
    Flashing = 3,
  }

  /// <summary>
  /// Bit offset for a stack light color (every two bits)
  /// </summary>
  public enum StackLightColor // bit offset
  {
    /// <summary>
    /// Red color
    /// </summary>
    Red = 0,
    /// <summary>
    /// Yellow color
    /// </summary>
    Yellow = 2,
    /// <summary>
    /// Green color
    /// </summary>
    Green = 4,
    /// <summary>
    /// Blue color
    /// </summary>
    Blue = 6,
    /// <summary>
    /// White color
    /// </summary>
    White = 8,
    // NextColors = 10, 12, 14, 16...
  }

  /// <summary>
  /// Stack light
  /// </summary>
  [Flags]
  [System.Text.Json.Serialization.JsonConverter (typeof (StackLightJsonConverter))]
  public enum StackLight
  {
    /// <summary>
    /// No light
    /// </summary>
    None = 0,
    /// <summary>
    /// Red acquired but off
    /// </summary>
    RedOff = StackLightStatus.Off << StackLightColor.Red,
    /// <summary>
    /// Red flashing
    /// </summary>
    RedFlashing = StackLightStatus.Flashing << StackLightColor.Red,
    /// <summary>
    /// Red on
    /// </summary>
    RedOn = StackLightStatus.On << StackLightColor.Red,
    /// <summary>
    /// Yellow acquired but off
    /// </summary>
    YellowOff = StackLightStatus.Off << StackLightColor.Yellow,
    /// <summary>
    /// Yellow flashing
    /// </summary>
    YellowFlashing = StackLightStatus.Flashing << StackLightColor.Yellow,
    /// <summary>
    /// Yellow on
    /// </summary>
    YellowOn = StackLightStatus.On << StackLightColor.Yellow,
    /// <summary>
    /// Green acquired but off
    /// </summary>
    GreenOff = StackLightStatus.Off << StackLightColor.Green,
    /// <summary>
    /// Green flashing
    /// </summary>
    GreenFlashing = StackLightStatus.Flashing << StackLightColor.Green,
    /// <summary>
    /// Green on
    /// </summary>
    GreenOn = StackLightStatus.On << StackLightColor.Green,
    /// <summary>
    /// Blue acquired but off
    /// </summary>
    BlueOff = StackLightStatus.Off << StackLightColor.Blue,
    /// <summary>
    /// Blue flashing
    /// </summary>
    BlueFlashing = StackLightStatus.Flashing << StackLightColor.Blue,
    /// <summary>
    /// Blue on
    /// </summary>
    BlueOn = StackLightStatus.On << StackLightColor.Blue,
    /// <summary>
    /// White acquired but off
    /// </summary>
    WhiteOff = StackLightStatus.Off << StackLightColor.White,
    /// <summary>
    /// White flashing
    /// </summary>
    WhiteFlashing = StackLightStatus.Flashing << StackLightColor.White,
    /// <summary>
    /// White on
    /// </summary>
    WhiteOn = StackLightStatus.On << StackLightColor.White,
  }

  /// <summary>
  /// Associated json converter
  /// </summary>
  public class StackLightJsonConverter : JsonConverter<StackLight>
  {
    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public override StackLight Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      var stackLightJson = System.Text.Json.JsonSerializer.Deserialize<StackLightJson> (ref reader, options);
      return stackLightJson.ConvertToStackLight ();
    }

    /// <summary>
    /// <see cref="JsonConverter{T}"/>
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    /// <exception cref="NotImplementedException"></exception>
    public override void Write (Utf8JsonWriter writer, StackLight value, JsonSerializerOptions options)
    {
      var stackLightJson = new StackLightJson (value);
      System.Text.Json.JsonSerializer.Serialize (writer, stackLightJson, options);
    }
  }

  /// <summary>
  /// Extensions to StackLight
  /// </summary>
  public static class StackLightExtensions
  {
    /// <summary>
    /// Create a stack light flag from a color and a status
    /// </summary>
    /// <param name="color"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public static StackLight Create (StackLightColor color, StackLightStatus status)
    {
      return (StackLight)((int)status << (int)color);
    }

    /// <summary>
    /// Check if other is included into t
    /// </summary>
    /// <param name="t"></param>
    /// <param name="other"></param>
    /// <returns></returns>
    public static bool HasFlag (this StackLight t, StackLight other)
    {
      return other == (t & other);
    }

    /// <summary>
    /// Check if the stack light has the specified color / status
    /// </summary>
    /// <param name="t"></param>
    /// <param name="color"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public static bool HasFlag (this StackLight t, StackLightColor color, StackLightStatus status)
    {
      return t.HasFlag (Create (color, status));
    }

    /// <summary>
    /// Get the stack light status for a stack light color
    /// </summary>
    /// <param name="t"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static StackLightStatus GetStatus (this StackLight t, StackLightColor color)
    {
      var colorFilter = (int)t & (3 << (int)color);
      return (StackLightStatus)(colorFilter >> (int)color);
    }

    /// <summary>
    /// Set a status for a specified color
    /// </summary>
    /// <param name="t"></param>
    /// <param name="color"></param>
    /// <param name="status"></param>
    /// <returns></returns>
    public static StackLight Set (this StackLight t, StackLightColor color, StackLightStatus status)
    {
      var cleared = t & ~Create (color, (StackLightStatus)3); // 3 = O11
      return cleared | Create (color, status);
    }

    /// <summary>
    /// Turn on or off a specified color
    /// </summary>
    /// <param name="t"></param>
    /// <param name="color"></param>
    /// <param name="on">On else Off</param>
    /// <returns></returns>
    public static StackLight Set (this StackLight t, StackLightColor color, bool on)
    {
      return t.Set (color, on ? StackLightStatus.On : StackLightStatus.Off);
    }

    /// <summary>
    /// Is the color on the stack light on or flashing?
    /// If the color is not acquired, an exception is returned
    /// </summary>
    /// <param name="t"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    /// <exception cref="Exception">the color is not acquired</exception>
    public static bool IsOnOrFlashing (this StackLight t, StackLightColor color)
    {
      return t.GetStatus (color) switch {
        StackLightStatus.NotAcquired => throw new Exception ("Color not acquired"),
        StackLightStatus.Off => false,
        _ => true,
      };
    }

    /// <summary>
    /// Is the color on the stack light on or flashing?
    /// If the color is not acquired, false is returned
    /// </summary>
    /// <param name="t"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static bool IsOnOrFlashingIfAcquired (this StackLight t, StackLightColor color)
    {
      return t.GetStatus (color) switch {
        StackLightStatus.NotAcquired or StackLightStatus.Off => false,
        _ => true,
      };
    }

    /// <summary>
    /// ToString version
    /// 
    /// <item>R: red</item>
    /// <item>Y: yellow</item>
    /// <item>G: green</item>
    /// <item>B: blue</item>
    /// <item>W: white</item>
    /// <item>-: off</item>
    /// <item>*: flashing</item>
    /// <item>+: on</item>
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string ToString (this StackLight t)
    {
      string s = "StackLight: ";
      if (!t.HasFlag (StackLightColor.Red, StackLightStatus.NotAcquired)) {
        s += "R";
        s += t.GetStatusString (StackLightColor.Red);
      }
      if (!t.HasFlag (StackLightColor.Yellow, StackLightStatus.NotAcquired)) {
        s += "Y";
        s += t.GetStatusString (StackLightColor.Yellow);
      }
      if (!t.HasFlag (StackLightColor.Green, StackLightStatus.NotAcquired)) {
        s += "G";
        s += t.GetStatusString (StackLightColor.Green);
      }
      if (!t.HasFlag (StackLightColor.Blue, StackLightStatus.NotAcquired)) {
        s += "B";
        s += t.GetStatusString (StackLightColor.Blue);
      }
      if (!t.HasFlag (StackLightColor.White, StackLightStatus.NotAcquired)) {
        s += "W";
        s += t.GetStatusString (StackLightColor.White);
      }
      return s;
    }

    static string GetStatusString (this StackLight t, StackLightColor color)
    {
      switch (t.GetStatus (color)) {
        case StackLightStatus.Off:
          return "-";
        case StackLightStatus.Flashing:
          return "*";
        case StackLightStatus.On:
          return "+";
        default:
          return "";
      }
    }
  }

  /// <summary>
  /// Extensions to StackLight
  /// </summary>
  public static class StackLightStatusExtensions
  {
    /// <summary>
    /// Return an empty string it NotAcquired
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string ToStringIfAcquired (this StackLightStatus t)
    {
      return t switch {
        StackLightStatus.NotAcquired => null,
        _ => t.ToString ()
      };
    }
  }
}
