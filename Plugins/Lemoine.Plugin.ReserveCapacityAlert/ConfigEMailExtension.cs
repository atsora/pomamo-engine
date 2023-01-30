// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Extensions.Alert;
using Lemoine.Extensions.Business.Group;

namespace Lemoine.Plugin.ReserveCapacityAlert
{
  public class ConfigEMailExtension : IConfigEMailExtension
  {
    public string DataType {
      get {
        return "ReserveCapacityInfo";
      }
    }

    public string DataTypeText {
      get {
        return "Reserve capacity alert";
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

    public bool UniqueInstance {
      get {
        return true;
      }
    }

    public bool Match(string configInput, string v)
    {
      var itemlist = Lemoine.Collections.EnumerableString.ParseListString (configInput);
      return (null != itemlist.FirstOrDefault (x => x.Equals(v)));
    }
  }
}
