// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using Lemoine.Core.Log;

namespace Lemoine.Stamping.AxisPropertyGetters
{
  /// <summary>
  /// BasicAxisPropertyGetter
  /// </summary>
  public class BasicAxisPropertyGetter : IAxisPropertyGetter
  {
    readonly ILog log = LogManager.GetLogger (typeof (BasicAxisPropertyGetter).FullName);

    /// <summary>
    /// Default unit:
    /// <item>Default</item>
    /// <item>Mm</item>
    /// <item>In</item>
    /// </summary>
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingDefault)]
    [JsonConverter (typeof (JsonStringEnumConverter))]
    public AxisUnit DefaultUnit { get; set; } = AxisUnit.Default;

    /// <summary>
    /// Default velocity from X, Y and Z
    /// 
    /// If null, it will be taken from config key "Stamping.Axis.DefaultMaxVelocity"
    /// </summary>
    [JsonIgnore (Condition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull)]
    public double? MaxVelocity { get; set; } = null;

    /// <summary>
    /// Constructor
    /// </summary>
    public BasicAxisPropertyGetter ()
    {
    }

    /// <summary>
    /// <see cref="IAxisPropertyGetter"/>
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    public AxisUnit GetDefaultUnit (string axis)
    {
      if (this.DefaultUnit.Equals (AxisUnit.Default)) {
        // Check if it is set by chance in the config table
        var defaultUnit = (AxisUnit) Lemoine.Info.ConfigSet.LoadAndGet ("Stamping.Axis.DefaultUnit", 0);
        return defaultUnit;
      }
      else {
      return DefaultUnit;
    }
    }

    /// <summary>
    /// <see cref="IAxisPropertyGetter"/>
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    public double? GetMaxAcceleration (string axis)
    {
      return null;
    }

    /// <summary>
    /// <see cref="IAxisPropertyGetter"/>
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    public double? GetMaxDeceleration (string axis)
    {
      return null;
    }

    /// <summary>
    /// <see cref="IAxisPropertyGetter"/>
    /// </summary>
    /// <param name="axis"></param>
    /// <returns></returns>
    public double? GetMaxVelocity (string axis)
    {
      switch (axis) {
      case "X" or "Y" or "Z":
        if (this.MaxVelocity is null) {
          var maxVelocity = Lemoine.Info.ConfigSet.LoadAndGet<double?> ("Stamping.Axis.DefaultMaxVelocity", null);
          return maxVelocity;
        }
        else {
          return this.MaxVelocity;
        }
      default:
        return null;
      }
    }
  }
}
