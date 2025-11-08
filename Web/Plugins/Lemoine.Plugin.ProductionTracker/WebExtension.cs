// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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
    public bool UniqueInstance => true;

    public Assembly GetAssembly () => this.GetType ().Assembly;
  }
}
