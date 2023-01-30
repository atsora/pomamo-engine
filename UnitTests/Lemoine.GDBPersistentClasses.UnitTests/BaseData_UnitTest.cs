// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;

using NHibernate;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class BaseData
  /// </summary>
  [TestFixture]
  public class BaseData_UnitTest
  {
    private string previousDSNName;

    static readonly ILog log = LogManager.GetLogger(typeof (BaseData_UnitTest).FullName);

    /// <summary>
    /// Test the method FindPersistentClass
    /// </summary>
    [Test]
    public void TestFindPersistentClass()
    {
      IComponentType componentType = ModelDAOHelper.ModelFactory.CreateComponentTypeFromName ("Test");
      
      Component component = (Component) ModelDAOHelper.ModelFactory.CreateComponentFromName (null, "COMPONENT1", componentType);
      component.Type = null;
      component = component.FindPersistentClass<Component> ();
      Assert.AreEqual (1, component.Id);
      
      component = (Component) ModelDAOHelper.ModelFactory.CreateComponentFromName (null, "Unknown", componentType);
      component.Type = null;
      component = component.FindPersistentClass<Component> ();
      Assert.Null (component);
      
      component = (Component) ModelDAOHelper.ModelFactory.CreateComponentFromType (new Project (), componentType);
      component.Project.Name = "JOB1";
      component.Type = new ComponentType ();
      component.Type.Name = "CAVITY";
      component = component.FindPersistentClass<Component> ();
      Assert.NotNull (component);
      Assert.AreEqual (1, component.Id);

      component = (Component) ModelDAOHelper.ModelFactory.CreateComponentFromType (new Project (), componentType);
      component.Project.Name = "JOB1";
      component.Type = new ComponentType ();
      component.Type.Name = "CAVITY";
      component = component.FindPersistentClass<Component> (new string[] {"Name", "Project", "Type.Name"});
      Assert.NotNull (component);
      Assert.AreEqual (1, component.Id);

      component = (Component) ModelDAOHelper.ModelFactory.CreateComponentFromType (new Project (), componentType);
      component.Project.Name = "JOB1";
      component.Type = new ComponentType ();
      component.Type.Name = "Unknown";
      component = component.FindPersistentClass<Component> (new string[] {"Name", "Project", "Type.Name"});
      Assert.Null (component);
      
      component = (Component) ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
      component.Type = new ComponentType ();
      component.Type.Name = "CAVITY";
      component = component.FindPersistentClass<Component> (new string[] {"Type"});
      Assert.NotNull (component);
      Assert.AreEqual (1, component.Id);
      
      Computer computer = new Computer ();
      computer.IsLctr = true;
      computer = computer.FindPersistentClass<Computer> ();
      Assert.NotNull (computer);
      Assert.AreEqual (1, computer.Id);
      
      IntermediateWorkPiece intermediateWorkPiece =
        new IntermediateWorkPiece (null);
      intermediateWorkPiece.Name = "SFKPROCESS2";
      intermediateWorkPiece = intermediateWorkPiece.FindPersistentClass<IntermediateWorkPiece> ();
      Assert.NotNull (intermediateWorkPiece);
      Assert.AreEqual (2, intermediateWorkPiece.Id);
      Assert.AreEqual ("SFKPROCESS2", intermediateWorkPiece.Name);
      
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        session.Lock (component, LockMode.Read);
        ComponentIntermediateWorkPiece ciwp =
          new ComponentIntermediateWorkPiece (component, intermediateWorkPiece);
        ciwp = ciwp.FindPersistentClass<ComponentIntermediateWorkPiece> ();
        Assert.NotNull (intermediateWorkPiece);
      }
    }
    
    /// <summary>
    /// Test the method FindPersistentClass for a relation
    /// </summary>
    [Test]
    public void TestRelationFindPersistentClass()
    {
      WorkOrder workOrder = new WorkOrder ();
      workOrder.Name = "JOB1";
      Project project = new Project ();
      project.Name = "JOB1";
      WorkOrderProject workOrderProject = new WorkOrderProject (workOrder, project);
      workOrderProject = workOrderProject.FindPersistentClass<WorkOrderProject> ();
      Assert.NotNull (workOrderProject);
      Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)workOrderProject.WorkOrder).Id);
      Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)workOrderProject.Project).Id);
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
