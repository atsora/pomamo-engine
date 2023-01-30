// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Extensions.Alert;

namespace Lemoine.Plugin.StackLightAlert
{
  public class ConfigEMailExtension : Lemoine.Extensions.Alert.IConfigEMailExtension
  {
    static readonly string RED_LIGHT_ON = "Red light on";

    public string DataType
    {
      get
      {
        return "StackLight";
      }
    }

    public string DataTypeText
    {
      get {
        return "Stack light";
      }
    }

    public IEnumerable<string> InputList
    {
      get
      {
        var list = new List<string> ();
        list.Add (RED_LIGHT_ON);
        return list;
      }
    }

    public ConfigEMailInputType InputType
    {
      get
      {
        return ConfigEMailInputType.List;
      }
    }

    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    public bool Match (string configInput, string v)
    {
      int intValue;
      if (!int.TryParse (v, out intValue)) {
        return false;
      }
      StackLight stackLight = (StackLight)intValue;
      if (string.Equals (configInput, RED_LIGHT_ON, StringComparison.InvariantCultureIgnoreCase)) {
        return stackLight.HasFlag (StackLightColor.Red, StackLightStatus.On);
      }
      return false;
    }
  }
}
