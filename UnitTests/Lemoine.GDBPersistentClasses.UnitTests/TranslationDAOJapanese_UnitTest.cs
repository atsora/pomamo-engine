// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the Japanese default values of <see cref="TranslationDAO"/>
  /// </summary>
  [TestFixture]
  public class TranslationDAOJapanese_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (TranslationDAOJapanese_UnitTest).FullName);

    string m_previousDSNName;

    /// <summary>
    /// In a UTF8 database, the Japanese default values are inserted and can be read back
    /// </summary>
    [Test]
    public void TestInsertJapaneseDefaultValues ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var translationDAO = new TranslationDAO ();
          var isUtf8 = translationDAO.IsServerEncodingUtf8 ();
          if (!isUtf8) {
            Assert.Ignore ("the unit test database is not encoded in UTF8");
          }

          translationDAO.InsertJapaneseDefaultValues ();
          ModelDAOHelper.DAOFactory.Flush ();

          var translation = translationDAO.Find ("ja", "MachineModeActive");
          Assert.That (translation, Is.Not.Null);
          Assert.That (translation.TranslationValue, Is.EqualTo ("稼働"));
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName", Lemoine.Info.Constants.DEFAULT_DSN_UNIT_TEST_NAME);
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName", m_previousDSNName);
      }
    }
  }
}
