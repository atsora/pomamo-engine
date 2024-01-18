// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.XPath;

using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Database.Persistent;

namespace Lemoine.DataRepository.UnitTests
{
  /// <summary>
  /// Unit tests for the class LemoineGDBBuilder
  /// </summary>
  [TestFixture]
  public class LemoineGDBBuilder_UnitTest
  {
    private string previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (LemoineGDBBuilder_UnitTest).FullName);

    /// <summary>
    /// Test a successful Build method
    /// </summary>
    [Test]
    public void TestValidBuild ()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        session.CreateQuery ("delete from Revision")
          .ExecuteUpdate ();
        session.CreateQuery ("delete from MachineModification")
          .ExecuteUpdate ();
        session.CreateQuery ("delete from GlobalModification")
          .ExecuteUpdate ();
        transaction.Commit ();
      }

      using (var daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = daoSession.BeginTransaction ()) {
        var session = NHibernateHelper.GetSession (daoSession);
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <UnknownType pulse:action='none' />
  <Revision pulse:action='create' pulse:newattributes='Id'>
    <Updater xsi:type=""Service"" Name=""Lemoine Machine Association Synchronization""
             pulse:action='reference' pulse:notfound='log fail' />
    <Machine Name=""MACHINE_A17""
             pulse:action='reference'
             pulse:notfound='fail'
             pulse:relation='none'
             pulse:newattributes='Id' />
    <PartView Name=""PART1""
              pulse:action='reference'
              pulse:notfound='fail'
              pulse:relation='none'
              pulse:newattributes='ComponentId' />
    <JobView Name=""JOB3""
             pulse:action='reference'
             pulse:notfound='fail'
             pulse:relation='none' />
    <SimpleOperationView Name=""SIMPLEOPERATION1""
                         pulse:action='reference'
                         pulse:notfound='fail'
                         pulse:relation='none'
                         pulse:newattributes='IntermediateWorkPieceId' />
    <WorkOrderProject pulse:action='reference'
                      pulse:notfound='fail'
                      pulse:relation='none'>
      <WorkOrder Name=""WO1""
                 pulse:action='reference'
                 pulse:notfound='fail' />
      <Project Name=""PART1""
               pulse:action='reference'
               pulse:notfound='fail' />
    </WorkOrderProject>
    <ComponentMachineAssociation Begin=""2011-07-01 08:00:00.010""
                                 other=""to ignore""
                                 pulse:action='create'
                                 pulse:relation='inverse'
                                 pulse:newattributes='Id'>
      <Component Id='{../../PartView/@ComponentId}'
                 other=""to ignore""
                 pulse:action='id'
                 pulse:notfound='log fail' />
      <Machine Name=""MACHINE_A17""
               pulse:action='reference' pulse:notfound='log fail' />
    </ComponentMachineAssociation>
    <OperationMachineAssociation Begin=""2011-07-01 08:00:00""
                                 other=""to ignore""
                                 pulse:action='create'
                                 pulse:relation='inverse'>
      <Operation Name=""SFKPROCESS3"" other=""to ignore""
                 pulse:action='reference' pulse:notfound='log fail' />
      <Machine Id='{../../Machine/@Id}'
               pulse:action='id' pulse:notfound='log fail' />
    </OperationMachineAssociation>
  </Revision>
  <Revision pulse:action='create'>
    <Updater xsi:type=""Service"" Name=""Lemoine Machine Association Synchronization""
             pulse:action='reference' pulse:notfound='log fail' />
  </Revision>
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        IList<Revision> revisions = session.CreateCriteria<Revision> ()
          .List<Revision> ();
        Assert.That (revisions, Has.Count.EqualTo (2));
        IUpdater updater = revisions[0].Updater;
        var service = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ServiceDAO
          .FindById (updater.Id);
        Assert.That (service, Is.Not.Null);
        Assert.That (service.Name, Is.EqualTo ("Lemoine Machine Association Synchronization"));
        // Check newattributes='Id'
        XPathNavigator node = pathNavigator.SelectSingleNode ("//Revision/@Id");
        Assert.That (node, Is.Not.Null);
        Assert.That (node.Value, Is.EqualTo (revisions[0].Id.ToString ()));
        IList<Modification> modifications = session.CreateCriteria<Modification> ()
          .List<Modification> ();
        Assert.That (modifications, Has.Count.EqualTo (2));
        IList<ComponentMachineAssociation> componentMachineAssociations =
          session.CreateCriteria<ComponentMachineAssociation> ()
          .List<ComponentMachineAssociation> ();
        Assert.That (componentMachineAssociations, Has.Count.EqualTo (1));
        Assert.That (componentMachineAssociations[0].Component.Name, Is.EqualTo ("PART1"));
        // Check newattributes='Id'
        XPathNavigator partIdNode = pathNavigator.SelectSingleNode ("//Revision/PartView/@ComponentId");
        Assert.That (partIdNode, Is.Not.Null);
        Assert.Multiple (() => {
          Assert.That (partIdNode.Value, Is.EqualTo ("18514"));
          Assert.That (((Lemoine.Collections.IDataWithId)componentMachineAssociations[0].Component).Id, Is.EqualTo (18514));
        });
        IList<OperationMachineAssociation> operationMachineAssociations =
          session.CreateCriteria<OperationMachineAssociation> ()
          .List<OperationMachineAssociation> ();
        Assert.Multiple (() => {
          Assert.That (operationMachineAssociations, Has.Count.EqualTo (1));
          Assert.That (((Lemoine.Collections.IDataWithId)operationMachineAssociations[0].Operation).Id, Is.EqualTo (11005));
        });
        // Clean the database
        session.CreateQuery ("delete from Revision")
          .ExecuteUpdate ();
        session.CreateQuery ("delete from MachineModification")
          .ExecuteUpdate ();
        session.CreateQuery ("delete from GlobalModification")
          .ExecuteUpdate ();
        session.Flush ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test a notfound / create reference process
    /// </summary>
    [Test]
    public void TestNotFoundCreateBuild ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <Revision pulse:action='create'>
    <Updater xsi:type=""Service"" Name=""Lemoine Machine Association Synchronization""
             pulse:action='reference' pulse:notfound='log fail' />
    <PartView Name=""TestNewPart""
              pulse:action='reference'
              pulse:notfound='log create'
              pulse:relation='none'>
      <Type TranslationKey=""UndefinedValue"" pulse:action='reference' pulse:notfound='fail' />
    </PartView>
    <JobView Name=""TestNewJob""
             pulse:action='reference'
             pulse:notfound='log create'
             pulse:relation='none'>
       <Status TranslationKey=""UndefinedValue"" pulse:action='reference' pulse:notfound='fail' />
    </JobView>
    <SimpleOperationView Name=""TestNewSimpleOperation""
                         pulse:action='reference'
                         pulse:notfound='log create'
                         pulse:relation='none'>
      <Type TranslationKey=""UndefinedValue"" pulse:action='reference' pulse:notfound='fail' />
    </SimpleOperationView>
    <WorkOrderProject pulse:action='reference'
                      pulse:notfound='log create'
                      pulse:relation='none'>
      <WorkOrder Name=""TestNewWorkOrder""
                 pulse:action='reference'
                 pulse:notfound='log create'>
        <Status Id=""1"" pulse:action='id' pulse:notfound='fail' />
      </WorkOrder>
      <Project Name=""TestNewPart""
               pulse:action='reference'
               pulse:notfound='fail' />
    </WorkOrderProject>
    <ComponentMachineAssociation Begin=""2011-07-01 08:00:00""
                                 other=""to ignore""
                                 pulse:action='create'
                                 pulse:relation='inverse'>
      <Component Name=""NEWCOMPONENT"" other=""to ignore""
                 pulse:action='reference' pulse:notfound='log create'>
         <Project Name=""NEWPROJECT""
                  pulse:action='reference' pulse:notfound='log create' />
         <Type TranslationKey=""UndefinedValue"" pulse:action='reference' pulse:notfound='fail' />
      </Component>
      <Machine Name=""MACHINE_A17""
                 pulse:action='reference' pulse:notfound='log fail' />
    </ComponentMachineAssociation>
  </Revision>
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

      // Test NEWCOMPONENT, NEWPROJECT, synchronization log and remove them
      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        // 1st part
        IPart part = session.CreateCriteria<Component> ()
          .Add (Expression.Eq ("Name", "TestNewPart"))
          .UniqueResult<Component> ().Part;
        Assert.That (part, Is.Not.Null);
        IJob job = session.CreateCriteria<Project> ()
          .Add (Expression.Eq ("Name", "TestNewJob"))
          .UniqueResult<Project> ().Job;
        Assert.That (job, Is.Not.Null);
        ISimpleOperation simpleOperation = session.CreateCriteria<Operation> ()
          .Add (Expression.Eq ("Name", "TestNewSimpleOperation"))
          .UniqueResult<Operation> ().SimpleOperation;
        Assert.That (simpleOperation, Is.Not.Null);
        IWorkOrder workOrder = session.CreateCriteria<WorkOrder> ()
          .Add (Expression.Eq ("Name", "TestNewWorkOrder"))
          .UniqueResult<WorkOrder> ();
        Assert.That (workOrder, Is.Not.Null);
        IWorkOrderProject workOrderProject =
          session.CreateQuery ("from WorkOrderProject foo " +
                               "where foo.WorkOrder.Name='TestNewWorkOrder' " +
                               "and foo.Project.Name='TestNewPart'")
          .UniqueResult<WorkOrderProject> ();
        Assert.That (workOrderProject, Is.Not.Null);
        Assert.That (workOrderProject.WorkOrder, Is.Not.Null);
        Assert.That (workOrderProject.WorkOrder.Status, Is.Not.Null);
        WorkOrderStatus workOrderStatus1 = session.Get<WorkOrderStatus> (1);
        Assert.Multiple (() => {
          Assert.That (workOrderStatus1, Is.Not.Null);
          Assert.That (workOrderProject.WorkOrder.Status, Is.EqualTo (workOrderStatus1));
        });
        // 1st part / Delete
        session.Delete (workOrderProject);
        session.Flush ();
        session.CreateQuery ("delete from PartView foo " +
                             "where foo.Name='TestNewPart'")
          .ExecuteUpdate ();
        session.CreateQuery ("delete from JobView foo " +
                             "where foo.Name='TestNewJob'")
          .ExecuteUpdate ();
        session.CreateQuery ("delete from SimpleOperationView foo " +
                             "where foo.Name='TestNewSimpleOperation'")
          .ExecuteUpdate ();
        session.Flush ();
        session.Delete (workOrder);
        session.Flush ();
        // 2nd part
        Project project = session.CreateCriteria<Project> ()
          .Add (Expression.Eq ("Name", "NEWPROJECT"))
          .UniqueResult<Project> ();
        Assert.That (project, Is.Not.Null);
        Component component = session.CreateCriteria<Component> ()
          .Add (Expression.Eq ("Name", "NEWCOMPONENT"))
          .UniqueResult<Component> ();
        Assert.That (component, Is.Not.Null);
        Assert.Multiple (() => {
          Assert.That (component.Project, Is.EqualTo (project));
          Assert.That (component.Type.Id, Is.EqualTo (1));
        });
        session.Delete (component);
        session.Delete (project);
        /* TODO: something broken here. The number of synchronization log is 0 now
        IList<SynchronizationLog> synchronizationLogs =
          session.CreateCriteria<SynchronizationLog> ()
          .Add (Expression.Eq ("Message", "Persistent class not found"))
          .List<SynchronizationLog> ();
        Assert.AreEqual (7, synchronizationLogs.Count);*/
        session.Flush ();
        // Clean the database
        session.CreateQuery ("delete from Revision")
          .ExecuteUpdate ();
        session.CreateQuery ("delete from SynchronizationLog")
          .ExecuteUpdate ();
        session.Flush ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test the InvalidType exception of the Build method
    /// </summary>
    [Test]
    public void TestInvalidType ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <UnknownType Name=""name"" pulse:action='create' />
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      Assert.Throws<LemoineGDBBuilder.InvalidTypeException>
        (delegate { b.Build (document, cancellationToken: System.Threading.CancellationToken.None); });

      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        IList<SynchronizationLog> synchronizationLogs =
          session.CreateCriteria<SynchronizationLog> ()
          .Add (Expression.Eq ("Message", "Type UnknownType is unknown and was not processed"))
          .List<SynchronizationLog> ();
        Assert.That (synchronizationLogs, Has.Count.EqualTo (1));
        session.Flush ();
        // Clean the database
        session.CreateQuery ("delete from SynchronizationLog")
          .ExecuteUpdate ();
        session.Flush ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test the UnknownProperty exception of the Build method
    /// </summary>
    [Test]
    public void TestUnknownProperty ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
    <JobView Name=""JOB3""
             pulse:action='reference'
             pulse:notfound='fail'
             pulse:relation='none'>
      <ComponentMachineAssociation Begin=""2011-07-01 08:00:00""
                                   other=""to ignore""
                                   pulse:action='create'
                                   pulse:relation='inverse'>
        <Component Name=""PART1"" other=""to ignore""
                   pulse:action='reference' pulse:notfound='log fail' />
        <Machine Name=""MACHINE_A17""
                   pulse:action='reference' pulse:notfound='log fail' />
      </ComponentMachineAssociation>
    </JobView>
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      Assert.Throws<LemoineGDBBuilder.UnknownPropertyException>
        (delegate { b.Build (document, cancellationToken: System.Threading.CancellationToken.None); });

      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        IList<SynchronizationLog> synchronizationLogs =
          session.CreateCriteria<SynchronizationLog> ()
          .Add (Expression.Eq ("Message",
                               "No parent named JobView exists in ComponentMachineAssociation with pulse:relation='inverse'"))
          .List<SynchronizationLog> ();
        Assert.That (synchronizationLogs, Has.Count.EqualTo (1));
        session.Flush ();
        // Clean the database
        session.CreateQuery ("delete from SynchronizationLog")
          .ExecuteUpdate ();
        session.Flush ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test the NotUnique exception of the Build method
    /// </summary>
    [Test]
    public void TestNotUnique ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
    <JobView pulse:action='reference'
             pulse:notfound='fail' />
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      Assert.Throws<LemoineGDBBuilder.NotUniqueException>
        (delegate { b.Build (document, cancellationToken: System.Threading.CancellationToken.None); });
      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        IList<SynchronizationLog> synchronizationLogs =
          session.CreateCriteria<SynchronizationLog> ()
          .List<SynchronizationLog> ();
        Assert.That (synchronizationLogs, Has.Count.EqualTo (1));
        session.Flush ();
        // Clean the database
        session.CreateQuery ("delete from SynchronizationLog")
          .ExecuteUpdate ();
        session.Flush ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test the NotFound exception of the Build method
    /// </summary>
    [Test]
    public void TestNotFound ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
    <JobView Name=""UnknownJob""
             pulse:action='reference'
             pulse:notfound='fail' />
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      Assert.Throws<LemoineGDBBuilder.NotFoundException>
        (delegate { b.Build (document, cancellationToken: System.Threading.CancellationToken.None); });

      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        IList<SynchronizationLog> synchronizationLogs =
          session.CreateCriteria<SynchronizationLog> ()
          .Add (Expression.Eq ("Message", "Persistent class not found"))
          .List<SynchronizationLog> ();
        Assert.That (synchronizationLogs, Has.Count.EqualTo (1));
        session.Flush ();
        // Clean the database
        session.CreateQuery ("delete from SynchronizationLog")
          .ExecuteUpdate ();
        session.Flush ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test an action id
    /// </summary>
    [Test]
    public void TestActionId ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <WorkOrderStatus Id=""1"" pulse:action='id' />
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      b.Build (document, cancellationToken: System.Threading.CancellationToken.None);
    }

    /// <summary>
    /// Test a notfound / create reference process
    /// </summary>
    [Test]
    public void TestComponentTypeReference ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <Component Name=""NEWCOMPONENT"" other=""to ignore""
             pulse:action='reference' pulse:notfound='log create'>
     <Project Name=""NEWPROJECT""
              pulse:action='reference' pulse:notfound='log create' />
     <Type TranslationKey=""UndefinedValue"" pulse:action='reference' pulse:notfound='fail' />
  </Component>
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        // 1st part / Delete
        Project project = session.CreateCriteria<Project> ()
          .Add (Expression.Eq ("Name", "NEWPROJECT"))
          .UniqueResult<Project> ();
        Assert.That (project, Is.Not.Null);
        Component component = session.CreateCriteria<Component> ()
          .Add (Expression.Eq ("Name", "NEWCOMPONENT"))
          .UniqueResult<Component> ();
        Assert.That (component, Is.Not.Null);
        Assert.Multiple (() => {
          Assert.That (component.Project, Is.EqualTo (project));
          Assert.That (component.Type.Id, Is.EqualTo (1));
        });
        session.Delete (component);
        session.Delete (project);
        session.Flush ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test to check the bug where NHibernate tried
    /// to save a transient object is saved
    /// </summary>
    [Test]
    public void TestBugTransientObjectSaved ()
    {
      XmlDocument document = new XmlDocument ();
      document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""10""
                                  pulse:action='reference'
                                  pulse:notfound='create'
                                  pulse:relation='none'>
    <Component Id=""18514""
               pulse:action='id'
               pulse:notfound='fail' />
    <IntermediateWorkPiece Id=""11008""
                           pulse:action='id'
                           pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
</root>");
      LemoineGDBBuilder b = new LemoineGDBBuilder ();
      b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

      using (ISession session = NHibernateHelper.OpenSession ()) {
        IList<ComponentIntermediateWorkPiece> componentIntermediateWorkPieces =
          session.CreateQuery (@"from ComponentIntermediateWorkPiece foo
where foo.Component.Id=18514")
          .List<ComponentIntermediateWorkPiece> ();
        Assert.That (componentIntermediateWorkPieces, Has.Count.EqualTo (1));
        Assert.That (componentIntermediateWorkPieces[0].Component.Name, Is.EqualTo ("PART1"));
      }

      // Clean the database
      using (ISession session = NHibernateHelper.OpenSession ())
      using (ITransaction transaction = session.BeginTransaction ()) {
        session.CreateQuery (@"delete from ComponentIntermediateWorkPiece foo
where foo.Component.Id=18514")
          .ExecuteUpdate ();
        transaction.Commit ();
      }
    }

    /// <summary>
    /// Test to try if we can match
    /// an existing ComponentIntermediateWorkPiece
    /// </summary>
    [Test]
    public void TestExistingComponentIntermediateWorkPiece ()
    {
      {
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""10""
                                  pulse:action='reference'
                                  pulse:notfound='fail'
                                  pulse:relation='none'
                                  pulse:identifiers='Component IntermediateWorkPiece'
                                  pulse:newattributes='Id'>
    <Component Id=""1""
               pulse:action='id'
               pulse:notfound='fail' />
    <IntermediateWorkPiece Id=""2""
                           pulse:action='id'
                           pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        // Check newattributes='Id'
        XPathNavigator node = pathNavigator.SelectSingleNode ("//ComponentIntermediateWorkPiece/@Id");
        Assert.That (node, Is.Not.Null);
        Assert.That (node.Value, Is.EqualTo ("2"));
      }

      {
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""15""
                                  pulse:action='reference'
                                  pulse:notfound='fail'
                                  pulse:relation='none'
                                  pulse:newattributes='Id'
                                  pulse:newelements='IntermediateWorkPiece'>
    <Component Id=""1""
               pulse:action='id'
               pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        // Check newattributes='Id'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//ComponentIntermediateWorkPiece/@Id");
          Assert.That (node, Is.Not.Null);
          Assert.That (node.Value, Is.EqualTo ("11"));
        }

        // Check newattributes='Operation'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//ComponentIntermediateWorkPiece/IntermediateWorkPiece/@Id");
          Assert.That (node, Is.Not.Null);
          Assert.That (node.Value, Is.EqualTo ("13506"));
        }
      }
      {
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""152""
                                  pulse:action='reference'
                                  pulse:notfound=''
                                  pulse:relation='none'
                                  pulse:newattributes='Id'>
    <Component Id=""1""
               pulse:action='id'
               pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        // Check newattributes='Id'
        XPathNavigator node = pathNavigator.SelectSingleNode ("//ComponentIntermediateWorkPiece/@Order");
        Assert.That (node, Is.Null);
      }
    }

    /// <summary>
    /// Test the if directive
    /// </summary>
    [Test]
    public void TestIfCondition ()
    {
      { // if returns true
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""15""
                                  pulse:action='reference'
                                  pulse:notfound='fail'
                                  pulse:relation='none'
                                  pulse:newattributes='Id'
                                  pulse:newelements='IntermediateWorkPiece'>
    <Component Id=""1""
               pulse:action='id'
               pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
  <A pulse:if=""{../ComponentIntermediateWorkPiece/@Id}"">
    <IntermediateWorkPiece Id=""{../../ComponentIntermediateWorkPiece/IntermediateWorkPiece/@Id}""
                           pulse:action='id'
                           pulse:notfound='fail'
                           pulse:newattributes='Name'
                           pulse:newelements='Operation' />
  </A>
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        // Check newattributes='Id'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//A");
          Assert.That (node, Is.Not.Null);
        }

        // Check newattributes='Name'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//A/IntermediateWorkPiece/@Name");
          Assert.That (node, Is.Not.Null);
        }

        // Check newelement='Operation'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//A/IntermediateWorkPiece/Operation/@Id");
          Assert.That (node, Is.Not.Null);
        }
      }

      { // if returns false
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""152""
                                  pulse:action='reference'
                                  pulse:notfound=''
                                  pulse:relation='none'
                                  pulse:newattributes='Id'
                                  pulse:newelements='IntermediateWorkPiece'>
    <Component Id=""1""
               pulse:action='id'
               pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
  <A pulse:if=""{../ComponentIntermediateWorkPiece}"" />
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        // Check newattributes='Id'
        XPathNavigator node = pathNavigator.SelectSingleNode ("//A");
        Assert.That (node, Is.Null);
      }
    }

    /// <summary>
    /// Test the ifnot directive
    /// </summary>
    [Test]
    public void TestIfNotCondition ()
    {
      { // ifnot returns true
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""15""
                                  pulse:action='reference'
                                  pulse:notfound='fail'
                                  pulse:relation='none'
                                  pulse:newattributes='Id'>
    <Component Id=""1""
               pulse:action='id'
               pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
  <A pulse:ifnot=""{../ComponentIntermediateWorkPiece/@Id}"" />
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        // Check newattributes='Id'
        XPathNavigator node = pathNavigator.SelectSingleNode ("//A");
        Assert.That (node, Is.Null);
      }

      { // ifnot returns false
        XmlDocument document = new XmlDocument ();
        document.LoadXml (@"<root
  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
  xmlns:pulse='urn:pulse.lemoinetechnologies.com:synchro:gdb'
  pulse:logthreshold='NOTICE'>
  <ComponentIntermediateWorkPiece Order=""152""
                                  pulse:action='reference'
                                  pulse:notfound=''
                                  pulse:relation='none'
                                  pulse:newattributes='Id'>
    <Component Id=""1""
               pulse:action='id'
               pulse:notfound='fail' />
  </ComponentIntermediateWorkPiece>
  <A pulse:ifnot=""{../ComponentIntermediateWorkPiece}"" />
</root>");
        LemoineGDBBuilder b = new LemoineGDBBuilder ();
        b.Build (document, cancellationToken: System.Threading.CancellationToken.None);

        // document XPath navigator
        XPathNavigator pathNavigator = document.CreateNavigator ();

        // Check newattributes='Id'
        XPathNavigator node = pathNavigator.SelectSingleNode ("//A");
        Assert.That (node, Is.Not.Null);
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");

      if (null == ModelDAOHelper.ModelFactory) {
        ModelDAOHelper.ModelFactory =
          new Lemoine.GDBPersistentClasses.GDBPersistentClassFactory ();
      }
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
