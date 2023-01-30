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
using Lemoine.Extensions.Business.DynamicTimes;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class LastActivityIsLastProductionEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (LastActivityIsLastProductionEnd_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public LastActivityIsLastProductionEnd_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Inactive);
          IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Active);
          IMachineMode manualActive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.ManualActive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.AutoFeed);
          IMachineMode machining = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Machining);

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .LastProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true
}
");
            var initializeResult = extension.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            CheckPending (extension);

            AddFact (machine, R (0, 1), inactive);
            CheckPending (extension);

            AddFact (machine, R (2, 3), manualActive);
            CheckPending (extension);

            AddFact (machine, R (3, 4), machining);
            CheckAfter (extension, T (4));

            AddFact (machine, R (5, 6), inactive);
            CheckAfter (extension, T (4));

            AddFact (machine, R (90, 101), manualActive);
            CheckFinal (extension, T (4));

            CheckNoData (extension, R (5, 101));
            CheckFinal (extension, T (4), R (3, 100, "[]"));
            CheckFinal (extension, T (4), R (0, 101));
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestWithMaximum ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Inactive);
          IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Active);
          IMachineMode manualActive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.ManualActive);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.AutoFeed);
          IMachineMode machining = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Machining);

          {
            var extension = new Lemoine.Plugin.ActivityIsProduction
              .LastProductionEnd ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true,
  ""Maximum"": ""0:10:00""
}
");
            var initializeResult = extension.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            CheckPending (extension);

            AddFact (machine, R (0, 1), inactive);
            CheckPending (extension);

            AddFact (machine, R (2, 3), manualActive);
            CheckPending (extension);

            AddFact (machine, R (3, 4), machining);
            CheckPending (extension);

            AddFact (machine, R (90, 95), active);
            AddFact (machine, R (95, 105), inactive);
            CheckFinal (extension, T (95));

            CheckNoData (extension, R (96, 101));
            CheckFinal (extension, T (95), R (94, 100, "[]"));
            CheckFinal (extension, T (95), R (0, 101));
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void AddFact (IMonitoredMachine machine, UtcDateTimeRange range, IMachineMode machineMode)
    {
      IFact fact = ModelDAOHelper.ModelFactory.CreateFact (machine, range.Lower.Value, range.Upper.Value, machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
    }

    void CheckPending (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (100), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.IsFalse (response.Hint.Lower.HasValue);
      Assert.IsFalse (response.Hint.Upper.HasValue);
      Assert.IsFalse (response.Final.HasValue);
    }

    void CheckAfter (IDynamicTimeExtension extension, DateTime after)
    {
      {
        var response = extension.Get (T (100), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      {
        var response = extension.Get (T (100), new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.IsFalse (response.Hint.Lower.HasValue);
        Assert.IsFalse (response.Final.HasValue);
      }
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final)
    {
      {
        var response = extension.Get (T (100), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = extension.Get (T (100), new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }

    void CheckNoData (IDynamicTimeExtension extension, UtcDateTimeRange limit)
    {
      DateTime at = T (100);
      var response = extension.Get (at, R (0), limit);
      Assert.IsTrue (response.NoData || (response.Final.HasValue && !limit.ContainsElement (response.Final.Value)));
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final, UtcDateTimeRange limit)
    {
      DateTime at = T (100);
      {
        var response = extension.Get (at, R (0), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      if (T (1) < final) {
        var response = extension.Get (at, R (1), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = extension.Get (at, new UtcDateTimeRange (final.Subtract (TimeSpan.FromSeconds (1))), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }
  }
}
