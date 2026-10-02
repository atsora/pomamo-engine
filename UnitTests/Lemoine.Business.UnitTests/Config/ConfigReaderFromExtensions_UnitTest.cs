// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Business.Config;
using Lemoine.Extensions.Business.Config;
using Lemoine.Info;
using Lemoine.Info.ConfigReader;
using NUnit.Framework;

namespace Lemoine.Business.UnitTests.Config
{
  /// <summary>
  /// Unit tests for the class ConfigReaderFromExtensions
  /// </summary>
  [TestFixture]
  public class ConfigReaderFromExtensions_UnitTest
  {
    const string GET_KEY = "ConfigReaderFromExtensions.UnitTest.Get";
    const string INITIALIZE_KEY = "ConfigReaderFromExtensions.UnitTest.Initialize";
    const string INNER_KEY = "ConfigReaderFromExtensions.UnitTest.Inner";
    const string INNER_DEFAULT = "inner-default";

    /// <summary>
    /// Config extension that reads a config value each time Get is called,
    /// like an extension whose code requires an assembly that is resolved by PluginLoadContext,
    /// which reads its own config values
    /// </summary>
    public class ReentrantGetConfigExtension
      : Lemoine.Extensions.NotConfigurableExtension
      , IConfigExtension
    {
      public double Priority => 100.0;

      public bool Initialize () => true;

      public T Get<T> (string key)
      {
        var inner = ConfigSet.LoadAndGet<string> (INNER_KEY, INNER_DEFAULT);
        if (key.Equals (GET_KEY)) {
          return (T)(object)$"value({inner})";
        }
        throw new ConfigKeyNotFoundException (key);
      }
    }

    /// <summary>
    /// Config extension that reads a config value in its initialization
    /// </summary>
    public class ReentrantInitializeConfigExtension
      : Lemoine.Extensions.NotConfigurableExtension
      , IConfigExtension
    {
      string m_inner = null;

      public double Priority => 100.0;

      public bool Initialize ()
      {
        m_inner = ConfigSet.LoadAndGet<string> (INNER_KEY, INNER_DEFAULT);
        return true;
      }

      public T Get<T> (string key)
      {
        if (key.Equals (INITIALIZE_KEY)) {
          return (T)(object)$"value({m_inner})";
        }
        throw new ConfigKeyNotFoundException (key);
      }
    }

    /// <summary>
    /// Config extension that raises an exception in its initialization
    /// </summary>
    public class ThrowingInitializeConfigExtension
      : Lemoine.Extensions.NotConfigurableExtension
      , IConfigExtension
    {
      public double Priority => 100.0;

      public bool Initialize ()
      {
        throw new InvalidOperationException ("Test exception in Initialize");
      }

      public T Get<T> (string key)
      {
        throw new ConfigKeyNotFoundException (key);
      }
    }

    /// <summary>
    /// Config reader that can be deactivated at the end of a test,
    /// since a config reader can't be removed from ConfigSet
    /// </summary>
    class SwitchableConfigReader : IGenericConfigReader
    {
      readonly IGenericConfigReader m_configReader;

      public bool Active { get; set; } = true;

      public SwitchableConfigReader (IGenericConfigReader configReader)
      {
        m_configReader = configReader;
      }

      public T Get<T> (string key)
      {
        if (!this.Active) {
          throw new ConfigKeyNotFoundException (key);
        }
        return m_configReader.Get<T> (key);
      }
    }

    /// <summary>
    /// Test a config extension that requests a config value in Get:
    /// the re-entrant call must not end in a stack overflow
    /// </summary>
    [Test]
    public void TestReentrantGet ()
    {
      RunWithExtension<ReentrantGetConfigExtension> (initialize: true, () => {
        var v = ConfigSet.LoadAndGet<string> (GET_KEY, "not-found");
        Assert.That (v, Is.EqualTo ($"value({INNER_DEFAULT})"));
      });
    }

    /// <summary>
    /// Test a config extension that requests a config value in its initialization,
    /// when the config reader is initialized only at its first use, after it was added to ConfigSet:
    /// the re-entrant call must not end in a stack overflow
    /// </summary>
    [Test]
    public void TestReentrantInitialize ()
    {
      RunWithExtension<ReentrantInitializeConfigExtension> (initialize: false, () => {
        var v = ConfigSet.LoadAndGet<string> (INITIALIZE_KEY, "not-found");
        Assert.That (v, Is.EqualTo ($"value({INNER_DEFAULT})"));
      });
    }

    /// <summary>
    /// Test an extension that raises an exception in its initialization:
    /// it must be skipped and the other extensions must remain active
    /// </summary>
    /// <param name="initialize">initialize the config reader before it is added to ConfigSet</param>
    [TestCase (true)]
    [TestCase (false)]
    public void TestInitializeException (bool initialize)
    {
      RunWithExtensions (initialize, () => {
        var v = ConfigSet.LoadAndGet<string> (GET_KEY, "not-found");
        Assert.That (v, Is.EqualTo ($"value({INNER_DEFAULT})"));
      }, typeof (ThrowingInitializeConfigExtension), typeof (ReentrantGetConfigExtension));
    }

    void RunWithExtension<TExtension> (bool initialize, Action test)
    {
      RunWithExtensions (initialize, test, typeof (TExtension));
    }

    void RunWithExtensions (bool initialize, Action test, params Type[] extensionTypes)
    {
      var previousService = ServiceProvider.Service;
      SwitchableConfigReader configReader = null;
      try {
        // New cache, not to get the extensions of another test
        ServiceProvider.Service = new CachedService (new Core.Cache.LruCacheClient (100));
        foreach (var extensionType in extensionTypes) {
          Lemoine.Extensions.ExtensionManager.Add (extensionType);
        }
        Lemoine.Extensions.ExtensionManager.Activate (false);
        Lemoine.Extensions.ExtensionManager.Load ();

        var configReaderFromExtensions = new ConfigReaderFromExtensions (false);
        if (initialize) {
          configReaderFromExtensions.Initialize ();
        }
        configReader = new SwitchableConfigReader (configReaderFromExtensions);
        ConfigSet.AddConfigReader (configReader);

        test ();
      }
      finally {
        if (configReader is not null) {
          configReader.Active = false;
        }
        ConfigSet.ResetCache ();
        Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        ServiceProvider.Service = previousService;
      }
    }
  }
}
