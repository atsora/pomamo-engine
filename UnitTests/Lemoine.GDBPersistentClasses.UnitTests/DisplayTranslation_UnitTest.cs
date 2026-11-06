// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Globalization;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DisplayTranslation
  /// </summary>
  [TestFixture]
  public class DisplayTranslation_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (DisplayTranslation_UnitTest).FullName);

    class TestUnit
    {
      public string NameOrTranslation { get; set; }
    }

    class TestField
    {
      public string NameOrTranslation { get; set; }
      public string Code { get; set; }
      public TestUnit Unit { get; set; }
    }

    static string GetTestPattern (string table, string variant)
    {
      switch ($"{table}/{variant}") {
      case "Unit/":
        return "<%NameOrTranslation%>";
      case "Unit/Brackets":
        return "(<%Display%>)";
      case "Field/":
        return "<%NameOrTranslation%> <%Unit.Display_Brackets%>";
      case "Field/Recursive":
        return "<%Display_Recursive%>";
      default:
        return null;
      }
    }

    /// <summary>
    /// Pattern with a translated name and the display variant of a foreign entity
    /// </summary>
    [Test]
    public void TestApplyForeignVariant ()
    {
      var field = new TestField { NameOrTranslation = " Feedrate ", Unit = new TestUnit { NameOrTranslation = "mm/min" } };
      var result = DisplayTranslation.Apply ("<%NameOrTranslation%> <%Unit.Display_Brackets%>", field, "Field", GetTestPattern);
      Assert.That (result, Is.EqualTo ("Feedrate (mm/min)"));
    }

    /// <summary>
    /// A foreign entity that is not set is an empty string, like nulltoempty in SQL
    /// </summary>
    [Test]
    public void TestApplyNullForeign ()
    {
      var field = new TestField { NameOrTranslation = "Feedrate", Code = null };
      Assert.Multiple (() => {
        Assert.That (DisplayTranslation.Apply ("<%NameOrTranslation%> <%Unit.Display_Brackets%>", field, "Field", GetTestPattern), Is.EqualTo ("Feedrate "));
        Assert.That (DisplayTranslation.Apply ("<%Code%>: <%NameOrTranslation%>", field, "Field", GetTestPattern), Is.EqualTo (": Feedrate"));
      });
    }

    /// <summary>
    /// A pattern that can't be applied returns null, so that the display of the database is kept
    /// </summary>
    [Test]
    public void TestApplyNotSupported ()
    {
      var field = new TestField { NameOrTranslation = "Feedrate" };
      Assert.Multiple (() => {
        Assert.That (DisplayTranslation.Apply ("<%Unknown%>", field, "Field", GetTestPattern), Is.Null, "unknown property");
        Assert.That (DisplayTranslation.Apply ("<%Display_NoVariant%>", field, "Field", GetTestPattern), Is.Null, "unknown variant");
        Assert.That (DisplayTranslation.Apply ("<%Display_Recursive%>", field, "Field", GetTestPattern), Is.Null, "recursive pattern");
      });
    }

    /// <summary>
    /// Comparison of a culture name with a locale of the database
    /// </summary>
    [Test]
    public void TestIsSameLocale ()
    {
      Assert.Multiple (() => {
        Assert.That (DisplayTranslation.IsSameLocale ("fr-FR", "fr-FR"), Is.True);
        Assert.That (DisplayTranslation.IsSameLocale ("fr-FR", "fr_FR"), Is.True);
        Assert.That (DisplayTranslation.IsSameLocale ("fr-FR", "FR-fr"), Is.True);
        Assert.That (DisplayTranslation.IsSameLocale ("fr-FR", "fr"), Is.False);
        Assert.That (DisplayTranslation.IsSameLocale ("en-US", "fr-FR"), Is.False);
      });
    }

    /// <summary>
    /// The display of a reason follows the UI culture when it differs from the locale of the database,
    /// and is the display of the database otherwise
    /// </summary>
    [Test]
    public void TestReasonDisplay ()
    {
      var previousCulture = CultureInfo.CurrentUICulture;
      var previousCatalog = PulseCatalog.Implementation;
      try {
        var multiCatalog = new MultiCatalog ();
        multiCatalog.Add (new Lemoine.ModelDAO.TranslationDAOCatalog ());
        multiCatalog.Add (new DefaultTextFileCatalog ());
        PulseCatalog.Implementation = new CachedCatalog (multiCatalog);

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var databaseLocale = ModelDAOHelper.DAOFactory.ConfigDAO.GetConfig ("i18n.locale.default")?.Value as string;
          Assume.That (databaseLocale, Is.Not.Null.And.Not.Empty, "no i18n.locale.default in the database");
          var pattern = ModelDAOHelper.DAOFactory.DisplayDAO.FindWithTable ("Reason")?.Pattern;
          Assume.That (pattern, Is.EqualTo ("<%NameOrTranslation%>"), "not the default display pattern of the reasons");

          var reason = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (2);
          Assume.That (reason?.TranslationKey, Is.Not.Null, "no reason with a translation key");

          var otherCulture = databaseLocale.StartsWith ("en", StringComparison.OrdinalIgnoreCase) ? "de-DE" : "en-US";
          CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo (otherCulture);
          Assert.That (reason.Display, Is.EqualTo (reason.NameOrTranslation), $"display in {otherCulture}");

          CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo (databaseLocale.Replace ('_', '-'));
          var sqlDisplay = typeof (DataWithDisplayFunction)
            .GetField ("m_display", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue (reason); // FindById returns the entity itself, not a proxy
          Assert.That (reason.Display, Is.EqualTo (sqlDisplay), "display in the locale of the database");
        }
      }
      finally {
        CultureInfo.CurrentUICulture = previousCulture;
        PulseCatalog.Implementation = previousCatalog;
      }
    }

    /// <summary>
    /// The display of the tables whose display in the database is the translated name
    /// (role, dockind) is their name or translation in the UI culture, when it differs from the locale of the database
    /// </summary>
    [Test]
    public void TestNameOrTranslationDisplay ()
    {
      var previousCulture = CultureInfo.CurrentUICulture;
      var previousCatalog = PulseCatalog.Implementation;
      try {
        var multiCatalog = new MultiCatalog ();
        multiCatalog.Add (new Lemoine.ModelDAO.TranslationDAOCatalog ());
        multiCatalog.Add (new DefaultTextFileCatalog ());
        PulseCatalog.Implementation = new CachedCatalog (multiCatalog);

        using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
          var databaseLocale = ModelDAOHelper.DAOFactory.ConfigDAO.GetConfig ("i18n.locale.default")?.Value as string;
          Assume.That (databaseLocale, Is.Not.Null.And.Not.Empty, "no i18n.locale.default in the database");
          var otherCulture = databaseLocale.StartsWith ("en", StringComparison.OrdinalIgnoreCase) ? "de-DE" : "en-US";

          var role = ModelDAOHelper.DAOFactory.RoleDAO.FindAll ()
            .FirstOrDefault (r => r.TranslationKey is not null);
          Assume.That (role, Is.Not.Null, "no role with a translation key");

          System.Collections.Generic.IList<IDocKind> docKinds;
          try {
            docKinds = ModelDAOHelper.DAOFactory.DocKindDAO.FindAll ();
          }
          catch (Exception ex) {
            log.Warn ("TestNameOrTranslationDisplay: no dockind table in the unit test database", ex);
            docKinds = new System.Collections.Generic.List<IDocKind> ();
          }

          CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo (otherCulture);
          Assert.Multiple (() => {
            Assert.That (role.Display, Is.EqualTo (role.NameOrTranslation), $"role in {otherCulture}");
            foreach (var docKind in docKinds) {
              Assert.That (docKind.Display, Is.EqualTo (docKind.NameOrTranslation), $"{docKind} in {otherCulture}");
            }
          });
        }
      }
      finally {
        CultureInfo.CurrentUICulture = previousCulture;
        PulseCatalog.Implementation = previousCatalog;
      }
    }
  }
}
