// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Project
  /// </summary>
  [TestFixture]
  public class Project_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Project_UnitTest).FullName);

    /// <summary>
    /// Test the merge function with components
    /// </summary>
    [Test]
    public void TestMergeComponents()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);

        IComponent componentA = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA.Name = "A";
        IComponent componentB = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentB.Name = "B";
        
        IProject projectA = new Project ();
        projectA.Name = "A";
        projectA.AddComponent (componentA);
        IProject projectB = new Project ();
        projectB.Name = "B";
        projectB.AddComponent (componentB);

        daoFactory.ProjectDAO.MakePersistent(projectA);
        daoFactory.ProjectDAO.MakePersistent(projectB);
        daoFactory.ComponentDAO.MakePersistent(componentA);
        daoFactory.ComponentDAO.MakePersistent(componentB);
        NHibernateHelper.GetCurrentSession ().Flush ();
        
        IProject merged = daoFactory.ProjectDAO
          .Merge (projectB,
                  projectA,
                  ConflictResolution.Keep);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual ("A", merged.Name);
        Assert.AreEqual (2, merged.Components.Count);
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the merge function with components
    /// </summary>
    [Test]
    public void TestMergeComponentsWithConflictedNames()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);

        IComponent componentA1 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA1.Name = "A";
        componentA1.Type = componentType;
        IComponent componentA2 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA2.Name = "A";
        componentA2.Type = componentType;
        
        IProject projectA = new Project ();
        projectA.Name = "A";
        projectA.AddComponent (componentA1);
        IProject projectB = new Project ();
        projectB.Name = "B";
        projectB.AddComponent (componentA2);
        
        daoFactory.ProjectDAO.MakePersistent(projectA);
        daoFactory.ProjectDAO.MakePersistent(projectB);
        daoFactory.ComponentDAO.MakePersistent(componentA1);
        daoFactory.ComponentDAO.MakePersistent(componentA2);
        NHibernateHelper.GetCurrentSession ().Flush ();

        IProject merged = daoFactory.ProjectDAO
          .Merge (projectB,
                  projectA,
                  ConflictResolution.Keep);
        NHibernateHelper.GetCurrentSession ().Flush ();
        
        Assert.AreEqual ("A", merged.Name);
        Assert.AreEqual (2, merged.Components.Count);
        foreach (IComponent component in merged.Components) {
          if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA2).Id) {
            Assert.AreEqual ("A (1)", component.Name);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA1).Id) {
            Assert.AreEqual ("A", component.Name);
          }
          else {
            Assert.Fail ("Bad component");
          }
        }
        
        transaction.Rollback ();
      }
      
      // - Case 2
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);

        IComponent componentA1 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA1.Name = "A";
        componentA1.Type = componentType;
        IComponent componentA2 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA2.Name = "A";
        componentA2.Type = componentType;
        IComponent componentA3 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA3.Name = "A (1)";
        componentA3.Type = componentType;
        IComponent componentA4 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA4.Name = "A (2)";
        componentA4.Type = componentType;
        
        IProject projectA = new Project ();
        projectA.Name = "A";
        projectA.AddComponent (componentA1);
        projectA.AddComponent (componentA3);
        IProject projectB = new Project ();
        projectB.Name = "B";
        projectB.AddComponent (componentA2);
        projectB.AddComponent (componentA4);
        
        daoFactory.ProjectDAO.MakePersistent(projectA);
        daoFactory.ProjectDAO.MakePersistent(projectB);
        daoFactory.ComponentDAO.MakePersistent(componentA1);
        daoFactory.ComponentDAO.MakePersistent(componentA2);
        daoFactory.ComponentDAO.MakePersistent(componentA3);
        daoFactory.ComponentDAO.MakePersistent(componentA4);
        NHibernateHelper.GetCurrentSession ().Flush ();
        
        IProject merged = daoFactory.ProjectDAO
          .Merge (projectB,
                  projectA,
                  ConflictResolution.Keep);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual ("A", merged.Name);
        Assert.AreEqual (4, merged.Components.Count);
        foreach (IComponent component in merged.Components) {
          if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA2).Id) {
            Assert.AreEqual ("A (3)", component.Name);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA1).Id) {
            Assert.AreEqual ("A", component.Name);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA3).Id) {
            Assert.AreEqual ("A (1)", component.Name);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA4).Id) {
            Assert.AreEqual ("A (2)", component.Name);
          }
          else {
            Assert.Fail ("Bad component");
          }
        }
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the merge function with components
    /// </summary>
    [Test]
    public void TestMergeComponentsWithConflictedCodes()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);

        IComponent componentA1 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA1.Code = "A";
        componentA1.Type = componentType;
        IComponent componentA2 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA2.Code = "A";
        componentA2.Type = componentType;
        
        IProject projectA = new Project ();
        projectA.Code = "A";
        projectA.AddComponent (componentA1);
        IProject projectB = new Project ();
        projectB.Code = "B";
        projectB.AddComponent (componentA2);
        
        daoFactory.ProjectDAO.MakePersistent(projectA);
        daoFactory.ProjectDAO.MakePersistent(projectB);
        daoFactory.ComponentDAO.MakePersistent(componentA1);
        daoFactory.ComponentDAO.MakePersistent(componentA2);
        NHibernateHelper.GetCurrentSession ().Flush ();
        
        IProject merged = daoFactory.ProjectDAO
          .Merge (projectB,
                  projectA,
                  ConflictResolution.Keep);
        Assert.AreEqual ("A", merged.Code);
        Assert.AreEqual (2, merged.Components.Count);
        foreach (IComponent component in merged.Components) {
          if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA2).Id) {
            Assert.AreEqual ("A (1)", component.Code);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA1).Id) {
            Assert.AreEqual ("A", component.Code);
          }
          else {
            Assert.Fail ("Bad component");
          }
        }
        
        transaction.Rollback ();
      }
      
      // - Case 2
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IComponentType componentType = daoFactory.ComponentTypeDAO.FindById(1);

        IComponent componentA1 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA1.Code = "A";
        componentA1.Type = componentType;
        IComponent componentA2 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA2.Code = "A";
        componentA2.Type = componentType;
        IComponent componentA3 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA3.Code = "A (1)";
        componentA3.Type = componentType;
        IComponent componentA4 = ModelDAOHelper.ModelFactory.CreateComponentFromType (null, componentType);
        componentA4.Code = "A (2)";
        componentA4.Type = componentType;
        
        IProject projectA = new Project ();
        projectA.Code = "A";
        projectA.AddComponent (componentA1);
        projectA.AddComponent (componentA3);
        IProject projectB = new Project ();
        projectB.Code = "B";
        projectB.AddComponent (componentA2);
        projectB.AddComponent (componentA4);

        daoFactory.ProjectDAO.MakePersistent(projectA);
        daoFactory.ProjectDAO.MakePersistent(projectB);
        daoFactory.ComponentDAO.MakePersistent(componentA1);
        daoFactory.ComponentDAO.MakePersistent(componentA2);
        daoFactory.ComponentDAO.MakePersistent(componentA3);
        daoFactory.ComponentDAO.MakePersistent(componentA4);
        NHibernateHelper.GetCurrentSession ().Flush ();
        
        IProject merged = daoFactory.ProjectDAO
          .Merge (projectB,
                  projectA,
                  ConflictResolution.Keep);
        Assert.AreEqual ("A", merged.Code);
        Assert.AreEqual (4, merged.Components.Count);
        foreach (IComponent component in merged.Components) {
          if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA2).Id) {
            Assert.AreEqual ("A (3)", component.Code);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA1).Id) {
            Assert.AreEqual ("A", component.Code);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA3).Id) {
            Assert.AreEqual ("A (1)", component.Code);
          }
          else if (((Lemoine.Collections.IDataWithId)component).Id == ((Lemoine.Collections.IDataWithId)componentA4).Id) {
            Assert.AreEqual ("A (2)", component.Code);
          }
          else {
            Assert.Fail ("Bad component");
          }
        }
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the merge function in case of a same work order
    /// </summary>
    [Test]
    public void TestMergeWithSameWorkOrder()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IWorkOrderStatus workOrderStatus =
          daoFactory.WorkOrderStatusDAO.FindById(1);
        
        IWorkOrder workOrder = new WorkOrder ();
        workOrder.Name = "WorkOrder";
        workOrder.Status = workOrderStatus;
        daoFactory.WorkOrderDAO.MakePersistent(workOrder);
        
        IProject projectA = new Project ();
        projectA.Name = "A";
        projectA.AddWorkOrder (workOrder);
        daoFactory.ProjectDAO.MakePersistent(projectA);
        IProject projectB = new Project ();
        projectB.Name = "B";
        projectB.AddWorkOrder (workOrder);
        daoFactory.ProjectDAO.MakePersistent(projectB);
        
        IProject merged = daoFactory.ProjectDAO
          .Merge (projectB,
                  projectA,
                  ConflictResolution.Keep);
        Assert.AreEqual ("A", merged.Name);
        
        Assert.AreEqual (1, projectB.WorkOrders.Count);
        foreach (WorkOrder workOrderB in projectB.WorkOrders) {
          Assert.AreEqual (workOrder, workOrderB);
        }
        
        transaction.Rollback ();
      }
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
