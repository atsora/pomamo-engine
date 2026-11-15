// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading.Tasks;
using Lemoine.Extensions.Web.Responses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Web.Doc;

namespace Pulse.Web.UnitTests.Doc
{
  /// <summary>
  /// Unit tests of the Doc/WorkInfoDocCount service
  /// </summary>
  public class WorkInfoDocCountService_UnitTest
  {
    static async Task<IDoc> NewDocAsync ()
    {
      var doc = ModelDAOHelper.ModelFactory.CreateDoc ($"workinfodoccount/{Guid.NewGuid ():N}.pdf");
      return await ModelDAOHelper.DAOFactory.DocDAO.MakePersistentAsync (doc);
    }

    static async Task<IOperation> NewOperationAsync (string name, bool archived)
    {
      var operation = ModelDAOHelper.ModelFactory.CreateOperation (await ModelDAOHelper.DAOFactory.OperationTypeDAO.FindByIdAsync (1));
      operation.Name = name;
      if (archived) {
        operation.ArchiveDateTime = DateTime.UtcNow;
      }
      return await ModelDAOHelper.DAOFactory.OperationDAO.MakePersistentAsync (operation);
    }

    static async Task<object> GetAsync (int? operationId, int? componentId) =>
      await new WorkInfoDocCountService ().Get (new WorkInfoDocCountRequestDTO {
        OperationId = operationId,
        ComponentId = componentId,
      });

    static async Task<int> CountAsync (int? operationId, int? componentId)
    {
      var response = await GetAsync (operationId, componentId);
      Assert.That (response, Is.InstanceOf<WorkInfoDocCountResponseDTO> ());
      return ((WorkInfoDocCountResponseDTO)response).DocCount;
    }

    /// <summary>
    /// Distinct documents of the active revision of the operation and of the component
    /// </summary>
    [Test]
    public async Task TestCountAsync ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var daoFactory = ModelDAOHelper.DAOFactory;
          var shared = await NewDocAsync ();
          var componentOnly = await NewDocAsync ();
          var revision1Only = await NewDocAsync ();
          var revision2Only = await NewDocAsync ();

          var project = ModelDAOHelper.ModelFactory.CreateProjectFromName ($"project-{Guid.NewGuid ():N}");
          await daoFactory.ProjectDAO.MakePersistentAsync (project);
          var component = ModelDAOHelper.ModelFactory.CreateComponentFromName (project, "COMP",
            await daoFactory.ComponentTypeDAO.FindByIdAsync (1));
          await daoFactory.ComponentDAO.MakePersistentAsync (component);
          component.Docs.Add (shared);
          component.Docs.Add (componentOnly);

          // Revision 1 archived, revision 2 active
          var baseName = $"op-{Guid.NewGuid ():N}";
          var revision1 = await NewOperationAsync (baseName, true);
          revision1.ActiveRevision.Docs.Add (revision1Only);
          var revision2 = await NewOperationAsync ($"{baseName} rev2", false);
          revision2.ActiveRevision.Docs.Add (shared);
          revision2.ActiveRevision.Docs.Add (revision2Only);
          var noDoc = await NewOperationAsync ($"op-{Guid.NewGuid ():N}", false);
          daoFactory.Flush ();

          // The operation counts the documents of its active revision, whatever revision is given
          var fromRevision1 = await CountAsync (revision1.Id, null);
          var fromRevision2 = await CountAsync (revision2.Id, null);
          var fromComponent = await CountAsync (null, component.Id);
          var both = await CountAsync (revision1.Id, component.Id);
          var none = await CountAsync (noDoc.Id, null);
          var unknown = await CountAsync (int.MaxValue, int.MaxValue);
          var unknownOperation = await CountAsync (int.MaxValue, component.Id);
          Assert.Multiple (() => {
            Assert.That (fromRevision1, Is.EqualTo (2));
            Assert.That (fromRevision2, Is.EqualTo (2));
            Assert.That (fromComponent, Is.EqualTo (2));
            // shared is counted once
            Assert.That (both, Is.EqualTo (3));
            Assert.That (none, Is.EqualTo (0));
            // Unknown ids count 0
            Assert.That (unknown, Is.EqualTo (0));
            Assert.That (unknownOperation, Is.EqualTo (2));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Neither OperationId nor ComponentId: wrong request parameter
    /// </summary>
    [Test]
    public async Task TestNoParameterAsync ()
    {
      var response = await GetAsync (null, null);
      Assert.That (response, Is.InstanceOf<ErrorDTO> ());
      Assert.That (((ErrorDTO)response).Status, Is.EqualTo (ErrorStatus.WrongRequestParameter));
    }
  }
}
