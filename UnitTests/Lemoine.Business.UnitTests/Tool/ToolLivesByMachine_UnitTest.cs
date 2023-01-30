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

          var response = Lemoine.Business.ServiceProvider
            .Get (new Lemoine.Business.Tool.ToolLivesByMachine (machineModule.MonitoredMachine));
          Assert.IsNotNull (response);
          {
            int i = 0;
            {
              var t = response.Tools[i];
              Assert.IsFalse (t.ActiveSisterTool);
              Assert.IsTrue (t.ValidSisterTools);
              Assert.IsTrue (t.Expired);
              Assert.AreEqual (-1, t.RemainingCyclesToLimit);
              Assert.IsFalse (t.Group);
            }
            ++i;
            {
              var t = response.Tools[i];
              Assert.IsTrue (t.ActiveSisterTool);
              Assert.IsTrue (t.ValidSisterTools);
              Assert.IsFalse (t.Expired);
              Assert.AreEqual (2, t.RemainingCyclesToLimit);
              Assert.IsFalse (t.Group);
            }
            ++i;
            {
              var t = response.Tools[i];
              Assert.IsFalse (t.ActiveSisterTool);
              Assert.IsFalse (t.ValidSisterTools);
              Assert.IsFalse (t.Expired);
              Assert.AreEqual (5, t.RemainingCyclesToLimit);
              Assert.IsFalse (t.Group);
            }
            ++i;
            {
              var t = response.Tools[i];
              Assert.IsFalse (t.ActiveSisterTool);
              Assert.IsFalse (t.ValidSisterTools);
              Assert.IsFalse (t.Expired);
              Assert.AreEqual (7, t.RemainingCyclesToLimit);
              Assert.AreEqual ("T123", t.Display);
              Assert.IsTrue (t.Group);
            }
          }
          Assert.IsFalse (response.Expired);
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
  }
}
