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
  public class PreviousCycleEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    public PreviousCycleEnd_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    [Test]
    public void GetFinalAndNotApplicableCases ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          var extension = new Lemoine.Plugin.DynamicTimesCycle.PreviousCycleEnd ();
          Assert.That (extension.Initialize (machine, null), Is.True);

          var cycle1 = CreateCycle (machine, 10, 20);
          var cycle2 = CreateCycle (machine, 30, 40);

          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));

          // No previous cycle at T(5) => not applicable
          CheckNotApplicable (extension, T (5));

          // Previous cycle exists and no active cycle at T(25) => final is previous cycle end
          CheckFinal (extension, T (25), T (20));

          // Active cycle at T(15) => not applicable
          CheckNotApplicable (extension, T (15));
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

    IOperationCycle CreateCycle (IMonitoredMachine machine, int beginMinute, int endMinute)
    {
      var cycle = ModelDAOHelper.ModelFactory.CreateOperationCycle (machine);
      cycle.SetRealBegin (T (beginMinute));
      cycle.SetRealEnd (T (endMinute));
      ModelDAOHelper.DAOFactory.OperationCycleDAO.MakePersistent (cycle);
      return cycle;
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
  }
}
