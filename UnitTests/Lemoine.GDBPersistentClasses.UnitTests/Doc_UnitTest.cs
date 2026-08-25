// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="Doc"/> and <see cref="DocDAO"/>
  /// </summary>
  public class Doc_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (Doc_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public Doc_UnitTest ()
      : base (new DateTime (2026, 01, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test the creation of a doc and the different ways to read it back
    /// </summary>
    [Test]
    public void TestMakePersistentAndFind ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var path = "quality/instructions.pdf";
          var doc = ModelDAOHelper.ModelFactory.CreateDoc (path);
          ModelDAOHelper.DAOFactory.DocDAO.MakePersistent (doc);
          ModelDAOHelper.DAOFactory.Flush ();

          var byId = ModelDAOHelper.DAOFactory.DocDAO.FindById (doc.Id);
          var byPath = ModelDAOHelper.DAOFactory.DocDAO.FindByPath (path);
          Assert.Multiple (() => {
            Assert.That (byId, Is.Not.Null);
            Assert.That (byId.Path, Is.EqualTo (path));
            Assert.That (byPath, Is.Not.Null);
            Assert.That (byPath.Id, Is.EqualTo (doc.Id));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test FindByPath returns null when no doc matches the path
    /// </summary>
    [Test]
    public void TestFindByPathUnknown ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var doc = ModelDAOHelper.DAOFactory.DocDAO.FindByPath ("unknown/path.pdf");
          Assert.That (doc, Is.Null);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test FindOrCreateByPath creates the doc only once
    /// </summary>
    [Test]
    public void TestFindOrCreateByPath ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var path = "maintenance/procedure.md";
          var created = ModelDAOHelper.DAOFactory.DocDAO.FindOrCreateByPath (path);
          ModelDAOHelper.DAOFactory.Flush ();
          var again = ModelDAOHelper.DAOFactory.DocDAO.FindOrCreateByPath (path);
          Assert.Multiple (() => {
            Assert.That (created.Id, Is.Not.EqualTo (0));
            Assert.That (again.Id, Is.EqualTo (created.Id));
            Assert.That (again.Path, Is.EqualTo (path));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
