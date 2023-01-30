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
using Pulse.Extensions;
using Lemoine.Extensions;
using Lemoine.Extensions.ExtensionsProvider;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Description of SetupSwitcher_UnitTest.
  /// </summary>
  public class NextMachineMode_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NextMachineMode_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NextMachineMode_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestNextMachineMode ()
    {
      var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
      var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
      var pluginFilter = new PluginFilterFromFlag (PluginFlag.AutoReason);
      Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

      extensionsProvider.Add (typeof (Lemoine.Plugin.SameMachineMode.NextMachineMode));

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
              .NextMachineMode ();
            var initializeResult = extension.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            CheckAfter (extension, T (0));
            CheckPending (machine);

            AddFact (machine, R (0, 1), manualActive);
            CheckAfter (extension, T (1));
            CheckAfter (machine, T (1));

            AddFact (machine, R (1, 2), manualActive);
            CheckAfter (extension, T (2));
            CheckAfter (machine, T (2));

            AddFact (machine, R (2, 3), machining);
            CheckFinal (extension, T (2));
            CheckFinal (machine, T (2));

            CheckNoData (extension, R (0, 1));
            CheckNoData (extension, R (0, 2));
            CheckFinal (extension, T (2), R (0, 2, "[]"));
            CheckFinal (extension, T (2), R (0, 3));
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestNextMachineModeInitialGap ()
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
              .NextMachineMode ();
            var initializeResult = extension.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            CheckAfter (extension, T (0));

            AddFact (machine, R (1, 2), manualActive);
            CheckNotApplicable (extension);
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
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestNextMachineModeLateGap ()
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
              .NextMachineMode ();
            var initializeResult = extension.Initialize (machine, null);
            Assert.IsTrue (initializeResult);

            CheckAfter (extension, T (0));

            AddFact (machine, R (0, 1), manualActive);
            CheckAfter (extension, T (1));

            AddFact (machine, R (2, 3), manualActive);
            CheckFinal (extension, T (1));

            CheckNoData (extension, R (0, 1));
            CheckFinal (extension, T (1), R (0, 1, "[]"));
            CheckFinal (extension, T (1), R (0, 2));
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
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.IsFalse (response.Hint.Lower.HasValue);
      Assert.IsFalse (response.Hint.Upper.HasValue);
      Assert.IsFalse (response.Final.HasValue);
    }

    void CheckPending (IMachine machine)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime ("NextMachineMode", machine, T (0));
      Assert.IsFalse (response.Hint.Lower.HasValue);
      Assert.IsFalse (response.Hint.Upper.HasValue);
      Assert.IsFalse (response.Final.HasValue);
    }

    void CheckAfter (IDynamicTimeExtension extension, DateTime after)
    {
      {
        var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      if (T (1) < after) {
        var response = extension.Get (T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
    }

    void CheckAfter (IMachine machine, DateTime after)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextMachineMode", machine, T (0));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      if (T (1) < after) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextMachineMode", machine, T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextMachineMode", machine, T (0), new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Hint.Lower.HasValue);
        Assert.AreEqual (after, response.Hint.Lower.Value);
        Assert.IsFalse (response.Final.HasValue);
      }
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final)
    {
      {
        var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      if (T (1) < final) {
        var response = extension.Get (T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (final), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }

    void CheckFinal (IMachine machine, DateTime final)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextMachineMode", machine, T (0));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      if (T (1) < final) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextMachineMode", machine, T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextMachineMode", machine, T (0), new UtcDateTimeRange (final), new UtcDateTimeRange ("(,)"));
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }

    void CheckNoData (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.IsTrue (response.NoData);
    }

    void CheckNotApplicable (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.IsTrue (response.NotApplicable);
    }

    void CheckNoData (IDynamicTimeExtension extension, UtcDateTimeRange limit)
    {
      var response = extension.Get (T (0), R (0), limit);
      Assert.IsTrue (response.NoData);
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final, UtcDateTimeRange limit)
    {
      {
        var response = extension.Get (T (0), R (0), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      if (T (1) < final) {
        var response = extension.Get (T (0), R (1), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (final), limit);
        Assert.IsTrue (response.Final.HasValue);
        Assert.AreEqual (final, response.Final.Value);
      }
    }
  }
}
