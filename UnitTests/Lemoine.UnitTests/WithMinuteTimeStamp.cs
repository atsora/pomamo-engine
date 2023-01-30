// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected WithMinuteTimeStamp (DateTime baseDateTime)
      : base (baseDateTime)
    {
    }
    #endregion // Constructors

    #region Methods
    protected override DateTime T(int n)
    {
      return m_baseDateTime.AddMinutes (n);
    }
    #endregion // Methods
  }
}
