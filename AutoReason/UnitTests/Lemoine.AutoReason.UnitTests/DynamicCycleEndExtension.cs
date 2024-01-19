// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.UnitTests;
using System;

namespace Lemoine.AutoReason.UnitTests
{
  class DynamicCycleEndExtension : WithSecondTimeStamp, IDynamicTimeExtension
  {
    static int s_step = 0;

    public DynamicCycleEndExtension () : base (DateTime.Today.AddHours (-12))
    {
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      return true;
    }

    public IMachine Machine
    {
      get; set;
    }

    public string Name
    {
      get
      {
        return "CycleEnd";
      }
    }

    public bool UniqueInstance
    {
      get
      {
        return true;
      }
    }

    public bool IsApplicable ()
    {
      return true;
    }

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime at)
    {
      throw new NotImplementedException ();
    }

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      IDynamicTimeResponse answer = null;

      answer = s_step++ switch {
        0 => this.CreateNotApplicable (),// Not in a cycle yet
        1 => this.CreateFinal (T (5)),// End of the first cycle
        2 => this.CreateNoData (),// No end for the second cycle
        3 => this.CreateFinal (T (20)),// End of the third cycle
        _ => this.CreatePending (),
      };
      return answer;
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      return TimeSpan.FromTicks (0);
    }

    public static void Reset ()
    {
      s_step = 0;
    }
  }
}
