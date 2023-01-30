// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;
using Lemoine.Database.Persistent;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the persistent class SimpleOperation.
  /// </summary>
  [TestFixture]
  public class SimpleOperation_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (SimpleOperation_UnitTest).FullName);

    /// <summary>
    /// Test the insertion of a new simple operation
    /// </summary>
    [Test]
    public void TestInsert()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        OperationType operationType = session.Get<OperationType> (1);
        SimpleOperationView simpleOperation = new SimpleOperationView ();
        simpleOperation.Name = "TestInsertSimpleOperation";
        simpleOperation.Type = operationType;
        session.Save (simpleOperation);
        session.Flush ();
        transaction.Rollback ();
      }
      
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        OperationType operationType = session.Get<OperationType> (1);
        Component component = session.Get<Component> (1);
        SimpleOperationView simpleOperation = new SimpleOperationView ();
        simpleOperation.Name = "TestInsertSimpleOperation";
        simpleOperation.Type = operationType;
        simpleOperation.Component = component;
        session.Save (simpleOperation);
        session.Flush ();
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test a selection of a SimpleOperation
    /// </summary>
    [Test]
    public void TestSelect()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        SimpleOperationView simpleOperation = session.Get<SimpleOperationView> (11008);
        Assert.NotNull (simpleOperation);
        Assert.AreEqual ("SIMPLEOPERATION1", simpleOperation.Name);
      }
    }
    
    /// <summary>
    /// Test the merge function with an existing operation
    /// </summary>
    [Test]
    public void TestMergeExisting()
    {
      ISimpleOperation oldOperation;
      ISimpleOperation newOperation;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        oldOperation = ModelDAOHelper.ModelFactory
          .CreateSimpleOperation (ModelDAOHelper.DAOFactory.OperationDAO.FindById (12666));
        newOperation = ModelDAOHelper.ModelFactory
          .CreateSimpleOperation (ModelDAOHelper.DAOFactory.OperationDAO.FindById (1));
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (oldOperation);
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (newOperation);
        
        ISimpleOperation merged =
          ModelDAOHelper.DAOFactory.SimpleOperationDAO
          .Merge (oldOperation, newOperation,
                  ConflictResolution.Exception);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual (1, merged.OperationId);
        Assert.AreEqual ("SFKPROCESS1", merged.Name);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the merge function with an operation with an order
    /// </summary>
    [Test]
    public void TestMergeExistingWithOrder()
    {
      ISimpleOperation oldOperation;
      ISimpleOperation newOperation;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        oldOperation = ModelDAOHelper.ModelFactory
          .CreateSimpleOperation (ModelDAOHelper.DAOFactory.OperationDAO.FindById (12666));
        newOperation = ModelDAOHelper.ModelFactory
          .CreateSimpleOperation (ModelDAOHelper.DAOFactory.OperationDAO.FindById (1));
      }
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (oldOperation);
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (newOperation);
        
        IComponent newComponent = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (1);
        IComponent oldComponent = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (2);
        
        IIntermediateWorkPiece oldIwp = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindById (12980);
        IIntermediateWorkPiece newIwp = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindById (1);
        
        oldComponent.AddIntermediateWorkPiece (oldIwp, 10);
        newComponent.AddIntermediateWorkPiece (newIwp, 10);
        
        ISimpleOperation merged =
          ModelDAOHelper.DAOFactory.SimpleOperationDAO
          .Merge (oldOperation, newOperation,
                  ConflictResolution.Exception);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual (1, merged.OperationId);
        Assert.AreEqual ("SFKPROCESS1", merged.Name);
        Assert.AreEqual (2, newOperation.ComponentIntermediateWorkPieces.Count);
        foreach (IComponentIntermediateWorkPiece ciwp in newOperation.ComponentIntermediateWorkPieces) {
          Assert.AreEqual (10, ciwp.Order.Value);
        }
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the merge function with an operation with an order
    /// </summary>
    [Test]
    public void TestMergeExistingWithOrderSameComponent()
    {
      ISimpleOperation oldOperation;
      ISimpleOperation newOperation;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        oldOperation = ModelDAOHelper.ModelFactory
          .CreateSimpleOperation (ModelDAOHelper.DAOFactory.OperationDAO.FindById (12666));
        newOperation = ModelDAOHelper.ModelFactory
          .CreateSimpleOperation (ModelDAOHelper.DAOFactory.OperationDAO.FindById (1));
      }
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (oldOperation);
        ModelDAOHelper.DAOFactory.SimpleOperationDAO.Lock (newOperation);

        IComponent component = ModelDAOHelper.DAOFactory.ComponentDAO.FindById (1);
        
        IIntermediateWorkPiece oldIwp = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindById (12980);
        IIntermediateWorkPiece newIwp = ModelDAOHelper.DAOFactory.IntermediateWorkPieceDAO.FindById (1);
        
        component.AddIntermediateWorkPiece (oldIwp, 10);
        component.AddIntermediateWorkPiece (newIwp);
        
        ISimpleOperation merged =
          ModelDAOHelper.DAOFactory.SimpleOperationDAO
          .Merge (oldOperation, newOperation,
                  ConflictResolution.Exception);
        NHibernateHelper.GetCurrentSession ().Flush ();
        Assert.AreEqual (1, merged.OperationId);
        Assert.AreEqual ("SFKPROCESS1", merged.Name);
        Assert.AreEqual (1, newOperation.ComponentIntermediateWorkPieces.Count);
        foreach (IComponentIntermediateWorkPiece ciwp in newOperation.ComponentIntermediateWorkPieces) {
          Assert.AreEqual (10, ciwp.Order.Value);
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
