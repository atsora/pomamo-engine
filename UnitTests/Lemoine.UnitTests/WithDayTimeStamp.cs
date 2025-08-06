// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.UnitTests
{
  /// <summary>
  /// Description of WithDayTimeStamp.
  /// </summary>
  public class WithDayTimeStamp: WithTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WithDayTimeStamp).FullName);

    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="baseDateTime">in UTC !</param>
    protected WithDayTimeStamp (DateTime baseDateTime)
      : base (baseDateTime)
    {
    }

    protected override DateTime T(double n)
    {
      return m_baseDateTime.AddDays (n);
    }
  }
}
