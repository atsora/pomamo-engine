// Copyright (C) 2024 Atsora Solutions
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
  public class CncVariable_UnitTest
  {
    private string m_previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (CncVariable_UnitTest).FullName);

    /// <summary>
    /// 
    /// </summary>
    [Test]
    public void TestSaveBool ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
          machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO.FindById (2); // monitored
          machine.Name = "machine_name";
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (machine, "machinemodule_test");
          ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent (machineModule);

          var now = DateTime.UtcNow;
          var cncVariable = ModelDAOHelper.ModelFactory.CreateCncVariable (machineModule, new UtcDateTimeRange (now), "TestBool", true);
          ModelDAOHelper.DAOFactory.CncVariableDAO.MakePersistent (cncVariable);
          var id = cncVariable.Id;
          ModelDAOHelper.DAOFactory.Flush ();
          var r = ModelDAOHelper.DAOFactory.CncVariableDAO.FindById (id, machineModule);
          Assert.That (r.Id, Is.EqualTo (id));
          Assert.That (r.Value, Is.True);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
