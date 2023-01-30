// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;

namespace Lemoine.Plugin.ProductionTracker
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
