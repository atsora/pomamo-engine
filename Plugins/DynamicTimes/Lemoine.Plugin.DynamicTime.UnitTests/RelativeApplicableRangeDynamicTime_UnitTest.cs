// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  [TestFixture]
  [NonParallelizable] // Uses static CycleDetectionStatusExtension
  public class RelativeApplicableRangeDynamicTime_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    public RelativeApplicableRangeDynamicTime_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    [Test]
    public void NextCycleStartSourceWithApplicableRange ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));
          extensionsProvider.Add (typeof (Lemoine.Plugin.DynamicTimesCycle.NextCycleStart));
          extensionsProvider.Add (typeof (Lemoine.Plugin.RelativeApplicableRangeDynamicTime.ApplicableRange));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          CreateCycle (machine, 30, 35);
          CreateCycle (machine, 40, 45);

          Lemoine.Info.ConfigSet.ForceValue ("DynamicTimesCycle.NextCycleStart.ApplicableTimeSpan", TimeSpan.FromDays (50000));
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));

          var extension = new Lemoine.Plugin.RelativeApplicableRangeDynamicTime.ApplicableRange ();
          extension.SetTestConfiguration ("""
            {
              "Name": "RelativeNextCycleStart",
              "SourceName": "NextCycleStart",
              "AfterDuration": "00:02:00",
              "BeforeDuration": "00:15:00"
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          // NextCycleStart = T(30), delta=20min -> out of range => not applicable
          CheckNotApplicable (extension, T (10));

          // NextCycleStart = T(30), delta=10min -> in range => final
          CheckFinal (extension, T (20), T (30));

          // Active cycle at T(32) => source NextCycleStart is not applicable
          CheckNotApplicable (extension, T (32));
        }
        finally {
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    void CreateCycle (IMonitoredMachine machine, int beginMinute, int endMinute)
    {
      var cycle = ModelDAOHelper.ModelFactory.CreateOperationCycle (machine);
      cycle.SetRealBegin (T (beginMinute));
      cycle.SetRealEnd (T (endMinute));
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (cycle);
    }

    void CheckNotApplicable (IDynamicTimeExtension extension, DateTime at)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NotApplicable, Is.True);
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime at, DateTime expectedFinal)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.That (response.Final.HasValue, Is.True);
      if (response.Final.HasValue) {
        Assert.That (response.Final.Value, Is.EqualTo (expectedFinal));
      }
    }
  }
}
