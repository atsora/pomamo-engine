// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions.Alert;

namespace Lemoine.Plugin.CncAlarmAlert
{
  public class ConfigEMailExtension : IConfigEMailExtension
  {
    static readonly string ALL_ALARMS = "Focused + unknown"; // Focused + unknown, ignored alarms are still ignored
    static readonly string FOCUSED_ALARMS = "Only focused alarms";

    public string DataType {
      get {
        return "CncAlarm";
      }
    }

    public string DataTypeText {
      get {
        return "Cnc alarms";
      }
    }

    public IEnumerable<string> InputList {
      get {
        var list = new List<string>();
        list.Add(ALL_ALARMS);
        list.Add(FOCUSED_ALARMS);
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
      if (string.Equals (configInput, ALL_ALARMS, StringComparison.InvariantCultureIgnoreCase)) {
        return true;
      }
      else { // Focused alarms only
        if (string.IsNullOrEmpty (v)) {
          // No focus parameter
          return false;
        }
        else {
          bool bValue;
          if (!bool.TryParse (v, out bValue)) {
            return false;
          }
          return bValue;
        }
      }
    }
  }
}
