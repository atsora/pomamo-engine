// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  [TestFixture]
  [NonParallelizable] // Uses static CycleDetectionStatusExtension
  public class NextCycleStart_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    public NextCycleStart_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    [Test]
    public void GetFinalPendingAndNotApplicableCases ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          var extension = new Lemoine.Plugin.DynamicTimesCycle.NextCycleStart ();
          Assert.That (extension.Initialize (machine, null), Is.True);

          CreateCycle (machine, 10, 20);
          CreateCycle (machine, 30, 40);

          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));

          // Active cycle at T(15) => not applicable
          CheckNotApplicable (extension, T (15));

          // No active cycle and a next cycle exists at T(25) => final is next cycle begin
          CheckFinal (extension, T (25), T (30));

          // No active cycle and no next cycle at T(50) => pending
          CheckPending (extension, T (50));
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
      Assert.Multiple (() => {
        Assert.That (response.Final.HasValue, Is.True);
        Assert.That (response.Final.Value, Is.EqualTo (expectedFinal));
      });
    }

    void CheckPending (IDynamicTimeExtension extension, DateTime at)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.Multiple (() => {
        Assert.That (response.Final.HasValue, Is.False);
        Assert.That (response.NotApplicable, Is.False);
      });
    }
  }
}
