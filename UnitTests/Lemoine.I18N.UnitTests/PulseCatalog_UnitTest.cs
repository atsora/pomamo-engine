// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;
using System.Threading;

using Lemoine.Core.Log;
using NUnit.Framework;

using Lemoine.I18N;
using System.Text;

namespace Lemoine.I18N.UnitTests
{
  /// <summary>
  /// Unit tests for the class Lemoine.I18N.PulseCatalog.
  /// </summary>
  [TestFixture]
  public class PulseCatalog_UnitTest
  {
    private static readonly ILog log = LogManager.GetLogger(typeof (PulseCatalog_UnitTest).FullName);

    /// <summary>
    /// Test the translated terms are ok after updating
    /// Thread.CurrentThread.CurrentUICulture
    /// </summary>
    [Test]
    public void TestGetStringAfterSettingUICulture ()
    {
      var baseCatalog = new TextFileCatalog ("pulse",
                                             TestContext.CurrentContext.TestDirectory);
      baseCatalog.LocalePrefix = "_";
      Lemoine.I18N.PulseCatalog.Implementation = baseCatalog;
      
      CultureInfo previousCultureInfo = Thread.CurrentThread.CurrentUICulture;
      string value;
      
      // Take the default value,
      // because no translation file is available for locale zu-ZA
      LocaleSettings.SetLanguage ("zu-ZA");
      //      PulseCatalog.CultureInfo = LocaleSettings.CurrentCulture;

      value = PulseCatalog.GetString ("Start");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Start is {value}");
      }
      Assert.That (value, Is.EqualTo ("Start"));
      
      value = PulseCatalog.GetString ("UnknownForTest");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownForTest is {value}");
      }
      Assert.That (value, Is.EqualTo ("UnknownForTest"));

      // Use some data written for the test
      LocaleSettings.SetLanguage ("xh-ZA");

      value = PulseCatalog.GetString ("MaxN");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      value = PulseCatalog.GetString ("UnknownTranslation");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownTranslation is {value}");
      }
      Assert.That (value, Is.EqualTo ("UnknownTranslation"));

      value = PulseCatalog.GetString ("MaxN");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      // Reset the used culture
      LocaleSettings.SetLanguage (previousCultureInfo);
    }
    
    /// <summary>
    /// Test the translated terms are ok
    /// after updating the property CultureInfo
    /// of the singleton class PulseCatalog.
    /// </summary>
    [Test]
    public void TestGetStringWithCulture ()
    {
      var baseCatalog = new TextFileCatalog ("pulse",
                                             TestContext.CurrentContext.TestDirectory);
      baseCatalog.LocalePrefix = "_";
      Lemoine.I18N.PulseCatalog.Implementation = baseCatalog;
      
      string value;

      // Take the default value,
      // because no translation file is available for locale zu-ZA
      PulseCatalog.CultureInfo = new CultureInfo ("zu-ZA");

      value = PulseCatalog.GetString ("Start");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Start is {value}");
      }
      Assert.That (value, Is.EqualTo ("Start"));
      
      value = PulseCatalog.GetString ("UnknownForTest");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownForTest is {value}");
      }
      Assert.That (value, Is.EqualTo ("UnknownForTest"));
      
      // Use some data written for the test
      PulseCatalog.CultureInfo = new CultureInfo ("xh-ZA");

      value = PulseCatalog.GetString ("MaxN");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      value = PulseCatalog.GetString ("UnknownTranslation");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownTranslation is {value}");
      }
      Assert.That (value, Is.EqualTo ("UnknownTranslation"));

      value = PulseCatalog.GetString ("MaxN");
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));
    }

    /// <summary>
    /// Test the CachedCatalog
    /// </summary>
    [Test]
    public void TestCachedCatalog ()
    {
      var baseCatalog = new TextFileCatalog ("pulse",
                                             TestContext.CurrentContext.TestDirectory);
      baseCatalog.LocalePrefix = "_";
      var catalog = new CachedCatalog (baseCatalog);
      string value;

      var cultureInfo = new CultureInfo ("xh-ZA");

      value = catalog.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      value = catalog.GetString ("UnknownTranslation", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownTranslation is {value}");
      }
      Assert.That (value, Is.Null);

      value = catalog.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));
    }

    /// <summary>
    /// Test the StorageCatalog
    /// </summary>
    [Test]
    public void TestStorageCatalog ()
    {
      var baseCatalog = new TextFileCatalog ("pulse",
                                             TestContext.CurrentContext.TestDirectory);
      baseCatalog.LocalePrefix = "_";
      var storage = new CachedCatalog (new DummyCatalog ());
      var catalog = new StorageCatalog (baseCatalog,
                                        storage);
      string value;

      var cultureInfo = new CultureInfo ("xh-ZA");

      value = catalog.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      value = catalog.GetString ("UnknownTranslation", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownTranslation is {value}");
      }
      Assert.That (value, Is.Null);

      value = catalog.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      value = storage.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      value = storage.GetString ("UnknownTranslation", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownTranslation is {value}");
      }
      Assert.That (value, Is.Null);

      value = storage.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));
    }

    /// <summary>
    /// Test the TextFileCatalog
    /// </summary>
    [Test]
    public void TestTextFileCatalog ()
    {
      var catalog = new TextFileCatalog ("pulse",
                                         TestContext.CurrentContext.TestDirectory);
      catalog.LocalePrefix = "_";
      string value;

      var cultureInfo = new CultureInfo ("xh-ZA");
      
      value = catalog.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));

      value = catalog.GetString ("UnknownTranslation", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of UnknownTranslation is {value}");
      }
      Assert.That (value, Is.Null);

      value = catalog.GetString ("MaxN", cultureInfo);
      if (log.IsDebugEnabled) {
        log.Debug ($"Translation of Max N is {value}");
      }
      Assert.That (value, Is.EqualTo ("Max N"));
    }
  }
}
