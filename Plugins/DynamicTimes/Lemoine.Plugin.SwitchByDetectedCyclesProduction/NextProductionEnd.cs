// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.Core.Cache;

namespace Lemoine.Plugin.SwitchByDetectedCyclesProduction
{
  public class NextProductionEnd
    : Switch
    , IDynamicTimeExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (NextProductionEnd).FullName);

    static readonly string SUFFIX = "NextProductionEnd";

    public NextProductionEnd ()
      : base (SUFFIX)
    { }
  }
}
