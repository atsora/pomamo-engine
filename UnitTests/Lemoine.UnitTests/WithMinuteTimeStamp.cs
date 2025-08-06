// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.UnitTests
{
  /// <summary>
  /// Description of UnitTestWithMinuteTimeStamps.
  /// </summary>
  public class WithMinuteTimeStamp: WithTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WithMinuteTimeStamp).FullName);
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected WithMinuteTimeStamp (DateTime baseDateTime)
      : base (baseDateTime)
    {
    }

    #region Methods
    protected override DateTime T(double n)
    {
      return m_baseDateTime.AddMinutes (n);
    }
    #endregion // Methods
  }
}
