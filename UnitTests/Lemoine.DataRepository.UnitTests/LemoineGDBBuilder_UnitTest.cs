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
        Assert.AreEqual (2, revisions.Count);
        IUpdater updater = revisions[0].Updater;
        var service = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.ServiceDAO
          .FindById (updater.Id);
        Assert.NotNull (service);
        Assert.AreEqual ("Lemoine Machine Association Synchronization",
                         service.Name);
        // Check newattributes='Id'
        XPathNavigator node = pathNavigator.SelectSingleNode ("//Revision/@Id");
        Assert.NotNull (node);
        Assert.AreEqual (revisions[0].Id.ToString (), node.Value);
        IList<Modification> modifications = session.CreateCriteria<Modification> ()
          .List<Modification> ();
        Assert.AreEqual (2, modifications.Count);
        IList<ComponentMachineAssociation> componentMachineAssociations =
          session.CreateCriteria<ComponentMachineAssociation> ()
          .List<ComponentMachineAssociation> ();
        Assert.AreEqual (1, componentMachineAssociations.Count);
        Assert.AreEqual ("PART1", componentMachineAssociations[0].Component.Name);
        // Check newattributes='Id'
        XPathNavigator partIdNode = pathNavigator.SelectSingleNode ("//Revision/PartView/@ComponentId");
        Assert.NotNull (partIdNode);
        Assert.AreEqual ("18514", partIdNode.Value);
        Assert.AreEqual (18514, ((Lemoine.Collections.IDataWithId)componentMachineAssociations[0].Component).Id);
        IList<OperationMachineAssociation> operationMachineAssociations =
          session.CreateCriteria<OperationMachineAssociation> ()
          .List<OperationMachineAssociation> ();
        Assert.AreEqual (1, operationMachineAssociations.Count);
        Assert.AreEqual (11005, ((Lemoine.Collections.IDataWithId)operationMachineAssociations[0].Operation).Id);
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
        Assert.NotNull (part);
        IJob job = session.CreateCriteria<Project> ()
          .Add (Expression.Eq ("Name", "TestNewJob"))
          .UniqueResult<Project> ().Job;
        Assert.NotNull (job);
        ISimpleOperation simpleOperation = session.CreateCriteria<Operation> ()
          .Add (Expression.Eq ("Name", "TestNewSimpleOperation"))
          .UniqueResult<Operation> ().SimpleOperation;
        Assert.NotNull (simpleOperation);
        IWorkOrder workOrder = session.CreateCriteria<WorkOrder> ()
          .Add (Expression.Eq ("Name", "TestNewWorkOrder"))
          .UniqueResult<WorkOrder> ();
        Assert.NotNull (workOrder);
        IWorkOrderProject workOrderProject =
          session.CreateQuery ("from WorkOrderProject foo " +
                               "where foo.WorkOrder.Name='TestNewWorkOrder' " +
                               "and foo.Project.Name='TestNewPart'")
          .UniqueResult<WorkOrderProject> ();
        Assert.NotNull (workOrderProject);
        Assert.NotNull (workOrderProject.WorkOrder);
        Assert.NotNull (workOrderProject.WorkOrder.Status);
        WorkOrderStatus workOrderStatus1 = session.Get<WorkOrderStatus> (1);
        Assert.NotNull (workOrderStatus1);
        Assert.AreEqual (workOrderStatus1, workOrderProject.WorkOrder.Status);
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
        Assert.NotNull (project);
        Component component = session.CreateCriteria<Component> ()
          .Add (Expression.Eq ("Name", "NEWCOMPONENT"))
          .UniqueResult<Component> ();
        Assert.NotNull (component);
        Assert.AreEqual (project, component.Project);
        Assert.AreEqual (1, component.Type.Id);
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
        Assert.AreEqual (1, synchronizationLogs.Count);
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
        Assert.AreEqual (1, synchronizationLogs.Count);
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
        Assert.AreEqual (1, synchronizationLogs.Count);
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
        Assert.AreEqual (1, synchronizationLogs.Count);
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
        Assert.NotNull (project);
        Component component = session.CreateCriteria<Component> ()
          .Add (Expression.Eq ("Name", "NEWCOMPONENT"))
          .UniqueResult<Component> ();
        Assert.NotNull (component);
        Assert.AreEqual (project, component.Project);
        Assert.AreEqual (1, component.Type.Id);
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
        Assert.AreEqual (1, componentIntermediateWorkPieces.Count);
        Assert.AreEqual ("PART1", componentIntermediateWorkPieces[0].Component.Name);
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
        Assert.NotNull (node);
        Assert.AreEqual ("2", node.Value);
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
          Assert.NotNull (node);
          Assert.AreEqual ("11", node.Value);
        }

        // Check newattributes='Operation'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//ComponentIntermediateWorkPiece/IntermediateWorkPiece/@Id");
          Assert.NotNull (node);
          Assert.AreEqual ("13506", node.Value);
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
        Assert.IsNull (node);
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
          Assert.IsNotNull (node);
        }

        // Check newattributes='Name'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//A/IntermediateWorkPiece/@Name");
          Assert.IsNotNull (node);
        }

        // Check newelement='Operation'
        {
          XPathNavigator node = pathNavigator.SelectSingleNode ("//A/IntermediateWorkPiece/Operation/@Id");
          Assert.IsNotNull (node);
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
        Assert.IsNull (node);
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
        Assert.IsNull (node);
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
        Assert.IsNotNull (node);
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
