// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Info.ConfigReader;
using NUnit.Framework;

namespace Lemoine.Info.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="InstanceInfo"/> and <see cref="DevInstanceConfigReader"/>
  /// </summary>
  public class InstanceInfo_UnitTest
  {
    /// <summary>
    /// Normal instance: no suffix
    /// </summary>
    [Test]
    public void TestNormalInstance ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, false);
        Assert.Multiple (() => {
          Assert.That (InstanceInfo.IsDev, Is.False);
          Assert.That (InstanceInfo.Suffix, Is.Empty);
          Assert.That (InstanceInfo.GetServiceName ("AtrackingAspService"), Is.EqualTo ("AtrackingAspService"));
        });
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// Dev instance: suffix -dev
    /// </summary>
    [Test]
    public void TestDevInstance ()
    {
      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, true);
        Assert.Multiple (() => {
          Assert.That (InstanceInfo.IsDev, Is.True);
          Assert.That (InstanceInfo.Suffix, Is.EqualTo ("-dev"));
          Assert.That (InstanceInfo.GetServiceName ("AtrackingAspService"), Is.EqualTo ("AtrackingAspService-dev"));
        });
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }

    /// <summary>
    /// The dev instance config reader only returns a value in a dev instance, and never the dev key itself
    /// </summary>
    [Test]
    public void TestDevInstanceConfigReader ()
    {
      var memoryConfigReader = new MemoryConfigReader ();
      memoryConfigReader.Add ("TestDevInstanceConfigReader.Key", "dev value");
      memoryConfigReader.Add (InstanceInfo.DEV_KEY, true);
      var configReader = new DevInstanceConfigReader (memoryConfigReader);

      try {
        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, false);
        Assert.Throws<ConfigKeyNotFoundException> (() => configReader.Get<string> ("TestDevInstanceConfigReader.Key"));

        Lemoine.Info.ConfigSet.ForceValue (InstanceInfo.DEV_KEY, true);
        Assert.Multiple (() => {
          Assert.That (configReader.Get<string> ("TestDevInstanceConfigReader.Key"), Is.EqualTo ("dev value"));
          Assert.Throws<ConfigKeyNotFoundException> (() => configReader.Get<bool> (InstanceInfo.DEV_KEY));
        });
      }
      finally {
        Lemoine.Info.ConfigSet.ResetForceValues ();
      }
    }
  }
}
