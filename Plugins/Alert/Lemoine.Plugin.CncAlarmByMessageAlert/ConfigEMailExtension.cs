// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Extensions;
using Lemoine.Extensions.Alert;

namespace Lemoine.Plugin.CncAlarmByMessageAlert
{
  public class ConfigEMailExtension : MultipleInstanceConfigurableExtension<Configuration>, IConfigEMailExtension
  {

    Configuration m_configuration = null;

    public string DataType {
      get {
        if (null == m_configuration) {
          Initialize ();
        }
        bool byMessage = m_configuration.FilterByMessage;
        if (byMessage) {
          return "CncAlarmByMessage";
        }
        else {
          return "CncAlarmByNumber";
        }
      }
    }

    public string DataTypeText {
      get {
        if (null == m_configuration) {
          Initialize ();
        }
        bool byMessage = m_configuration.FilterByMessage;
        if (byMessage) {
          return "Cnc alarms by message";
        }
        else {
          return "Cnc alarms by number";
        }
      }
    }

    public IEnumerable<string> InputList {
      get {
        var list = new List<string>();
        return list;
      }
    }

    public ConfigEMailInputType InputType {
      get {
        return ConfigEMailInputType.Text;
      }
    }

    public bool Match(string configInput, string v)
    {
      return v.Contains (configInput);
    }

    public bool Initialize ()
    {
      if (!LoadConfiguration (out m_configuration)) {
        return false;
      }
      return true;
    }
  }
}
