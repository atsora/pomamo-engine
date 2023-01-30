// Copyright (C) 2009-2023 Lemoine Automation Technologies
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

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="baseDateTime">in UTC !</param>
    protected WithDayTimeStamp (DateTime baseDateTime)
      : base (baseDateTime)
    {
    }
    #endregion // Constructors

    #region Methods
    protected override DateTime T(int n)
    {
      return m_baseDateTime.AddDays (n);
    }
    #endregion // Methods
  }
}
