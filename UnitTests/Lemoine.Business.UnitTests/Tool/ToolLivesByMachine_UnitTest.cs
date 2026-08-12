// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;

using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;

namespace Lemoine.Business.UnitTests.Tool
{
  /// <summary>
  /// 
  /// </summary>
  public class ToolLivesByMachine_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (ToolLivesByMachine_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ToolLivesByMachine_UnitTest ()
      : base (new DateTime (2019, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestRedundantTools ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Use a different machine to avoid reusing the same cached response between tests.
          var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (2);
          var unit = ModelDAOHelper.DAOFactory.UnitDAO.FindById ((int)UnitId.NumberOfCycles);

          var tp1 = CreateToolPosition (machineModule, "T1", 123);
          var tl1 = CreateToolLife (tp1, unit, 0);
          var tp2 = CreateToolPosition (machineModule, "T2", 123);
          var tl2 = CreateToolLife (tp2, unit, 2);
          var tp3 = CreateToolPosition (machineModule, "T3", 123);
          var tl3 = CreateToolLife (tp3, unit, 5);
          ModelDAOHelper.DAOFactory.Flush ();

          Lemoine.Core.Cache.CacheManager.CacheClient?.FlushAll ();
          var response = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Tool.ToolLivesByMachine (machineModule.MonitoredMachine));
          Assert.That (response, Is.Not.Null);

          var tool1 = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "T1");
          Assert.That (tool1, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (tool1.ActiveSisterTool, Is.False);
            Assert.That (tool1.ValidSisterTools, Is.True);
            Assert.That (tool1.Expired, Is.True);
            Assert.That (tool1.RemainingCyclesToLimit, Is.EqualTo (-1));
            Assert.That (tool1.Group, Is.False);
          });

