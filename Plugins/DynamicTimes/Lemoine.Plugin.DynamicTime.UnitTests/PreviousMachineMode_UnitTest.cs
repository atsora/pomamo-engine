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

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class PreviousMachineMode_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (PreviousMachineMode_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public PreviousMachineMode_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestSameMachineMode ()
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
            var extension = new Lemoine.Plugin.SameMachineMode
              .PreviousMachineMode ();
            var initializeResult = extension.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            CheckPending (extension);

            AddFact (machine, R (0, 1), manualActive);
            CheckPending (extension);

            AddFact (machine, R (2, 3), manualActive);
            CheckPending (extension);

            AddFact (machine, R (3, 101), manualActive);
            CheckFinal (extension, T (2));
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
    public void TestSameMachineModeGap ()
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
            var extension = new Lemoine.Plugin.SameMachineMode
              .PreviousMachineMode ();
            var initializeResult = extension.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            CheckPending (extension);

            AddFact (machine, R (0, 1), manualActive);
            CheckPending (extension);

            AddFact (machine, R (2, 3), manualActive);
            CheckPending (extension);

            AddFact (machine, R (101, 102), manualActive);
            CheckNoData (extension);
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

    void CheckFinal (IDynamicTimeExtension extension, DateTime start)
    {
      {
        var response = extension.Get (T (100), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (start, response.Final.Value);
      }
      {
        var response = extension.Get (T (100), new UtcDateTimeRange (start.Subtract (TimeSpan.FromSeconds (1))), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (start, response.Final.Value);
      }
    }

    void CheckNoData (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (100), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.IsTrue (response.NoData);
    }
  }
}
