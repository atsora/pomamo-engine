// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Job
  /// </summary>
  [TestFixture]
  public class Job_UnitTest
  {
    private string m_previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Job_UnitTest).FullName);

    /// <summary>
    /// Test the FindPersistentClass method after an insertion
    /// </summary>
    [Test]
    public void TestFindPersistentClass()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        IWorkOrderStatus workOrderStatus = daoFactory.WorkOrderStatusDAO.FindById (1);
        JobView job = new JobView ();
        job.Name = "TestInsertJob";
        job.Status = workOrderStatus;
        session.Save (job);
        session.Flush ();
        JobView job2 = new JobView ();
        job2.Name = "TestInsertJob";
        job2.Status = workOrderStatus;

        JobView job3 = (JobView) job2.FindPersistentClass ();
        Assert.NotNull (job3);
        Assert.AreEqual ("TestInsertJob", job3.Name);

        JobView job4 = (JobView) job2.FindPersistentClass (new string[] {"Name"});
        Assert.NotNull (job4);
        Assert.AreEqual ("TestInsertJob", job4.Name);
        
        transaction.Rollback ();
      }
    }
    

    [OneTimeSetUp]
    public void Init()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
