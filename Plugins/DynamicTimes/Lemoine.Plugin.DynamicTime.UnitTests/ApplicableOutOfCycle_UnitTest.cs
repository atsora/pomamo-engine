// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  class DynamicEndExtensionApplicableOutOfCycle
    : Lemoine.UnitTests.WithMinuteTimeStamp
    , IDynamicTimeExtension
  {
    static DateTime? s_final = null;
    static bool s_notApplicable = false;
    static bool s_noData = false;

    public DynamicEndExtensionApplicableOutOfCycle ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    public static void SetFinal (DateTime final)
    {
      s_final = final;
      s_notApplicable = false;
      s_noData = false;
    }

    public static void SetNotApplicable ()
    {
      s_final = null;
      s_notApplicable = true;
      s_noData = false;
    }

    public static void SetNoData ()
    {
      s_final = null;
      s_notApplicable = false;
      s_noData = true;
    }

    public static void Reset ()
    {
      s_final = null;
      s_notApplicable = false;
      s_noData = false;
    }

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      return true;
    }

    public IMachine Machine { get; set; }

    public string Name => "SourceDT";

    public bool UniqueInstance => true;

    public bool IsApplicable () => true;

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime at) => DynamicTimeApplicableStatus.YesAtDateTime;

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      if (s_notApplicable) {
        return this.CreateNotApplicable ();
      }
      if (s_noData) {
        return this.CreateNoData ();
      }
      if (s_final.HasValue) {
        return this.CreateFinal (s_final.Value);
      }
      return this.CreateWithHint (hint);
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
    {
      return TimeSpan.FromTicks (0);
    }
  }

  /// <summary>
  /// Unit tests for the ApplicableOutOfCycle dynamic time plugin.
  /// </summary>
  [TestFixture]
  [NonParallelizable] // Uses static CycleDetectionStatusExtension and DynamicEndExtensionApplicableOutOfCycle
  public class ApplicableOutOfCycle_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicableOutOfCycle_UnitTest).FullName);

    public ApplicableOutOfCycle_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// Base and final both out of cycle → Final is returned.
    /// </summary>
    [Test]
    public void TestBothOutOfCycleFinal ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));
          extensionsProvider.Add (typeof (DynamicEndExtensionApplicableOutOfCycle));
          extensionsProvider.Add (typeof (Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          // Cycle at [5, 10), nothing at T(0) or T(15)
          CreateCycle (machine, 5, 10);
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));

          DynamicEndExtensionApplicableOutOfCycle.SetFinal (T (15));

          var extension = new Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle ();
          extension.SetTestConfiguration ("""
            {
              "Name": "ApplicableOutOfCycle",
              "SourceName": "SourceDT",
              "BaseOutOfCycle": true,
              "FinalOutOfCycle": true
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          // T(0) out of cycle, final T(15) out of cycle → Final
          CheckFinal (extension, T (0), T (15));
        }
        finally {
          DynamicEndExtensionApplicableOutOfCycle.Reset ();
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Base dateTime falls inside a cycle and BaseOutOfCycle=true → NotApplicable.
    /// </summary>
    [Test]
    public void TestBaseInCycleNotApplicable ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));
          extensionsProvider.Add (typeof (DynamicEndExtensionApplicableOutOfCycle));
          extensionsProvider.Add (typeof (Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          CreateCycle (machine, 5, 10);
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));

          DynamicEndExtensionApplicableOutOfCycle.SetFinal (T (15));

          var extension = new Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle ();
          extension.SetTestConfiguration ("""
            {
              "Name": "ApplicableOutOfCycle",
              "SourceName": "SourceDT",
              "BaseOutOfCycle": true,
              "FinalOutOfCycle": false
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          // T(6) is inside cycle [5, 10) → NotApplicable
          CheckNotApplicable (extension, T (6));
        }
        finally {
          DynamicEndExtensionApplicableOutOfCycle.Reset ();
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Final dateTime falls inside a cycle and FinalOutOfCycle=true → NotApplicable.
    /// </summary>
    [Test]
    public void TestFinalInCycleNotApplicable ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));
          extensionsProvider.Add (typeof (DynamicEndExtensionApplicableOutOfCycle));
          extensionsProvider.Add (typeof (Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          // Cycle at [5, 10) — final T(7) falls inside it
          CreateCycle (machine, 5, 10);
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));

          DynamicEndExtensionApplicableOutOfCycle.SetFinal (T (7));

          var extension = new Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle ();
          extension.SetTestConfiguration ("""
            {
              "Name": "ApplicableOutOfCycle",
              "SourceName": "SourceDT",
              "BaseOutOfCycle": true,
              "FinalOutOfCycle": true
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          // T(0) is out of cycle but final T(7) is inside cycle → NotApplicable
          CheckNotApplicable (extension, T (0));
        }
        finally {
          DynamicEndExtensionApplicableOutOfCycle.Reset ();
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Cycle detection hasn't reached the final dateTime yet → Pending.
    /// </summary>
    [Test]
    public void TestPendingWhenCycleDetectionBehindFinal ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));
          extensionsProvider.Add (typeof (DynamicEndExtensionApplicableOutOfCycle));
          extensionsProvider.Add (typeof (Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          // Cycle detection only up to T(10), but final is T(15)
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (10));

          DynamicEndExtensionApplicableOutOfCycle.SetFinal (T (15));

          var extension = new Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle ();
          extension.SetTestConfiguration ("""
            {
              "Name": "ApplicableOutOfCycle",
              "SourceName": "SourceDT",
              "BaseOutOfCycle": true,
              "FinalOutOfCycle": true
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          // Cycle detection is behind final T(15) → Pending
          CheckPending (extension, T (0));
        }
        finally {
          DynamicEndExtensionApplicableOutOfCycle.Reset ();
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Cycle detection hasn't reached the base dateTime yet → Pending.
    /// </summary>
    [Test]
    public void TestPendingWhenCycleDetectionBehindBase ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));
          extensionsProvider.Add (typeof (DynamicEndExtensionApplicableOutOfCycle));
          extensionsProvider.Add (typeof (Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          // Cycle detection only up to T(2), base is T(5)
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (2));

          DynamicEndExtensionApplicableOutOfCycle.SetFinal (T (15));

          var extension = new Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle ();
          extension.SetTestConfiguration ("""
            {
              "Name": "ApplicableOutOfCycle",
              "SourceName": "SourceDT",
              "BaseOutOfCycle": true,
              "FinalOutOfCycle": true
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          // Cycle detection T(2) is behind base T(5) → Pending
          CheckPending (extension, T (5));
        }
        finally {
          DynamicEndExtensionApplicableOutOfCycle.Reset ();
          CycleDetectionStatusExtension.SetCycleDetectionDateTime (null);
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Source returns NotApplicable → NotApplicable propagated.
    /// </summary>
    [Test]
    public void TestSourceNotApplicablePropagated ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (CycleDetectionStatusExtension));
          extensionsProvider.Add (typeof (DynamicEndExtensionApplicableOutOfCycle));
          extensionsProvider.Add (typeof (Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);

          CycleDetectionStatusExtension.SetCycleDetectionDateTime (T (100));
          DynamicEndExtensionApplicableOutOfCycle.SetNotApplicable ();

          var extension = new Lemoine.Plugin.ApplicableOutOfCycleDynamicTime.ApplicableOutOfCycle ();
          extension.SetTestConfiguration ("""
            {
              "Name": "ApplicableOutOfCycle",
              "SourceName": "SourceDT",
              "BaseOutOfCycle": true,
              "FinalOutOfCycle": true
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          CheckNotApplicable (extension, T (0));
        }
        finally {
          DynamicEndExtensionApplicableOutOfCycle.Reset ();
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

    void CheckFinal (IDynamicTimeExtension extension, DateTime at, DateTime expectedFinal)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.Multiple (() => {
        Assert.That (response.Final.HasValue, Is.True);
        Assert.That (response.Final.Value, Is.EqualTo (expectedFinal));
      });
    }

    void CheckNotApplicable (IDynamicTimeExtension extension, DateTime at)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NotApplicable, Is.True);
    }

    void CheckPending (IDynamicTimeExtension extension, DateTime at)
    {
      var response = extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
      Assert.Multiple (() => {
        Assert.That (response.Final.HasValue, Is.False);
        Assert.That (response.NotApplicable, Is.False);
        Assert.That (response.NoData, Is.False);
      });
    }
  }
}
