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
      Assert.That (Lemoine.Info.ConfigSet.Get<bool> (key), Is.True);
      {
        var commonApplicationData = System.Environment.GetFolderPath (System.Environment.SpecialFolder.CommonApplicationData);
        var commonConfigDirectory = Path.Combine (commonApplicationData, "Pomamo");
        var path = Path.Combine (commonConfigDirectory, $"{PulseInfo.ProductFolderName}.options.d", key + ".options");
        Assert.That (File.Exists (path), Is.True);
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
      Assert.That (a, Is.EqualTo (1));
      var path = Path.Combine (PulseInfo.LocalConfigurationDirectory, "UnitTest.persistentoptions");
      Assert.That (
File.ReadAllText (path).ReplaceLineEndings (), Is.EqualTo ("""
A:1

""".ReplaceLineEndings ()));
      configReader.Available = false;
      a = persistentCacheConfigReader.Get<int> ("A");
      Assert.That (a, Is.EqualTo (1));
      Assert.Throws<ConfigKeyNotFoundException> (() => persistentCacheConfigReader.Get<int> ("B"));
      configReader.Available = true;
      var b = persistentCacheConfigReader.Get<int> ("B");
      Assert.Multiple (() => {
        Assert.That (b, Is.EqualTo (2));
        Assert.That (
  File.ReadAllText (path).ReplaceLineEndings (), Is.EqualTo ("""
A:1
B:2

""".ReplaceLineEndings ()));
      });
      configReader.Add ("A", 11, true);
      a = persistentCacheConfigReader.Get<int> ("A");
      Assert.Multiple (() => {
        Assert.That (a, Is.EqualTo (11));
        Assert.That (
  File.ReadAllText (path).ReplaceLineEndings (), Is.EqualTo ("""
A:11

""".ReplaceLineEndings ()));
      });
      var o = persistentCacheConfigReader.Get<object> ("A");
      var s = o?.ToString ();
      Assert.Multiple (() => {
        Assert.That (o, Is.EqualTo (11));
        Assert.That (o, Is.TypeOf<int> ());
        Assert.That (s, Is.EqualTo ("11"));
      });
      File.Delete (path);
    }
  }
}
