// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Config
  /// </summary>
  [TestFixture]
  public class Config_UnitTest
  {
    private string m_previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Config_UnitTest).FullName);

    /// <summary>
    /// Test insert / read
    /// </summary>
    [Test]
    public void TestSaveUpdate()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        Config config = new Config ("Test");
        config.Value = true;
        session.Save (config);
        session.Flush ();
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test insert / read a boolean
    /// </summary>
    [Test]
    public void TestBool()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IConfig config = ModelDAOHelper.ModelFactory.CreateConfig ("Test");
        config.Value = true;
        ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
        
        IConfig config2 = ModelDAOHelper.DAOFactory.ConfigDAO.GetConfig ("Test");
        Assert.IsNotNull (config2);
        Assert.AreEqual (true, config2.Value);

        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test insert / read an int
    /// </summary>
    [Test]
    public void TestInt()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IConfig config = ModelDAOHelper.ModelFactory.CreateConfig ("Test");
        config.Value = 3;
        ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
        
        IConfig config2 = ModelDAOHelper.DAOFactory.ConfigDAO.GetConfig ("Test");
        Assert.IsNotNull (config2);
        Assert.AreEqual (3, config2.Value);

        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test insert / read a TimeSpan
    /// </summary>
    [Test]
    public void TestTimeSpan()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IConfig config = ModelDAOHelper.ModelFactory.CreateConfig ("Test");
        config.Value = TimeSpan.FromMinutes (3);
        ModelDAOHelper.DAOFactory.ConfigDAO.MakePersistent (config);
        
        IConfig config2 = ModelDAOHelper.DAOFactory.ConfigDAO.GetConfig ("Test");
        Assert.IsNotNull (config2);
        Assert.AreEqual (TimeSpan.FromMinutes (3), config2.Value);

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
