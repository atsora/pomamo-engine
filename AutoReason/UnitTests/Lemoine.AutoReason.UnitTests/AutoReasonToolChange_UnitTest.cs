// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using Lemoine.Plugin.AutoReasonToolChange;
using Lemoine.GDBPersistentClasses;
using System.Linq;
using Pulse.Extensions.Extension;

namespace Lemoine.AutoReason.UnitTests
{
  /// <summary>
  /// Unit tests for the class AutoReasonToolChange
  /// </summary>
  [TestFixture]
  public class AutoReasonToolChange_UnitTest : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (AutoReasonToolChange_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public AutoReasonToolChange_UnitTest () : base (DateTime.Today.AddMonths (-1))
    {
      Lemoine.Info.ProgramInfo.Name = "NUnit";
    }

    /// <summary>
    /// Test case 1
    /// * tool life reset in a running period
    /// => no auto reason created
    /// </summary>
    [Test]
    public void ToolChangeCase1 ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        using (var transaction = session.BeginTransaction ()) {
          try {
            // Machine and corresponding machine module
            var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
            Assert.NotNull (machine);
            var machineModules = machine.MachineModules;
            Assert.IsNotEmpty (machineModules);
            IMachineModule firstMachineModule = null;
            foreach (var machineModule in machineModules) {
              firstMachineModule = machineModule;
              break;
            }

            // FIRST RUN, INITIALIZING THE PLUGIN ON AN OLD EVENT

            IEventLevel level = ModelDAOHelper.DAOFactory.EventLevelDAO.FindById (1);
            Assert.NotNull (level);

            // Tool life warning at T(0)
            {
              var etl = ModelDAOHelper.ModelFactory.CreateEventToolLife (level, EventToolLifeType.WarningReached, T (0), firstMachineModule);
              etl.ToolId = "1";
              etl.ToolNumber = "1";
              ModelDAOHelper.DAOFactory.EventToolLifeDAO.MakePersistent (etl);
            }

            // Plugin
            var autoReason = GetAutoReasonExtension (machine);
            autoReason.RunOnce ();
            
            // SECOND RUN WITH INTERESTING EVENTS

            // Tool life event corresponding to a reset at T5
            {
              var etl = ModelDAOHelper.ModelFactory.CreateEventToolLife (level, EventToolLifeType.CurrentLifeReset, T (5), firstMachineModule);
              etl.ToolId = "1";
              etl.ToolNumber = "1";
              ModelDAOHelper.DAOFactory.EventToolLifeDAO.MakePersistent (etl);
            }
            autoReason.RunOnce ();

            // THIRD RUN WITH ANOTHER INTERESTING EVENT

            // Tool life event corresponding to a reset at T10
            {
              var etl = ModelDAOHelper.ModelFactory.CreateEventToolLife (level, EventToolLifeType.CurrentLifeReset, T (10), firstMachineModule);
              etl.ToolId = "5";
              etl.ToolNumber = "5";
              ModelDAOHelper.DAOFactory.EventToolLifeDAO.MakePersistent (etl);
            }
            autoReason.RunOnce ();

            // Check that an autoreason appeared
            {
              var reasonAssociations = new ReasonMachineAssociationDAO ().FindAll ()
                .OrderBy (x => x.Range.Lower).ToList ();
              Assert.AreEqual (2, reasonAssociations.Count, "wrong number of associations");

              var association = reasonAssociations[0];
              Assert.AreEqual ("T1", association.ReasonDetails, "wrong T for reason 1");
              Assert.AreEqual (T (5), association.Begin.Value, "wrong start for reason 1");
              Assert.AreEqual (T (5 + 60), association.End.Value, "wrong end for reason 1");

              association = reasonAssociations[1];
              Assert.AreEqual ("T5", association.ReasonDetails, "wrong T for reason 2");
              Assert.AreEqual (T (10), association.Begin.Value, "wrong start for reason 2");
              Assert.AreEqual (T (10 + 60), association.End.Value, "wrong end for reason 2");
            }
          } finally {
            transaction.Rollback ();
          }
        }
      }
    }

    AutoReasonExtension GetAutoReasonExtension (IMonitoredMachine machine)
    {
      var plugin = new Lemoine.Plugin.AutoReasonToolChange.Plugin ();
      plugin.Install (0);
      var autoReason = new AutoReasonExtension ();
      autoReason.TestMode = true;
      autoReason.SetTestConfiguration (@"
{
  ""ReasonScore"": 65.0,
  ""ManualScore"": 100.0,
  ""RightMargin"": ""0:01:00""
}");
      autoReason.Initialize (machine, null);

      return autoReason;
    }

    [SetUp]
    public void Setup ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CycleIsProduction.LastProductionEnd));
      Lemoine.Extensions.ExtensionManager.Add (typeof (Lemoine.Plugin.CycleIsProduction.NextProductionStart));
    }

    [TearDown]
    public void TearDown ()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
    }
  }
}
