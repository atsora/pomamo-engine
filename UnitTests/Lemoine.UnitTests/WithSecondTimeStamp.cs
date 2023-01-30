// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;

namespace Lemoine.UnitTests
{
  /// <summary>
  /// Description of UnitTestWithSecondTimeStamps.
  /// </summary>
  public class WithSecondTimeStamp: WithTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (WithSecondTimeStamp).FullName);

    #region Getters / Setters
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected WithSecondTimeStamp (DateTime baseDateTime)
      : base (baseDateTime)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Convienent function to create a DateTime relative to the origin
    /// </summary>
    /// <param name="n">number of seconds to add</param>
    /// <returns></returns>
    protected override DateTime T(int n)
    {
      return m_baseDateTime.AddSeconds (n);
    }
    #endregion // Methods
  }
}
