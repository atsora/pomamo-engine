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
  /// Unit tests for the class Path (also Sequence and Operation)
  /// </summary>
  [TestFixture]
  public class Path_UnitTest
  {
    string previousDSNName;
    static readonly ILog log = LogManager.GetLogger(typeof (Path_UnitTest).FullName);

    /// <summary>
    /// Test several paths on same operation
    /// </summary>
    [Test]
    public void TestMultiPaths()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession();
        IOperationType operationType = ModelDAOHelper.DAOFactory.OperationTypeDAO.FindById (1);
        IOperation operation = ModelDAOHelper.ModelFactory.CreateOperation(operationType);
        operation.Name = "operation1";
        
        IPath path1 = ModelDAOHelper.ModelFactory.CreatePath();
        path1.Number = 1;
        IPath path2 = ModelDAOHelper.ModelFactory.CreatePath();
        path2.Number = 2;
        
        // set distinct numbers for multi-paths
        // otherwise operation won't register second path with same number !
        
        path1.Operation = operation;
        path2.Operation = operation;
        
        Assert.AreEqual(2, operation.Paths.Count);
        
        ISequence seq1 = ModelDAOHelper.ModelFactory.CreateSequence("seq1");
        seq1.Order = 1;
        ISequence seq2 = ModelDAOHelper.ModelFactory.CreateSequence("seq2");
        seq2.Order = 2;
        
        seq1.Path = path1;
        seq2.Path = path1;
        
        Assert.AreEqual(2, path1.Sequences.Count);
        Assert.AreEqual(0, path2.Sequences.Count);
        Assert.AreEqual(seq1.Operation.Name, operation.Name);
        Assert.AreEqual(seq2.Operation.Name, operation.Name);
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
