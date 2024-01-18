// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Plugins.UnitTests
{
  /// <summary>
  /// Unit tests for the plugin IdleTimePerPeriod
  /// </summary>
  [TestFixture]
  public class Test_IdleTimePerPeriod
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Test_IdleTimePerPeriod).FullName);

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestMethod()
    {
      int value = 0;
      int expected = 0;
      Assert.That (value, Is.EqualTo (expected));
    }
  }
}
