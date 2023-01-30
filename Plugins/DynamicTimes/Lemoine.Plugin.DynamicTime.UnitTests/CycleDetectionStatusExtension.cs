// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Database;
using Lemoine.Model;
using Pulse.Extensions.Database;
using System;
using System.Collections.Generic;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  internal class CycleDetectionStatusExtension
    : ICycleDetectionStatusExtension
  {
    static DateTime? s_cycleDetectionDateTime = null;

    public static void SetCycleDetectionDateTime (DateTime? dateTime)
    {
      s_cycleDetectionDateTime = dateTime;
    }

    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    public int CycleDetectionStatusPriority
    {
      get { return 1; }
    }

    public DateTime? GetCycleDetectionDateTime ()
    {
      return s_cycleDetectionDateTime;
    }

    public bool Initialize (IMachine machine)
    {
      return true;
    }
  }


}
