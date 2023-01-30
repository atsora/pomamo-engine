// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class OperationCycle
  /// </summary>
  [TestFixture]
  public class OperationCycle_UnitTest
  {
    private string m_previousDSNName;

    static readonly ILog log = LogManager.GetLogger(typeof (OperationCycle_UnitTest).FullName);

    /// <summary>
    /// Test the request to retrieve the operation cycles in range
    /// </summary>
    [Test]
    public void TestOperationCycleInRange()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
        Assert.IsTrue (machine is IMonitoredMachine);
        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          {
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory.CreateOperationCycle (machine as IMonitoredMachine);
            operationCycle.Begin = new DateTime (2012, 01, 01, 10, 00, 00, DateTimeKind.Utc);
            operationCycle.SetRealEnd (new DateTime (2012, 01, 01, 10, 10, 00, DateTimeKind.Utc));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }
          {
            IOperationCycle operationCycle =
              ModelDAOHelper.ModelFactory.CreateOperationCycle (machine as IMonitoredMachine);
            operationCycle.Begin = new DateTime (2012, 01, 01, 11, 00, 00, DateTimeKind.Utc);
            operationCycle.SetRealEnd (new DateTime (2012, 01, 01, 11, 10, 00, DateTimeKind.Utc));
            ModelDAOHelper.DAOFactory.OperationCycleDAO
              .MakePersistent (operationCycle);
          }
          {
            IList<IOperationCycle> operationCycles =
              ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAllInRange (machine as IMonitoredMachine,
                                                                          new UtcDateTimeRange (new DateTime (2012, 01, 01, 10, 50, 00, DateTimeKind.Utc)));
            Assert.AreEqual (1, operationCycles.Count);
          }
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test the request to retrieve the operation cycle at a given date
    /// </summary>
    [Test]
    public void TestOperationCycleAt()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
        Assert.IsTrue (machine is IMonitoredMachine);
        using (IDAOTransaction transaction = session.BeginTransaction ())
        {
          
          IOperationCycle operationCycle1 =
            ModelDAOHelper.ModelFactory.CreateOperationCycle (machine as IMonitoredMachine);
          operationCycle1.Begin = new DateTime (2012, 01, 01, 10, 00, 00);
          operationCycle1.SetRealEnd (new DateTime (2012, 01, 01, 10, 10, 00));
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle1);
          
          
          IOperationCycle operationCycle2 =
            ModelDAOHelper.ModelFactory.CreateOperationCycle (machine as IMonitoredMachine);
          operationCycle2.Begin = new DateTime (2012, 01, 01, 11, 00, 00);
          operationCycle2.SetRealEnd (new DateTime (2012, 01, 01, 11, 10, 00));
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle2);
          
          IOperationCycle operationCycle3 =
            ModelDAOHelper.ModelFactory.CreateOperationCycle (machine as IMonitoredMachine);
          operationCycle3.Begin = new DateTime (2012, 01, 01, 12, 00, 00);
          ModelDAOHelper.DAOFactory.OperationCycleDAO
            .MakePersistent (operationCycle3);
          
          Assert.IsNotNull(operationCycle1, "op cycle 1 should be not null");
          Assert.IsNotNull(operationCycle2, "op cycle 2 should be not null");
          Assert.IsNotNull(operationCycle3, "op cycle 3 should be not null");
          
          IOperationCycle operationCycleFind1 =
            ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAt (machine as IMonitoredMachine,
                                                                new DateTime (2012, 01, 01, 10, 05, 00));
          
          IOperationCycle operationCycleFind2 =
            ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAt (machine as IMonitoredMachine,
                                                                new DateTime (2012, 01, 01, 11, 05, 00));
          
          IOperationCycle operationCycleFind3 =
            ModelDAOHelper.DAOFactory.OperationCycleDAO.FindAt (machine as IMonitoredMachine,
                                                                new DateTime (2012, 01, 01, 12, 05, 00));

          Assert.IsNotNull(operationCycleFind1, "op cycle find 1 should be not null");
          Assert.IsNotNull(operationCycleFind2, "op cycle find 2 should be not null");
          Assert.IsNotNull(operationCycleFind3, "op cycle find 3 should be not null");

          Assert.AreEqual(operationCycle1, operationCycleFind1, "op cycles 1 should be equal");
          Assert.AreEqual(operationCycle2, operationCycleFind2, "op cycles 2 should be equal");
          Assert.AreEqual(operationCycle3, operationCycleFind3, "op cycles 3 should be equal");
          
          transaction.Rollback ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();
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
