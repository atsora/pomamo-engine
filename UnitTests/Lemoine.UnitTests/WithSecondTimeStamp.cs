// Copyright (C) 2009-2023 Lemoine Automation Technologies
// Copyright (C) 2025 Atsora Solutions
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

    /// <summary>
    /// Description of the constructor
    /// </summary>
    protected WithSecondTimeStamp (DateTime baseDateTime)
      : base (baseDateTime)
    {
    }

    /// <summary>
    /// Convienent function to create a DateTime relative to the origin
    /// </summary>
    /// <param name="n">number of seconds to add</param>
    /// <returns></returns>
    protected override DateTime T(double n)
    {
      return m_baseDateTime.AddSeconds (n);
    }
  }
}
