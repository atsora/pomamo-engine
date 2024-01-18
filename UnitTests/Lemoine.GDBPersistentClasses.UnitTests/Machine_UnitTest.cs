// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NUnit.Framework;
using Lemoine.Database.Persistent;
using System.Linq;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Machine
  /// </summary>
  [TestFixture]
  public class Machine_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Machine_UnitTest).FullName);

    /// <summary>
    /// Test getting the data on a machine given the ID
    /// </summary>
    [Test]
    public void GetMachine()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        // ID = 1: MACHINE_A17
        Machine machine = session.Get <Machine> (1);
        Assert.Multiple (() => {
          Assert.That (machine.Id, Is.EqualTo (1));
          Assert.That (machine.Name, Is.EqualTo ("MACHINE_A17"));
        });

        // ID = 10 does not exist
        machine = session.Get <Machine> (10);
        log.DebugFormat ("GetMachine: Got machine {0} for ID=10",
                         machine);
        Assert.That (machine, Is.EqualTo (null));
      }
    }
    
    /// <summary>
    /// Test getting the data on a machine given the ID
    /// </summary>
    [Test]
    public void UpdateMachine()
    {
      Machine machine;
      MonitoredMachine monitoredMachine;
      
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        machine = session.Get <Machine> (1);
        Assert.Multiple (() => {
          Assert.That (machine.Id, Is.EqualTo (1));
          Assert.That (machine.Name, Is.EqualTo ("MACHINE_A17"));
        });

        monitoredMachine =
          session.Get <MonitoredMachine> (1);
        Assert.Multiple (() => {
          Assert.That (monitoredMachine.Id, Is.EqualTo (1));
          Assert.That (monitoredMachine.Name, Is.EqualTo ("MACHINE_A17"));
        });

        MachineModule machineModule =
          session.Get <MachineModule> (1);
        Assert.That (machineModule.Name, Is.EqualTo ("machinemodule-1"));
        
        machineModule.MonitoredMachine.Name = "Poi";
        session.Update (machineModule);
        Assert.Multiple (() => {
          Assert.That (monitoredMachine.Name, Is.EqualTo ("Poi"));
          Assert.That (machine.Name, Is.EqualTo ("Poi"));
        });

        monitoredMachine.MainMachineModule.Name = "Abc";
        session.Update (monitoredMachine);
        Assert.That (machineModule.Name, Is.EqualTo ("Abc"));
        
        MonitoredMachine monitoredMachine2 =
          machine as MonitoredMachine;
        Assert.Multiple (() => {
          Assert.That (monitoredMachine.Id, Is.EqualTo (1));
          Assert.That (monitoredMachine.Name, Is.EqualTo ("Poi"));
          Assert.That (monitoredMachine.MainMachineModule.Name, Is.EqualTo ("Abc"));
        });

        session.Flush ();
        transaction.Rollback ();
      }
      
      using (ISession session = NHibernateHelper.OpenSession ())
        using (ITransaction transaction = session.BeginTransaction ())
      {
        session.Lock (machine, LockMode.None);
        Assert.Multiple (() => {
          Assert.That (machine.Id, Is.EqualTo (1));
          Assert.That (machine.Name, Is.EqualTo ("Poi"));
        });

        session.Lock (monitoredMachine, LockMode.None);
        Assert.Multiple (() => {
          Assert.That (monitoredMachine.Id, Is.EqualTo (1));
          Assert.That (monitoredMachine.Name, Is.EqualTo ("Poi"));
        });

        MachineModule machineModule =
          session.Get <MachineModule> (1);
        Assert.That (machineModule.Name, Is.EqualTo ("machinemodule-1"));
        
        machineModule.MonitoredMachine.Name = "Poi";
        session.Update (machineModule);
        Assert.Multiple (() => {
          Assert.That (monitoredMachine.Name, Is.EqualTo ("Poi"));
          Assert.That (machine.Name, Is.EqualTo ("Poi"));
        });

        /*
         * Note: with session.Lock, NHibernate.NonUniqueObjectException
         *       is returned
         * Note2: without session.Lock, machineModule.Name == null
        session.Lock (monitoredMachine.MainMachineModule, LockMode.None);
        monitoredMachine.MainMachineModule.Name = "Abc";
        session.Update (monitoredMachine);
        Assert.AreEqual ("Abc", machineModule.Name);
         */

        session.Flush ();
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test saving a new machine
    /// </summary>
    [Test]
    public void SaveMachine()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IMonitoredMachine machine = ModelDAOHelper.ModelFactory
          .CreateMonitoredMachine ();
        machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO
          .FindById (2);
        machine.Name = "SaveTest";
        ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
        ModelDAOHelper.DAOFactory.FlushData ();
        transaction.Rollback ();
      }

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IMachine machine = ModelDAOHelper.ModelFactory
          .CreateMachine ();
        machine.MonitoringType = ModelDAOHelper.DAOFactory.MachineMonitoringTypeDAO
          .FindById (1);
        machine.Name = "SaveTest";
        ModelDAOHelper.DAOFactory.MachineDAO.MakePersistent (machine);
        ModelDAOHelper.DAOFactory.FlushData ();
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test getting the data on a monitored machine given the ID
    /// </summary>
    [Test]
    public void GetMonitoredMachine()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        // ID = 1: MACHINE_A17
        MonitoredMachine machine = session.Get <MonitoredMachine> (1);
        Assert.Multiple (() => {
          Assert.That (machine.Id, Is.EqualTo (1));
          Assert.That (machine.Name, Is.EqualTo ("MACHINE_A17"));
          Assert.That (machine.OperationFromCnc, Is.EqualTo (true));
        });

        // ID = 10 does not exist
        machine = session.Get <MonitoredMachine> (10);
        log.DebugFormat ("GetMonitoredMachine: Got machine {0} for ID=10",
                         machine);
        Assert.That (machine, Is.EqualTo (null));
      }
    }

    [Test]
    public void GetCncAcquisitionWithMonitoredMachine ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        var cncAcquisition = ModelDAOHelper.DAOFactory.CncAcquisitionDAO
          .FindByIdWithMonitoredMachine (1);
        var machine = cncAcquisition.MachineModules.Single ().MonitoredMachine;
        Assert.That (machine.Name, Is.EqualTo ("MACHINE_A17"));
      }
    }

    /// <summary>
    /// Check that the attribute Version remains the same after a MakePersistent
    /// If the machine is not updated
    /// </summary>
    [Test]
    public void VersionWithMakePersistent ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        // ID = 1: MACHINE_A17
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
        Assert.IsNotNull (machine);
        int initialVersion = machine.Version;

        ModelDAOHelper.DAOFactory.MachineDAO.MakePersistent (machine);
        Assert.That (machine.Version, Is.EqualTo (initialVersion), "the version should have been the same after MakePersistent");

        // The version should remain the same even after a flush and a reload
        ModelDAOHelper.DAOFactory.FlushData ();
        Assert.That (machine.Version, Is.EqualTo (initialVersion), "the version should have been the same after Flush");

        ModelDAOHelper.DAOFactory.MachineDAO.Reload (machine);
        Assert.That (machine.Version, Is.EqualTo (initialVersion), "the version should have been the same after Reload");

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
