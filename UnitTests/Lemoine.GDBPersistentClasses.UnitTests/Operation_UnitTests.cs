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
  /// Unit tests for the persistent class Operation.
  /// </summary>
  [TestFixture]
  public class Operation_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (SimpleOperation_UnitTest).FullName);

    /// <summary>
    /// Test the merge function with a created operation
    /// </summary>
    [Test]
    public void TestMergeCreated()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        IOperationType operationType = daoFactory.OperationTypeDAO.FindById(1);
        Operation newOperation = (Operation) ModelDAOHelper.ModelFactory.CreateOperation (operationType);
        newOperation.Name = "CreatedOperation";
        IOperation oldOperation = daoFactory.OperationDAO.FindById (12666);
        IOperation merged = ModelDAOHelper.DAOFactory.OperationDAO.Merge (oldOperation, newOperation,
                                                                          ConflictResolution.Exception);
        Assert.AreEqual (12666, ((Lemoine.Collections.IDataWithId)merged).Id);
        Assert.AreEqual ("CreatedOperation", merged.Name);
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the merge function with an existing operation
    /// </summary>
    [Test]
    public void TestMergeExisting()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        IOperationType operationType = daoFactory.OperationTypeDAO.FindById(1);
        IOperation oldOperation = daoFactory.OperationDAO.FindById (12666);
        IOperation newOperation = daoFactory.OperationDAO.FindById (1);
        IOperation merged = ModelDAOHelper.DAOFactory.OperationDAO
          .Merge (oldOperation, newOperation,
                  ConflictResolution.Exception);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)merged).Id);
        Assert.AreEqual ("SFKPROCESS1", merged.Name);
        Assert.AreEqual (1, merged.Paths.Count, "not a single path");
        bool sequence9found = false;
        foreach (Sequence sequence in merged.Sequences) {
          Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)sequence.Operation).Id);
          if (9 == sequence.Id) { sequence9found = true; }
        }
        Assert.AreEqual (true, sequence9found);
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the merge function with some conflicts
    /// </summary>
    [Test]
    public void TestMergeConflicts()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        IOperation oldOperation = daoFactory.OperationDAO.FindById(2);
        IOperation newOperation = daoFactory.OperationDAO.FindById(1);
        Assert.Throws<ConflictException> (
          new TestDelegate
          (delegate ()
           { ModelDAOHelper.DAOFactory.OperationDAO
               .Merge (oldOperation, newOperation,
                       ConflictResolution.Exception); } ));
        transaction.Rollback ();
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        IOperation oldOperation = daoFactory.OperationDAO.FindById(2);
        IOperation newOperation = daoFactory.OperationDAO.FindById(1);
        IOperation merged = ModelDAOHelper.DAOFactory.OperationDAO
          .Merge (oldOperation, newOperation,
                  ConflictResolution.Overwrite);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)merged).Id);
        Assert.AreEqual ("SFKPROCESS2", merged.Name);
        transaction.Rollback ();
      }
      
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        IOperation oldOperation = daoFactory.OperationDAO.FindById(2);
        IOperation newOperation = daoFactory.OperationDAO.FindById(1);
        IOperation merged = ModelDAOHelper.DAOFactory.OperationDAO
          .Merge (oldOperation, newOperation,
                  ConflictResolution.Keep);
        Assert.AreEqual (1, ((Lemoine.Collections.IDataWithId)merged).Id);
        Assert.AreEqual ("SFKPROCESS1", merged.Name);
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test GetDisplay()
    /// </summary>
    [Test]
    public void TestDisplay ()
    {
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IOperation operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (2);
        Assert.AreEqual ("SFKPROCESS2", operation.Display);
        Assert.AreEqual ("COMPONENT1 <CAVITY> 2", operation.LongDisplay);
        Assert.AreEqual ("2", operation.ShortDisplay);
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
