// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.CncValueTime
{
  public class LambdaConditionEvaluator
  {
    readonly Func<object, bool> m_f;

    public LambdaConditionEvaluator (Func<object, bool> f)
    {
      m_f = f;
    }

    public bool Run (object x)
    {
      return m_f (x);
    }
  }
}
