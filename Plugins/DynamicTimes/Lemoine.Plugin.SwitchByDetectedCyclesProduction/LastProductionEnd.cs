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
  public class LastProductionEnd
    : Switch
    , IDynamicTimeExtension
  {
    readonly ILog log = LogManager.GetLogger (typeof (LastProductionEnd).FullName);

    static readonly string SUFFIX = "LastProductionEnd";

    public LastProductionEnd ()
      : base (SUFFIX)
    { }
  }
}
