// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Description of ToolPosition_UnitTest.
  /// </summary>
  [TestFixture]
  public class ToolPosition_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ToolPosition_UnitTest).FullName);

    #region Setup and dispose
    string previousDSNName;

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName", "LemoineUnitTests");
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName", previousDSNName);
      }
    }
    #endregion // Setup and dispose

    /// <summary>
    /// Test if ToolPositions can be stored in / removed from the database
    /// </summary>
    [Test]
    public void TestInsertDelete ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Retrieve the differents tool positions stored
        var toolPositions = daoFactory.ToolPositionDAO.FindAll ();
        int count = toolPositions.Count;

        // Create a machine with a machine module
        var machine = ModelDAOHelper.ModelFactory.CreateMonitoredMachine ();
        machine.MonitoringType = daoFactory.MachineMonitoringTypeDAO.FindById (2); // monitored
        machine.Name = "machine_name";
        daoFactory.MonitoredMachineDAO.MakePersistent (machine);
        var machineModule = ModelDAOHelper.ModelFactory.CreateMachineModuleFromName (machine, "machinemodule_name");
        daoFactory.MachineModuleDAO.MakePersistent (machineModule);

        // Create and add a new tool position
        var toolPosition = ModelDAOHelper.ModelFactory.CreateToolPosition (machineModule, "toolId1");
        toolPosition.ToolNumber = "1";
        toolPosition.SetProperty ("property1", "value1");
        toolPosition.SetProperty ("property2", "value2");

        daoFactory.ToolPositionDAO.MakePersistent (toolPosition);
        daoFactory.FlushData ();
        daoFactory.ToolPositionDAO.Reload (toolPosition);

        Assert.Multiple (() => {
          // Check that another element is stored
          Assert.That (daoFactory.ToolPositionDAO.FindAll (), Has.Count.EqualTo (count + 1), "Wrong count after insertion");

          // Check the properties (after a reload)
          Assert.That (toolPosition.Version, Is.EqualTo (1), "wrong version here");
          Assert.That (toolPosition.Properties, Has.Count.EqualTo (2));
          Assert.That (toolPosition.ToolId, Is.EqualTo ("toolId1"), "wrong tool id");
        });
        Assert.Multiple (() => {
          Assert.That (toolPosition.Properties.ContainsKey ("property1"), Is.EqualTo (true));
          Assert.That (toolPosition.Properties.ContainsKey ("property2"), Is.EqualTo (true));
          Assert.That (toolPosition.Properties["property1"], Is.EqualTo ("value1"));
          Assert.That (toolPosition.Properties["property2"], Is.EqualTo ("value2"));
        });
        int currentVersion = toolPosition.Version;

        // Update the properties
        toolPosition.SetProperty ("property1", "value3");
        daoFactory.ToolPositionDAO.MakePersistent (toolPosition);
        daoFactory.FlushData ();
        daoFactory.ToolPositionDAO.Reload (toolPosition);

        // Check the new properties (after a reload)
        Assert.That (toolPosition.Properties, Has.Count.EqualTo (2));
        Assert.Multiple (() => {
          Assert.That (toolPosition.Properties.ContainsKey ("property1"), Is.EqualTo (true));
          Assert.That (toolPosition.Properties.ContainsKey ("property2"), Is.EqualTo (true));
          Assert.That (toolPosition.Properties["property1"], Is.EqualTo ("value3"));
          Assert.That (toolPosition.Properties["property2"], Is.EqualTo ("value2"));
          Assert.That (toolPosition.Version, Is.EqualTo (currentVersion + 1), "wrong version");
        });

        // Remove the tool position from the database
        daoFactory.ToolPositionDAO.MakeTransient (toolPosition);

        // Check the number of elements stored
        Assert.That (daoFactory.ToolPositionDAO.FindAll (), Has.Count.EqualTo (count), "Wrong count after deletion");
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test MakePersistent with no change
    /// (the version number shouldn't change)
    /// </summary>
    [Test]
    public void TestVersionWithMakePersistent ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Check there is at least one tool position in the database
        var toolPositions = daoFactory.ToolPositionDAO.FindAll ();
        Assert.Greater (toolPositions.Count, 0, "at least one tool position should have been stored in the database");

        // Take the first tool position, keep the version in a variable
        var toolPosition = toolPositions[0];
        int initialVersion = toolPosition.Version;

        // Call "MakePersistent" on this untouched tool position and check that the version is the same
        daoFactory.ToolPositionDAO.MakePersistent (toolPosition);
        Assert.That (toolPosition.Version, Is.EqualTo (initialVersion), "the version should have been the same after MakePersistent");

        // The version should remain the same even after a flush and a reload
        daoFactory.FlushData ();
        Assert.That (toolPosition.Version, Is.EqualTo (initialVersion), "the version should have been the same after Flush");

        daoFactory.ToolPositionDAO.Reload (toolPosition);
        Assert.That (toolPosition.Version, Is.EqualTo (initialVersion), "the version should have been the same after Reload");

        transaction.Rollback ();
      }
    }
  }
}
