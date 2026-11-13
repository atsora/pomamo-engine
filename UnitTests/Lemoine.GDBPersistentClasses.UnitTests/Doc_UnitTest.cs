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
    /// Test FindBySearchAsync combines the path, the tag and the kind
    /// </summary>
    [Test]
    public async Task TestFindBySearchAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var marker = Guid.NewGuid ().ToString ("N");
          var first = ModelDAOHelper.ModelFactory.CreateDoc ($"search/{marker}/Shaft-Drawing.pdf");
          first.Tags = new List<string> { $"tag-{marker}" };
          first.Kind = await ModelDAOHelper.DAOFactory.DocKindDAO.FindByIdAsync (3);
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (first);
          var second = ModelDAOHelper.ModelFactory.CreateDoc ($"search/{marker}/shaft.nc");
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (second);
          ModelDAOHelper.DAOFactory.Flush ();

          var byPath = await ModelDAOHelper.DAOFactory.DocDAO.FindBySearchAsync ($"{marker.ToUpperInvariant ()}/SHAFT", null, null, 10);
          var byTag = await ModelDAOHelper.DAOFactory.DocDAO.FindBySearchAsync (null, $"tag-{marker}", null, 10);
          var byKind = await ModelDAOHelper.DAOFactory.DocDAO.FindBySearchAsync (marker, null, 3, 10);
          var limited = await ModelDAOHelper.DAOFactory.DocDAO.FindBySearchAsync (marker, null, null, 1);
          Assert.Multiple (() => {
            Assert.That (byPath.Select (x => x.Id), Is.EqualTo (new[] { first.Id, second.Id }));
            Assert.That (byTag.Select (x => x.Id), Is.EqualTo (new[] { first.Id }));
            Assert.That (byKind.Select (x => x.Id), Is.EqualTo (new[] { first.Id }));
            Assert.That (limited, Has.Count.EqualTo (1));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test FindBySearchAsync also matches the name, the code and the external code of the
    /// components and of the operations the documents are associated to
    /// </summary>
    [Test]
    public async Task TestFindBySearchWorkInformationAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var daoFactory = ModelDAOHelper.DAOFactory;
          var marker = Guid.NewGuid ().ToString ("N");
          var componentDoc = ModelDAOHelper.ModelFactory.CreateDoc ($"wi/{Guid.NewGuid ():N}/drawing.pdf");
          await daoFactory.DocDAO.MakePersistentAsync (componentDoc);
          var operationDoc = ModelDAOHelper.ModelFactory.CreateDoc ($"wi/{Guid.NewGuid ():N}/program.nc");
          await daoFactory.DocDAO.MakePersistentAsync (operationDoc);
          var otherDoc = ModelDAOHelper.ModelFactory.CreateDoc ($"wi/{Guid.NewGuid ():N}/other.pdf");
          await daoFactory.DocDAO.MakePersistentAsync (otherDoc);

          var project = ModelDAOHelper.ModelFactory.CreateProjectFromName ($"project-{marker}");
          await daoFactory.ProjectDAO.MakePersistentAsync (project);
          var component = ModelDAOHelper.ModelFactory.CreateComponentFromName (project, $"Shaft-{marker}",
            await daoFactory.ComponentTypeDAO.FindByIdAsync (1));
          component.Code = $"cc-{marker}";
          component.ExternalCode = $"ce-{marker}";
          await daoFactory.ComponentDAO.MakePersistentAsync (component);
          component.Docs.Add (componentDoc);
          var operation = ModelDAOHelper.ModelFactory.CreateOperation (await daoFactory.OperationTypeDAO.FindByIdAsync (1));
          operation.Name = $"Op10-{marker}";
          operation.Code = $"oc-{marker}";
          operation.ExternalCode = $"oe-{marker}";
          await daoFactory.OperationDAO.MakePersistentAsync (operation);
          operation.ActiveRevision.Docs.Add (operationDoc);
          daoFactory.Flush ();

          var componentName = await daoFactory.DocDAO.FindBySearchAsync ($"SHAFT-{marker.ToUpperInvariant ()}", null, null, 10);
          var componentCode = await daoFactory.DocDAO.FindBySearchAsync ($"cc-{marker}", null, null, 10);
          var componentExternalCode = await daoFactory.DocDAO.FindBySearchAsync ($"ce-{marker}", null, null, 10);
          var operationName = await daoFactory.DocDAO.FindBySearchAsync ($"op10-{marker}", null, null, 10);
          var operationCode = await daoFactory.DocDAO.FindBySearchAsync ($"oc-{marker}", null, null, 10);
          var operationExternalCode = await daoFactory.DocDAO.FindBySearchAsync ($"oe-{marker}", null, null, 10);
          var both = await daoFactory.DocDAO.FindBySearchAsync (marker, null, null, 10);
          var withKind = await daoFactory.DocDAO.FindBySearchAsync ($"cc-{marker}", null, 3, 10);
          Assert.Multiple (() => {
            Assert.That (componentName.Select (x => x.Id), Is.EqualTo (new[] { componentDoc.Id }));
            Assert.That (componentCode.Select (x => x.Id), Is.EqualTo (new[] { componentDoc.Id }));
            Assert.That (componentExternalCode.Select (x => x.Id), Is.EqualTo (new[] { componentDoc.Id }));
            Assert.That (operationName.Select (x => x.Id), Is.EqualTo (new[] { operationDoc.Id }));
            Assert.That (operationCode.Select (x => x.Id), Is.EqualTo (new[] { operationDoc.Id }));
            Assert.That (operationExternalCode.Select (x => x.Id), Is.EqualTo (new[] { operationDoc.Id }));
            Assert.That (both.Select (x => x.Id), Is.EquivalentTo (new[] { componentDoc.Id, operationDoc.Id }));
            Assert.That (withKind, Is.Empty);
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test FindAllTagsAsync returns the distinct tags, ordered
    /// </summary>
    [Test]
    public async Task TestFindAllTagsAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var marker = Guid.NewGuid ().ToString ("N");
          var first = ModelDAOHelper.ModelFactory.CreateDoc ($"tags/{marker}/first.pdf");
          first.Tags = new List<string> { $"{marker}-b", $"{marker}-a" };
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (first);
          var second = ModelDAOHelper.ModelFactory.CreateDoc ($"tags/{marker}/second.pdf");
          second.Tags = new List<string> { $"{marker}-a", $"{marker}-c" };
          await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (second);
          ModelDAOHelper.DAOFactory.Flush ();

          var tags = await ModelDAOHelper.DAOFactory.DocDAO.FindAllTagsAsync ();
          Assert.Multiple (() => {
            Assert.That (tags.Where (x => x.StartsWith (marker)), Is.EqualTo (new[] { $"{marker}-a", $"{marker}-b", $"{marker}-c" }));
            Assert.That (tags, Is.Unique);
          });
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
