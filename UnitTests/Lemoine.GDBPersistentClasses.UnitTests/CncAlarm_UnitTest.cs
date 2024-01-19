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
  /// Description of CncAlarm_UnitTest.
  /// </summary>
  [TestFixture]
  public class CncAlarm_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncAlarm_UnitTest).FullName);

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
        System.Environment.SetEnvironmentVariable ("DefaultDSNName", previousDSNName);
      }
    }
    #endregion // Setup and dispose

    /// <summary>
    /// Test if CncAlarms can be stored in / removed from the database
    /// </summary>
    [Test]
    public void TestInsertDelete()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession())
        using (IDAOTransaction transaction = daoSession.BeginTransaction())
      {
        var cncAlarmDAO = daoFactory.CncAlarmDAO;
        
        // Retrieve the differents alarms stored
        var cncAlarms = cncAlarmDAO.FindAll();
        int count = cncAlarms.Count;
        
        // Create a machine with a machine module
        var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine();
        machine.MonitoringType = daoFactory.MachineMonitoringTypeDAO.FindById(2); // monitored
        machine.Name = "machine_name";
        daoFactory.MonitoredMachineDAO.MakePersistent(machine);
        var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName(machine, "machinemodule_name");
        daoFactory.MachineModuleDAO.MakePersistent(machineModule);
        
        // Create and add a new cnc alarm
        var range = new UtcDateTimeRange(
          new LowerBound<DateTime>(new DateTime(1900, 1, 1)),
          new UpperBound<DateTime>(new DateTime(1900, 2, 1)));
        var cncAlarm = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, range, "cncInfo", "subCncInfo", "alarmType", "1");
        cncAlarm.Properties["property1"] = "value1";
        cncAlarm.Properties["property2"] = "value2";
        daoFactory.CncAlarmDAO.MakePersistent(cncAlarm);
        ModelDAOHelper.DAOFactory.FlushData();
        ModelDAOHelper.DAOFactory.CncAlarmDAO.Reload(cncAlarm);

        Assert.Multiple (() => {
          // Check that another element is stored
          Assert.That (cncAlarmDAO.FindAll (), Has.Count.EqualTo (count + 1), "Wrong count after insertion");

          // Check the properties (after a reload)
          Assert.That (cncAlarm.Properties, Has.Count.EqualTo (2));
          Assert.That (cncAlarm.CncInfo, Is.EqualTo ("cncInfo"), "wrong cnc info");
          Assert.That (cncAlarm.CncSubInfo, Is.EqualTo ("subCncInfo"), "wrong cnc sub info");
          Assert.That (cncAlarm.Type, Is.EqualTo ("alarmType"), "wrong type");
          Assert.That (cncAlarm.Number, Is.EqualTo ("1"), "wrong name");
        });
        Assert.Multiple (() => {
          Assert.That (cncAlarm.Properties.ContainsKey ("property1"), Is.EqualTo (true));
          Assert.That (cncAlarm.Properties.ContainsKey ("property2"), Is.EqualTo (true));
          Assert.That (cncAlarm.Properties["property1"], Is.EqualTo ("value1"));
          Assert.That (cncAlarm.Properties["property2"], Is.EqualTo ("value2"));
        });

        // Remove the cnc alarm from the database
        cncAlarmDAO.MakeTransient(cncAlarm);

        // Check the number of elements stored
        Assert.That (cncAlarmDAO.FindAll(), Has.Count.EqualTo (count), "Wrong count after deletion");
        
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
        
        // Create alarms
        var dateRange = new UtcDateTimeRange(new DateTime(2000, 1, 1), new DateTime(2000, 1, 2));
        var alarm1 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 1", "C234-H");
        alarm1.Properties["severity"] = "high";
        alarm1.Properties["foo"] = "bar";
        daoFactory.CncAlarmDAO.MakePersistent(alarm1);
        var alarm2 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 3", "D234-H");
        alarm2.Properties["severity"] = "high";
        daoFactory.CncAlarmDAO.MakePersistent(alarm2);
        var alarm3 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 3", "C235-H");
        daoFactory.CncAlarmDAO.MakePersistent(alarm3);
        var alarm4 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 2", "number");
        daoFactory.CncAlarmDAO.MakePersistent(alarm4);
        
        ModelDAOHelper.DAOFactory.FlushData();
        daoFactory.CncAlarmDAO.Reload(alarm1);
        daoFactory.CncAlarmDAO.Reload(alarm2);
        daoFactory.CncAlarmDAO.Reload(alarm3);
        daoFactory.CncAlarmDAO.Reload(alarm4);

        Assert.Multiple (() => {
          // Check that the severities are correct
          Assert.That (alarm1.Severity, Is.EqualTo (cAS), "Wrong severity for alarm 1");
          Assert.That (alarm2.Severity, Is.Null, "Wrong severity for alarm 2");
          Assert.That (alarm3.Severity, Is.Null, "Wrong severity for alarm 3");
          Assert.That (alarm4.Severity, Is.Null, "Wrong severity for alarm 4");
        });

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
        
        // Create alarms
        var dateRange = new UtcDateTimeRange(new DateTime(2000, 1, 1), new DateTime(2000, 1, 2));
        var alarm1 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 1", "n");
        daoFactory.CncAlarmDAO.MakePersistent(alarm1);
        var alarm2 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 2", "n");
        daoFactory.CncAlarmDAO.MakePersistent(alarm2);
        var alarm3 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 3", "n");
        daoFactory.CncAlarmDAO.MakePersistent(alarm3);
        var alarm4 = ModelDAOHelper.ModelFactory.CreateCncAlarm(
          machineModule, dateRange, "Cnc test", "", "type 4", "n");
        daoFactory.CncAlarmDAO.MakePersistent(alarm4);
        
        ModelDAOHelper.DAOFactory.FlushData();
        daoFactory.CncAlarmDAO.Reload(alarm1);
        daoFactory.CncAlarmDAO.Reload(alarm2);
        daoFactory.CncAlarmDAO.Reload(alarm3);
        daoFactory.CncAlarmDAO.Reload(alarm4);

        Assert.Multiple (() => {
          // Check that the severities are correct
          Assert.That (alarm1.Severity, Is.EqualTo (cAS1), "Wrong severity for alarm 1");
          Assert.That (alarm2.Severity, Is.Null, "Wrong severity for alarm 2");
          Assert.That (alarm3.Severity, Is.Null, "Wrong severity for alarm 3");
          Assert.That (alarm4.Severity, Is.Null, "Wrong severity for alarm 4");
        });

        transaction.Rollback();
      }
    }
  }
}
