// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Info.UnitTests
{
  /// <summary>
  /// Unit tests for the class PulseInfo.
  /// </summary>
  [TestFixture]
  public class PulseInfo_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (PulseInfo_UnitTest).FullName);

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestInstallationDirectoryFromEnvironmentVariable ()
    {
      System.Environment.SetEnvironmentVariable ("MainServerInstallDir",
                                                 "Test");
      Assert.That (Lemoine.Info.PulseInfo.MainServerInstallationDirectory, Is.EqualTo ("Test"));
      System.Environment.SetEnvironmentVariable ("MainServerInstallDir",
                                                 "");
    }
  }
}
