// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for a persistent class
  /// </summary>
  [TestFixture]
  public class CurrentCncAlarm_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CurrentCncAlarm_UnitTest).FullName);
    
    #region Setup and dispose
    string previousDSNName;
    
    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable("DefaultDSNName");
      System.Environment.SetEnvironmentVariable("DefaultDSNName", "LemoineUnitTests");
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable("DefaultDSNName", previousDSNName);
      }
    }
    #endregion // Setup and dispose
    
    /// <summary>
    /// Test if CurrentCncAlarms can be stored in / removed from the database
    /// </summary>
    [Test]
    public void TestInsertDelete()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession())
        using (IDAOTransaction transaction = daoSession.BeginTransaction())
      {
        var currentCncAlarmDAO = daoFactory.CurrentCncAlarmDAO;
        
        // Retrieve the differents alarms stored
        var currentCncAlarms = currentCncAlarmDAO.FindAll();
        int count = currentCncAlarms.Count;
        
        // Create a machine with a machine module
        var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine();
        machine.MonitoringType = daoFactory.MachineMonitoringTypeDAO.FindById(2); // monitored
        machine.Name = "machine_name";
        daoFactory.MonitoredMachineDAO.MakePersistent(machine);
        var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName(machine, "machinemodule_test");
        daoFactory.MachineModuleDAO.MakePersistent(machineModule);
        
        // Create and add a new current cnc alarm
        var datetime = new DateTime(1900, 2, 1);
        var currentCncAlarm = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "cncInfo", "subCncInfo", "alarmType", "1");
        currentCncAlarm.Properties["property1"] = "value1";
        currentCncAlarm.Properties["property2"] = "value2";
        daoFactory.CurrentCncAlarmDAO.MakePersistent(currentCncAlarm);
        ModelDAOHelper.DAOFactory.Flush();
        ModelDAOHelper.DAOFactory.CurrentCncAlarmDAO.Reload(currentCncAlarm);

        Assert.Multiple (() => {
          // Check that another element is stored
          Assert.That (currentCncAlarmDAO.FindAll (), Has.Count.EqualTo (count + 1), "Wrong count after insertion");

          // Check the properties (after a reload)
          Assert.That (currentCncAlarm.Properties, Has.Count.EqualTo (2));
          Assert.That (currentCncAlarm.CncInfo, Is.EqualTo ("cncInfo"), "wrong cnc info");
          Assert.That (currentCncAlarm.CncSubInfo, Is.EqualTo ("subCncInfo"), "wrong cnc sub info");
          Assert.That (currentCncAlarm.Type, Is.EqualTo ("alarmType"), "wrong type");
          Assert.That (currentCncAlarm.Number, Is.EqualTo ("1"), "wrong name");
        });
        Assert.Multiple (() => {
          Assert.That (currentCncAlarm.Properties.ContainsKey ("property1"), Is.EqualTo (true));
          Assert.That (currentCncAlarm.Properties.ContainsKey ("property2"), Is.EqualTo (true));
          Assert.That (currentCncAlarm.Properties["property1"], Is.EqualTo ("value1"));
          Assert.That (currentCncAlarm.Properties["property2"], Is.EqualTo ("value2"));
        });

        // Remove the cnc alarm from the database
        currentCncAlarmDAO.MakeTransient(currentCncAlarm);

        // Check the number of elements stored
        Assert.That (currentCncAlarmDAO.FindAll(), Has.Count.EqualTo (count), "Wrong count after deletion");
        
        transaction.Rollback();
      }
    }
    
    /// <summary>
    /// Test if the severity is correctly determined
    /// </summary>
    [Test]
    public void TestSeverity()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession())
        using (IDAOTransaction transaction = daoSession.BeginTransaction())
      {
        // Creation of a monitored machine and a machine module
        var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine();
        machine.Name = "machine_test";
        machine.MonitoringType = ModelDAOHelper.DAOFactory
          .MachineMonitoringTypeDAO.FindById(2); // Monitored
        ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent(machine);
        var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName(machine, "machinemodule_test");
        ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent(machineModule);
        
        // Create a severity
        var cAS = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity("Cnc test", "severity test");
        daoFactory.CncAlarmSeverityDAO.MakePersistent(cAS);
        
        // Create a severity pattern
        var pattern = new CncAlarmSeverityPatternRules ();
        pattern.Type = "type 1";
        pattern.Number = "^C[0-9]{3}-[A-Z]+$";
        pattern.Properties["severity"] = "high";
        var cASP = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern("Cnc test", pattern, cAS);
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent(cASP);
        
        // Create current alarms
        var datetime = new DateTime(2000, 1, 2);
        var alarm1 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 1", "C234-H");
        alarm1.Properties["severity"] = "high";
        alarm1.Properties["foo"] = "bar";
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm1);
        var alarm2 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 1", "D234-H");
        alarm1.Properties["severity"] = "high";
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm2);
        var alarm3 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 1", "C235-H");
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm3);
        var alarm4 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 2", "number");
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm4);
        
        ModelDAOHelper.DAOFactory.Flush();
        daoFactory.CurrentCncAlarmDAO.Reload(alarm1);
        daoFactory.CurrentCncAlarmDAO.Reload(alarm2);
        daoFactory.CurrentCncAlarmDAO.Reload(alarm3);
        daoFactory.CurrentCncAlarmDAO.Reload(alarm4);

        // Check that the severities are correct
        Assert.That (alarm1.Severity, Is.EqualTo (cAS), "Wrong severity for alarm 1");
        Assert.IsNull(alarm2.Severity, "Wrong severity for alarm 2");
        Assert.IsNull(alarm3.Severity, "Wrong severity for alarm 3");
        Assert.IsNull(alarm4.Severity, "Wrong severity for alarm 4");
        
        transaction.Rollback();
      }
    }
    
    /// <summary>
    /// Test that deactivated patterns and severities are not taken into account
    /// when assiging a severity to an alarm
    /// </summary>
    [Test]
    public void TestDeactivatedPatterns()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = ModelDAOHelper.DAOFactory.OpenSession())
        using (IDAOTransaction transaction = daoSession.BeginTransaction())
      {
        // Creation of a monitored machine and a machine module
        var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine();
        machine.Name = "machine_test";
        machine.MonitoringType = ModelDAOHelper.DAOFactory
          .MachineMonitoringTypeDAO.FindById(2); // Monitored
        ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent(machine);
        var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName(machine, "machinemodule_test");
        ModelDAOHelper.DAOFactory.MachineModuleDAO.MakePersistent(machineModule);
        
        // Create two severities
        var cAS1 = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity("Cnc test", "severity test");
        daoFactory.CncAlarmSeverityDAO.MakePersistent(cAS1);
        
        var cAS2 = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity("Cnc test", "disabled severity");
        cAS2.Status = EditStatus.DEFAULT_VALUE_DELETED;
        daoFactory.CncAlarmSeverityDAO.MakePersistent(cAS2);
        
        // Create three severity patterns
        var pattern1 = new CncAlarmSeverityPatternRules ();
        pattern1.Type = "type 1";
        var cASP1 = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern("Cnc test", pattern1, cAS1);
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent(cASP1);
        
        var pattern2 = new CncAlarmSeverityPatternRules ();
        pattern2.Type = "type 2";
        var cASP2 = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern("Cnc test", pattern2, cAS2);
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent(cASP2);
        
        var pattern3 = new CncAlarmSeverityPatternRules ();
        pattern3.Type = "type 3";
        var cASP3 = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern("Cnc test", pattern3, cAS1);
        cASP3.Status = EditStatus.DEFAULT_VALUE_DELETED;
        daoFactory.CncAlarmSeverityPatternDAO.MakePersistent(cASP3);
        
        // Create current alarms
        var datetime = new DateTime(2000, 1, 2);
        var alarm1 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 1", "n");
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm1);
        var alarm2 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 2", "n");
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm2);
        var alarm3 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 3", "n");
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm3);
        var alarm4 = ModelDAOHelper.ModelFactory.CreateCurrentCncAlarm(
          machineModule, datetime, "Cnc test", "", "type 4", "n");
        daoFactory.CurrentCncAlarmDAO.MakePersistent(alarm4);
        
        ModelDAOHelper.DAOFactory.Flush();
        daoFactory.CurrentCncAlarmDAO.Reload(alarm1);
        daoFactory.CurrentCncAlarmDAO.Reload(alarm2);
        daoFactory.CurrentCncAlarmDAO.Reload(alarm3);
        daoFactory.CurrentCncAlarmDAO.Reload(alarm4);

        // Check that the severities are correct
        Assert.That (alarm1.Severity, Is.EqualTo (cAS1), "Wrong severity for alarm 1");
        Assert.IsNull(alarm2.Severity, "Wrong severity for alarm 2");
        Assert.IsNull(alarm3.Severity, "Wrong severity for alarm 3");
        Assert.IsNull(alarm4.Severity, "Wrong severity for alarm 4");
        
        transaction.Rollback();
      }
    }
  }
}
