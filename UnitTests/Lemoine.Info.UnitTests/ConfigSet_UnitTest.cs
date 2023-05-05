// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using System.IO;
using Lemoine.Info.ConfigReader;

namespace Lemoine.Info.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class ConfigSet_UnitTest
  {
    readonly ILog log = LogManager.GetLogger (typeof (ConfigSet_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ConfigSet_UnitTest ()
    { }

    [OneTimeSetUp]
    public void SetUp ()
    {
      Lemoine.Info.ConfigSet.SetOsConfigReader (new Lemoine.Info.ConfigReader.TargetSpecific.OsConfigReader ());
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestPersistentConfigWriter ()
    {
      var key = "TestPersistentConfigWriter";
      Lemoine.Info.ConfigSet.SetPersistentConfig (key, true);
      Assert.IsTrue (Lemoine.Info.ConfigSet.Get<bool> (key));
      {
        var commonApplicationData = System.Environment.GetFolderPath (System.Environment.SpecialFolder.CommonApplicationData);
        var commonConfigDirectory = Path.Combine (commonApplicationData, "Lemoine", "PULSE");
        var path = Path.Combine (commonConfigDirectory, $"{PulseInfo.ProductFolderName}.options.d", key + ".options");
        Assert.IsTrue (File.Exists (path));
      }
      Lemoine.Info.ConfigSet.ResetPersistentConfig (key);
      Assert.Throws<ConfigKeyNotFoundException> (() => Lemoine.Info.ConfigSet.Get<bool> (key));
    }

    [Test]
    public void TestPersistentCacheConfigReader ()
    {
      var configReader = new TestExceptionConfigReader ();
      configReader.Add ("A", 1);
      configReader.Add ("B", 2);
      configReader.Add ("C", 3);
      var persistentCacheConfigReader = new PersistentCacheConfigReader (configReader, "UnitTest.persistentoptions");
      var a = persistentCacheConfigReader.Get<int> ("A");
      Assert.AreEqual (1, a);
      var path = Path.Combine (PulseInfo.LocalConfigurationDirectory, "UnitTest.persistentoptions");
      Assert.AreEqual (
"""
A:1

""".ReplaceLineEndings (), File.ReadAllText (path).ReplaceLineEndings ());
      configReader.Available = false;
      a = persistentCacheConfigReader.Get<int> ("A");
      Assert.AreEqual (1, a);
      Assert.Throws<ConfigKeyNotFoundException> (() => persistentCacheConfigReader.Get<int> ("B"));
      configReader.Available = true;
      var b = persistentCacheConfigReader.Get<int> ("B");
      Assert.AreEqual (2, b);
      Assert.AreEqual (
"""
A:1
B:2

""".ReplaceLineEndings (), File.ReadAllText (path).ReplaceLineEndings ());
      configReader.Add ("A", 11, true);
      a = persistentCacheConfigReader.Get<int> ("A");
      Assert.AreEqual (11, a);
      Assert.AreEqual (
"""
A:11

""".ReplaceLineEndings (), File.ReadAllText (path).ReplaceLineEndings ());
      File.Delete (path);
    }
  }
}
