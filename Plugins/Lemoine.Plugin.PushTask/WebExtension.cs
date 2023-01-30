// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Web.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Lemoine.Plugin.PushTask
{
  public class WebExtension
    : Lemoine.Extensions.Web.IWebExtension
  {
    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    public Assembly GetAssembly ()
    {
      return this.GetType ().Assembly;
    }
  }
}
