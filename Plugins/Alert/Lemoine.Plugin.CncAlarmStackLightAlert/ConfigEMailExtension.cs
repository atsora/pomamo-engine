// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions.Alert;
using Lemoine.Model;

namespace Lemoine.Plugin.CncAlarmStackLightAlert
{
  public class ConfigEMailExtension : IConfigEMailExtension
  {
    // green light is always ignored
    static readonly string YELLOW_RED_LIGHTS = "Yellow or red light";
    static readonly string RED_LIGHTS = "Only red light";

    public string DataType {
      get {
        return "CncValueWithAlarm"; // "Pair<CncValue,List<CncAlarm>>"
      }
    }

    public string DataTypeText {
      get {
        return "Cnc alarms - Stack light";
      }
    }

    public IEnumerable<string> InputList {
      get {
        var list = new List<string>();
        list.Add(YELLOW_RED_LIGHTS);
        list.Add(RED_LIGHTS);
        return list;
      }
    }

    public ConfigEMailInputType InputType {
      get {
        return ConfigEMailInputType.List;
      }
    }

    public bool UniqueInstance {
      get {
        return true;
      }
    }

    public bool Match(string configInput, string v)
    {
      int intValue;
      if (!int.TryParse (v, out intValue)) {
        return false;
      }
      var stackLight = (StackLight)intValue;
      
      bool isYellow = stackLight.IsOnOrFlashingIfAcquired(StackLightColor.Yellow);
      bool isRed = stackLight.IsOnOrFlashingIfAcquired(StackLightColor.Red);
      
      if (string.Equals(configInput, RED_LIGHTS, StringComparison.InvariantCultureIgnoreCase)) {
        return isRed;
      }

      if (string.Equals(configInput, YELLOW_RED_LIGHTS, StringComparison.InvariantCultureIgnoreCase)) {
        return isYellow || isRed;
      }

      return false;
    }
  }
}