          var tool2 = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "T2");
          Assert.That (tool2, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (tool2.ActiveSisterTool, Is.True);
            Assert.That (tool2.ValidSisterTools, Is.True);
            Assert.That (tool2.Expired, Is.False);
            Assert.That (tool2.RemainingCyclesToLimit, Is.EqualTo (2));
            Assert.That (tool2.Group, Is.False);
          });

          var tool3 = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "T3");
          Assert.That (tool3, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (tool3.ActiveSisterTool, Is.False);
            Assert.That (tool3.ValidSisterTools, Is.False);
            Assert.That (tool3.Expired, Is.False);
            Assert.That (tool3.RemainingCyclesToLimit, Is.EqualTo (5));
            Assert.That (tool3.Group, Is.False);
          });

          var toolGroup = response.Tools.FirstOrDefault (t => t.Group);
          Assert.That (toolGroup, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (toolGroup.ActiveSisterTool, Is.False);
            Assert.That (toolGroup.ValidSisterTools, Is.False);
            Assert.That (toolGroup.Expired, Is.False);
            Assert.That (toolGroup.RemainingCyclesToLimit, Is.EqualTo (7));
            Assert.That (toolGroup.Display, Is.EqualTo ("T123"));
            Assert.That (toolGroup.Group, Is.True);
          });

          Assert.That (response.Expired, Is.False);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    IToolPosition CreateToolPosition (IMachineModule machineModule, string toolId, int toolNumber)
    {
      var tp1 = ModelDAOHelper.ModelFactory
        .CreateToolPosition (machineModule, toolId);
      tp1.ToolNumber = toolNumber.ToString();
      ModelDAOHelper.DAOFactory.ToolPositionDAO.MakePersistent (tp1);
      return tp1;
    }

    IToolLife CreateToolLife (IToolPosition toolPosition, IUnit unit, int v)
    {
      var tl1 = ModelDAOHelper.ModelFactory
        .CreateToolLife (toolPosition.MachineModule, toolPosition, unit, Core.SharedData.ToolLifeDirection.Down);
      tl1.Value = v;
      tl1.Limit = 5;
      ModelDAOHelper.DAOFactory.ToolLifeDAO.MakePersistent (tl1);
      return tl1;
    }

    IToolLife CreateToolLife (IToolPosition toolPosition, IUnit unit, double value, double limit, double? cycleDelta = null)
    {
      var tl = ModelDAOHelper.ModelFactory
        .CreateToolLife (toolPosition.MachineModule, toolPosition, unit, Core.SharedData.ToolLifeDirection.Down);
      tl.Value = value;
      tl.Limit = limit;
      if (cycleDelta.HasValue) {
        tl.CycleDelta = cycleDelta.Value;
      }
      ModelDAOHelper.DAOFactory.ToolLifeDAO.MakePersistent (tl);
      return tl;
    }

    /// <summary>
    /// Test with Wear type tool lives using CycleDelta
    /// </summary>
    [Test]
    public void TestWearToolsWithCycleDelta ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Use a different machine to keep this test away from the shared cache entry.
          var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (3);
          var wearUnit = ModelDAOHelper.DAOFactory.UnitDAO.FindById ((int)UnitId.ToolWear);

          // Tool 1: Wear = 50, CycleDelta = 5 => Remaining cycles = 50/5 = 10
          var tp1 = CreateToolPosition (machineModule, "W1", 201);
          var tl1 = CreateToolLife (tp1, wearUnit, 50.0, 100.0, 5.0);

          // Tool 2: Wear = 40, CycleDelta = 2 => Remaining cycles = 40/2 = 20
          var tp2 = CreateToolPosition (machineModule, "W2", 202);
          var tl2 = CreateToolLife (tp2, wearUnit, 40.0, 100.0, 2.0);

          // Tool 3: Wear = 5, CycleDelta = 1 => Remaining cycles = 5/1 = 5
          var tp3 = CreateToolPosition (machineModule, "W3", 203);
          var tl3 = CreateToolLife (tp3, wearUnit, 5.0, 100.0, 1.0);

          // Tool 4: Expired (Wear = 0)
          var tp4 = CreateToolPosition (machineModule, "W4", 204);
          var tl4 = CreateToolLife (tp4, wearUnit, 0.0, 100.0, 10.0);

          ModelDAOHelper.DAOFactory.Flush ();

          Lemoine.Core.Cache.CacheManager.CacheClient?.FlushAll ();
          var response = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Tool.ToolLivesByMachine (machineModule.MonitoredMachine));

          Assert.That (response, Is.Not.Null);

          // Tool W1: 10 cycles remaining
          var tool1 = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "W1");
          Assert.That (tool1, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (tool1.Expired, Is.False);
            Assert.That (tool1.RemainingCyclesToLimit, Is.EqualTo (10));
          });

          // Tool W2: 20 cycles remaining
          var tool2 = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "W2");
          Assert.That (tool2, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (tool2.Expired, Is.False);
            Assert.That (tool2.RemainingCyclesToLimit, Is.EqualTo (20));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test with Wear type tool without CycleDelta (should default to 1)
    /// </summary>
    [Test]
    public void TestWearToolsWithoutCycleDelta ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Use a different machine to keep this test away from the shared cache entry.
          var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (4);
          var wearUnit = ModelDAOHelper.DAOFactory.UnitDAO.FindById ((int)UnitId.ToolWear);

          // Tool without CycleDelta: Wear = 25, Limit = 100, no CycleDelta => default delta = 1, Remaining cycles = 25/1 = 25
          var tp1 = CreateToolPosition (machineModule, "WNoDelta", 301);
          var tl1 = CreateToolLife (tp1, wearUnit, 25.0, 100.0, null);

          ModelDAOHelper.DAOFactory.Flush ();

          Lemoine.Core.Cache.CacheManager.CacheClient?.FlushAll ();
          var response = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Tool.ToolLivesByMachine (machineModule.MonitoredMachine));

          Assert.That (response, Is.Not.Null);
          var tool1 = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "WNoDelta");
          Assert.That (tool1, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (tool1.Expired, Is.False);
            // When CycleDelta is null, the code logs an error and considers delta as 1
            Assert.That (tool1.RemainingCyclesToLimit, Is.EqualTo (25));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test with mixed tool types: Wear with CycleDelta and regular NumberOfCycles
    /// </summary>
    [Test]
    public void TestMixedWearAndCycleTools ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Use a different machine to keep this test away from the shared cache entry.
          var machineModule = ModelDAOHelper.DAOFactory.MachineModuleDAO
            .FindById (5);
          var wearUnit = ModelDAOHelper.DAOFactory.UnitDAO.FindById ((int)UnitId.ToolWear);
          var cycleUnit = ModelDAOHelper.DAOFactory.UnitDAO.FindById ((int)UnitId.NumberOfCycles);

          // Wear tool: Value = 40, CycleDelta = 4 => Remaining cycles = 40/4 = 10
          var tpWear = CreateToolPosition (machineModule, "WearMixed", 401);
          var tlWear = CreateToolLife (tpWear, wearUnit, 40.0, 100.0, 4.0);

          // Regular cycle tool: Value = 15, Limit = 20 => Remaining cycles = 15
          var tpCycle = CreateToolPosition (machineModule, "CycleMixed", 402);
          var tlCycle = CreateToolLife (tpCycle, cycleUnit, 15);

          ModelDAOHelper.DAOFactory.Flush ();

          var response = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Tool.ToolLivesByMachine (machineModule.MonitoredMachine));

          Assert.That (response, Is.Not.Null);
          Assert.That (response.Tools.Count, Is.GreaterThanOrEqualTo (2));

          var wearTool = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "WearMixed");
          Assert.That (wearTool, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (wearTool.Expired, Is.False);
            Assert.That (wearTool.RemainingCyclesToLimit, Is.EqualTo (10));
          });

          var cycleTool = response.Tools.FirstOrDefault (t => t.ToolLife.Position.ToolId == "CycleMixed");
          Assert.That (cycleTool, Is.Not.Null);
          Assert.Multiple (() => {
            Assert.That (cycleTool.Expired, Is.False);
            Assert.That (cycleTool.RemainingCyclesToLimit, Is.EqualTo (15));
          });
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
