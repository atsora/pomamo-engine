// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Fact
  /// </summary>
  [TestFixture]
  public class Fact_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Fact_UnitTest).FullName);

    /// <summary>
    /// Test the properties of the fact are correctly retrieved
    /// with an equality query
    /// </summary>
    [Test]
    public void TestFactPropertiesEq ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
        DateTime day = new DateTime (2008, 01, 16, 11, 31, 0,
                                     DateTimeKind.Utc);
        ICollection<IFact> facts =
          ModelDAOHelper.DAOFactory.FactDAO
          .FindAllInUtcRange (machine,
                              new UtcDateTimeRange (day, day.AddDays (1)));
        Assert.That (facts, Has.Count.EqualTo (1));
        foreach (IFact fact in facts) {
          log.DebugFormat ("TestFactProperties: read fact ({0}) begin={1}",
                           fact.Machine.Id, fact.Begin);
          Assert.That (fact.Begin.Kind, Is.EqualTo (DateTimeKind.Utc));
        }
      }
    }
    
    /// <summary>
    /// Test the properties of the fact are correctly retrieved
    /// with a 'in range' test
    /// </summary>
    [Test]
    public void TestFactInRange ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
        DateTime from = new DateTime (2008, 01, 16, 11, 50, 00,
                                      DateTimeKind.Local);
        DateTime to = new DateTime (2008, 01, 16, 11, 58, 00,
                                    DateTimeKind.Local);
        ICollection<IFact> facts =
          ModelDAOHelper.DAOFactory.FactDAO
          .FindAllInUtcRange (machine,
                              new UtcDateTimeRange (from, to));
        Assert.That (facts, Has.Count.EqualTo (4));
        foreach (IFact fact in facts) {
          log.DebugFormat ("TestFactProperties: read fact ({0}) begin={1}",
                           fact.Machine.Id, fact.Begin);
          Assert.That (fact.Begin.Kind, Is.EqualTo (DateTimeKind.Utc));
        }
        
        from = new DateTime ();
        to = new DateTime (2007, 08, 01, 14, 58, 20,
                           DateTimeKind.Utc);
        facts =
          ModelDAOHelper.DAOFactory.FactDAO
          .FindAllInUtcRange (machine, new UtcDateTimeRange (from, to));
        Assert.That (facts.Count, Is.Not.EqualTo (0));
        foreach (IFact fact in facts) {
          log.DebugFormat ("TestFactProperties: read fact ({0}) begin={1}",
                           fact.Machine.Id, fact.Begin);
          Assert.That (fact.Begin.Kind, Is.EqualTo (DateTimeKind.Utc));
        }
      }
    }

    /// <summary>
    /// Test the properties of the fact are correctly retrieved
    /// with a 'all after' test
    /// </summary>
    [Test]
    public void TestFactAllAfter ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMonitoredMachine machine =
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
        DateTime from = new DateTime (2008, 01, 16, 11, 50, 00,
                                      DateTimeKind.Local);
        ICollection<IFact> facts =
          ModelDAOHelper.DAOFactory.FactDAO
          .FindAllAfter (machine,
                         from,
                         3);
        Assert.That (facts, Has.Count.EqualTo (3));
      }
    }

    /// <summary>
    /// Test the update of a fact
    /// </summary>
    [Test]
    public void TestFactUpdate ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          IMachineMode newMachineMode = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById (2);
          IFact fact = ModelDAOHelper.DAOFactory.FactDAO
            .FindById (13, machine);
          fact.CncMachineMode = newMachineMode;
          ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
          NHibernateHelper.GetCurrentSession().Flush();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the delete of a fact
    /// </summary>
    [Test]
    public void TestFactDelete ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          IFact fact = ModelDAOHelper.DAOFactory.FactDAO
            .FindById (13, machine);
          ModelDAOHelper.DAOFactory.FactDAO.MakeTransient (fact);
          NHibernateHelper.GetCurrentSession().Flush();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test a lazy load of a fact
    /// </summary>
    [Test]
    public void TestFactPersistentCacheOnly ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          IFact fact = NHibernateHelper.GetCurrentSession ()
            .GetPersistentCacheOnly<IFact> (13);
          Assert.That (fact, Is.EqualTo (null));
          transaction.Rollback ();
        }
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (1);
          IFact fact1 = ModelDAOHelper.DAOFactory.FactDAO
            .FindById(13, machine);
          Assert.That (fact1, Is.Not.EqualTo (null));
          IFact fact2 = NHibernateHelper.GetCurrentSession ()
            .GetPersistentCacheOnly<IFact> (13);
          Assert.That (fact2, Is.Not.EqualTo (null));
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the lock of a fact
    /// </summary>
    [Test]
    public void TestFactLock ()
    {
      IFact fact;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        fact = ModelDAOHelper.DAOFactory.FactDAO
          .FindById (13, 1);
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        ModelDAOHelper.DAOFactory.FactDAO
          .Lock (fact);
      }
    }

    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();
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
