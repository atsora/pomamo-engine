// Copyright (C) 2024 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions.Alert;

namespace Lemoine.Plugin.CncAlarmSequenceAlert
{
  public class ConfigEMailExtension : IConfigEMailExtension
  {
    static readonly string ALL_ALARMS = "Focused + unknown"; // Focused + unknown, ignored alarms are still ignored
    static readonly string FOCUSED_ALARMS = "Focused alarms with sequence";

    public string DataType => "CncAlarmSequence";

    public string DataTypeText => "Cnc alarms with sequence";

    public IEnumerable<string> InputList
    {
      get {
        var list = new List<string> ();
        list.Add (ALL_ALARMS);
        list.Add (FOCUSED_ALARMS);
        return list;
      }
    }

    public ConfigEMailInputType InputType => ConfigEMailInputType.List;

    public bool UniqueInstance => true;

    public bool Match (string configInput, string v)
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
