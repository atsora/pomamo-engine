// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using NUnit.Framework;

namespace Lemoine.I18N.UnitTests
{
  /// <summary>
  /// Unit tests for the class Lemoine.I18N.LocaleSettings.
  /// </summary>
  [TestFixture]
  public class LocaleSettings_UnitTest
  {
    /// <summary>
    /// Test the conversion of a language setting into a .NET culture name
    /// </summary>
    [TestCase ("en-US", "en-US")]
    [TestCase ("fr", "fr")]
    [TestCase ("en_US.UTF-8", "en-US")]
    [TestCase ("de_DE.UTF-8@euro", "de-DE")]
    [TestCase ("fr_FR@euro", "fr-FR")]
    [TestCase ("en_US:en", "en-US")]
    [TestCase (" fr_FR.UTF-8 : en ", "fr-FR")]
    [TestCase ("C.UTF-8", null)]
    [TestCase ("C", null)]
    [TestCase ("POSIX", null)]
    [TestCase ("", null)]
    [TestCase (null, null)]
    public void TestToCultureName (string language, string expected)
    {
      Assert.That (LocaleSettings.ToCultureName (language), Is.EqualTo (expected));
    }
  }
}
