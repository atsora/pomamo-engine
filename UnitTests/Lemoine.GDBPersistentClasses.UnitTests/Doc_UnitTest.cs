// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
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

    /// <summary>
    /// Test the tags are normalized, saved, read back and modified in place
    /// </summary>
    [Test]
    public async Task TestTagsAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var doc = ModelDAOHelper.ModelFactory.CreateDoc ("tags/doc.pdf");
          Assert.That (doc.Tags, Is.Empty);
          doc.Tags = new List<string> { " quality ", "", "safety", "quality" };
          Assert.That (doc.Tags, Is.EqualTo (new[] { "quality", "safety" }));
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (doc);
          ModelDAOHelper.DAOFactory.Flush ();
          NHibernateHelper.GetCurrentSession ().Clear ();

          var readBack = await ModelDAOHelper.DAOFactory.DocDAO.FindByIdAsync (doc.Id);
          Assert.That (readBack.Tags, Is.EqualTo (new[] { "quality", "safety" }));

          // A modification of the list in place makes the doc dirty
          readBack.Tags.Add ("lubrication");
          ModelDAOHelper.DAOFactory.Flush ();
          NHibernateHelper.GetCurrentSession ().Clear ();

          var modified = await ModelDAOHelper.DAOFactory.DocDAO.FindByIdAsync (doc.Id);
          Assert.That (modified.Tags, Is.EqualTo (new[] { "quality", "safety", "lubrication" }));

          // Null clears the tags
          modified.Tags = null;
          Assert.That (modified.Tags, Is.Empty);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test FindByTagAsync returns only the docs that carry the tag
    /// </summary>
    [Test]
    public async Task TestFindByTagAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var first = ModelDAOHelper.ModelFactory.CreateDoc ("tags/first.pdf");
          first.Tags = new List<string> { "unittest-quality", "unittest-safety" };
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (first);
          var second = ModelDAOHelper.ModelFactory.CreateDoc ("tags/second.pdf");
          second.Tags = new List<string> { "unittest-safety" };
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (second);
          var third = ModelDAOHelper.ModelFactory.CreateDoc ("tags/third.pdf");
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (third);
          ModelDAOHelper.DAOFactory.Flush ();

          var quality = await ModelDAOHelper.DAOFactory.DocDAO.FindByTagAsync ("unittest-quality");
          var safety = await ModelDAOHelper.DAOFactory.DocDAO.FindByTagAsync ("unittest-safety");
          var unknown = await ModelDAOHelper.DAOFactory.DocDAO.FindByTagAsync ("unittest-unknown");
          Assert.Multiple (() => {
            Assert.That (quality.Select (x => x.Id), Is.EqualTo (new[] { first.Id }));
            Assert.That (safety.Select (x => x.Id), Is.EqualTo (new[] { first.Id, second.Id }));
            Assert.That (unknown, Is.Empty);
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
