// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Core.Cache;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Plugin.DynamicTimesOperation;
using NUnit.Framework;
using Pulse.Extensions.Database;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Unit tests for the class <see cref="Lemoine.Plugin.DynamicTimesOperation.OperationEnd"/>
  ///
  /// OperationEnd returns the end of the operation slot that is active at the requested date/time.
  /// The answer depends both on the operation slot at that date/time and on how far the
  /// operation detection went.
  /// </summary>
  [TestFixture]
  [NonParallelizable] // Uses a static operation detection date/time
  public class OperationEnd_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationEnd_UnitTest).FullName);

    static readonly string APPLICABLE_TIME_SPAN_KEY = "DynamicTimesOperation.OperationEnd.ApplicableTimeSpan";

    /// <summary>
    /// Operation detection status with a date/time that can be set by the tests
    /// </summary>
    class OperationDetectionStatusExtension
      : IOperationDetectionStatusExtension
    {
      static DateTime? s_operationDetectionDateTime = null;

      public static void SetOperationDetectionDateTime (DateTime? dateTime)
      {
        s_operationDetectionDateTime = dateTime;
      }

      public bool UniqueInstance => true;

      public int OperationDetectionStatusPriority => 1;

      public DateTime? GetOperationDetectionDateTime () => s_operationDetectionDateTime;

      public bool Initialize (IMachine machine) => true;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public OperationEnd_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    /// <summary>
    /// The operation slot at the requested date/time is completed
    /// Result:
    /// * its end is returned as a final date/time
    /// </summary>
    [Test]
    public void Get_CompletedOperationSlot_Final ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, 10);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        CheckFinal (extension, T (5), T (10));
        CheckFinal (extension, T (6), T (10));
      });
    }

    /// <summary>
    /// The operation slot at the requested date/time is still running and the operation detection
    /// went further than the requested date/time
    /// Result:
    /// * the response is pending, with a hint starting at the operation detection date/time
    /// </summary>
    [Test]
    public void Get_OngoingOperationSlot_PendingWithHintAtOperationDetection ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, null);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        var response = Get (extension, T (6));
        Assert.Multiple (() => {
          Assert.That (response.IsPending (), Is.True, "the response should be pending");
          Assert.That (response.Hint.Lower.HasValue, Is.True, "no hint lower bound");
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (T (100)), "wrong hint lower bound");
        });
      });
    }

    /// <summary>
    /// The operation slot at the requested date/time is still running but the operation detection
    /// did not reach the requested date/time yet
    /// Result:
    /// * the response is pending and the hint is not restricted
    /// </summary>
    [Test]
    public void Get_OngoingOperationSlotNotDetectedYet_PendingWithGivenHint ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, null);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (3));

        var extension = CreateExtension (machine);

        var response = Get (extension, T (6));
        Assert.Multiple (() => {
          Assert.That (response.IsPending (), Is.True, "the response should be pending");
          Assert.That (response.Hint.Lower.HasValue, Is.False, "the hint should not be restricted");
        });
      });
    }

    /// <summary>
    /// There is no operation slot at the requested date/time, which was already detected
    /// Result:
    /// * the dynamic time is not applicable
    /// </summary>
    [Test]
    public void Get_NoOperationSlot_NotApplicable ()
    {
      RunInTransaction (machine => {
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        CheckNotApplicable (extension, T (6));
      });
    }

    /// <summary>
    /// The requested date/time was not detected yet and there is no operation slot there
    /// Result:
    /// * the response is pending, an operation slot may still be detected later
    /// </summary>
    [Test]
    public void Get_NoOperationSlotNotDetectedYet_Pending ()
    {
      RunInTransaction (machine => {
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        CheckPending (extension, T (200));
      });
    }

    /// <summary>
    /// The operation slot at the requested date/time is completed but has no operation
    /// Result:
    /// * the dynamic time is not applicable
    /// </summary>
    [Test]
    public void Get_CompletedOperationSlotWithoutOperation_NotApplicable ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, null, 5, 10);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        CheckNotApplicable (extension, T (6));
      });
    }

    /// <summary>
    /// There is no operation detection status
    /// Result:
    /// * the dynamic time is not applicable
    /// </summary>
    [Test]
    public void Get_NoOperationDetectionStatus_NotApplicable ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, null);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (null);

        var extension = CreateExtension (machine);

        CheckNotApplicable (extension, T (6));
      });
    }

    /// <summary>
    /// A completed operation slot is applicable at its own date/times
    /// </summary>
    [Test]
    public void IsApplicableAt_CompletedOperationSlot_YesAtDateTime ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, 10);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        Assert.Multiple (() => {
          Assert.That (extension.IsApplicableAt (T (5)), Is.EqualTo (DynamicTimeApplicableStatus.YesAtDateTime));
          Assert.That (extension.IsApplicableAt (T (6)), Is.EqualTo (DynamicTimeApplicableStatus.YesAtDateTime));
        });
      });
    }

    /// <summary>
    /// An on-going operation slot is applicable as soon as the date/time has been detected
    /// </summary>
    [Test]
    public void IsApplicableAt_OngoingOperationSlot_YesAtDateTime ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, null);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicableAt (T (6)), Is.EqualTo (DynamicTimeApplicableStatus.YesAtDateTime));
      });
    }

    /// <summary>
    /// An on-going operation slot whose date/time was not detected yet is pending
    /// </summary>
    [Test]
    public void IsApplicableAt_OngoingOperationSlotNotDetectedYet_Pending ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, null);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (3));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicableAt (T (6)), Is.EqualTo (DynamicTimeApplicableStatus.Pending));
      });
    }

    /// <summary>
    /// An on-going operation slot with no operation detection status is pending
    /// </summary>
    [Test]
    public void IsApplicableAt_OngoingOperationSlotNoOperationDetectionStatus_Pending ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, null);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (null);

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicableAt (T (6)), Is.EqualTo (DynamicTimeApplicableStatus.Pending));
      });
    }

    /// <summary>
    /// There is no operation slot at an already detected date/time
    /// </summary>
    [Test]
    public void IsApplicableAt_NoOperationSlot_NoAtDateTime ()
    {
      RunInTransaction (machine => {
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicableAt (T (6)), Is.EqualTo (DynamicTimeApplicableStatus.NoAtDateTime));
      });
    }

    /// <summary>
    /// There is no operation slot yet at a date/time that was not detected yet
    /// </summary>
    [Test]
    public void IsApplicableAt_NoOperationSlotNotDetectedYet_Pending ()
    {
      RunInTransaction (machine => {
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicableAt (T (200)), Is.EqualTo (DynamicTimeApplicableStatus.Pending));
      });
    }

    /// <summary>
    /// A completed operation slot with no operation is not applicable at that date/time
    /// </summary>
    [Test]
    public void IsApplicableAt_CompletedOperationSlotWithoutOperation_NoAtDateTime ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, null, 5, 10);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicableAt (T (6)), Is.EqualTo (DynamicTimeApplicableStatus.NoAtDateTime));
      });
    }

    /// <summary>
    /// There is no operation slot and no operation detection status
    /// </summary>
    [Test]
    public void IsApplicableAt_NoOperationSlotNoOperationDetectionStatus_Never ()
    {
      RunInTransaction (machine => {
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (null);

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicableAt (T (6)), Is.EqualTo (DynamicTimeApplicableStatus.Never));
      });
    }

    /// <summary>
    /// There is no operation detection status
    /// Result:
    /// * the dynamic time is not applicable at all on this machine
    /// </summary>
    [Test]
    public void IsApplicable_NoOperationDetectionStatus_False ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, 10);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (null);

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicable (), Is.False);
      });
    }

    /// <summary>
    /// The machine has no operation slot at all
    /// Result:
    /// * the dynamic time is not applicable at all on this machine
    /// </summary>
    [Test]
    public void IsApplicable_NoOperationSlot_False ()
    {
      RunInTransaction (machine => {
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicable (), Is.False);
      });
    }

    /// <summary>
    /// The only operation slot of the machine is older than the applicable time span
    /// Result:
    /// * the dynamic time is not applicable at all on this machine
    /// </summary>
    [Test]
    public void IsApplicable_OperationSlotOutOfApplicableTimeSpan_False ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, 10);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));
        Lemoine.Info.ConfigSet.ForceValue (APPLICABLE_TIME_SPAN_KEY, TimeSpan.FromDays (1));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicable (), Is.False);
      });
    }

    /// <summary>
    /// The machine has an operation slot within the applicable time span
    /// Result:
    /// * the dynamic time is applicable on this machine
    /// </summary>
    [Test]
    public void IsApplicable_OperationSlotInApplicableTimeSpan_True ()
    {
      RunInTransaction (machine => {
        CreateOperationSlot (machine, GetOperation (), 5, 10);
        OperationDetectionStatusExtension.SetOperationDetectionDateTime (T (100));
        Lemoine.Info.ConfigSet.ForceValue (APPLICABLE_TIME_SPAN_KEY, TimeSpan.FromDays (365 * 100));

        var extension = CreateExtension (machine);

        Assert.That (extension.IsApplicable (), Is.True);
      });
    }

    /// <summary>
    /// The name of the dynamic time, that is used to reference it in the configurations
    /// </summary>
    [Test]
    public void Name_IsOperationEnd ()
    {
      RunInTransaction (machine => {
        var extension = CreateExtension (machine);
        Assert.Multiple (() => {
          Assert.That (extension.Name, Is.EqualTo ("OperationEnd"));
          Assert.That (extension.Machine, Is.EqualTo (machine));
        });
      });
    }

    /// <summary>
    /// A final or a not applicable response can be cached permanently, a pending one can not
    /// </summary>
    [Test]
    public void GetCacheTimeout_PermanentOnlyOnCompletedResponse ()
    {
      RunInTransaction (machine => {
        var extension = CreateExtension (machine);

        Assert.Multiple (() => {
          Assert.That (extension.GetCacheTimeout (extension.CreateFinal (T (10))),
            Is.EqualTo (CacheTimeOut.Permanent.GetTimeSpan ()), "wrong cache timeout for a final response");
          Assert.That (extension.GetCacheTimeout (extension.CreateNotApplicable ()),
            Is.EqualTo (CacheTimeOut.Permanent.GetTimeSpan ()), "wrong cache timeout for a not applicable response");
          Assert.That (extension.GetCacheTimeout (extension.CreateWithHint (new UtcDateTimeRange ("(,)"))),
            Is.EqualTo (CacheTimeOut.CurrentLong.GetTimeSpan ()), "wrong cache timeout for a pending response");
        });
      });
    }

    /// <summary>
    /// Run the body of a test in a transaction that is rolled back afterwards,
    /// with the monitored machine 2
    /// </summary>
    void RunInTransaction (Action<IMonitoredMachine> body)
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null, "no monitored machine with id 2");
          body (machine);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    OperationEnd CreateExtension (IMonitoredMachine machine)
    {
      var extension = new OperationEnd ();
      Assert.That (extension.Initialize (machine, null), Is.True, "the initialization of the extension failed");
      return extension;
    }

    IOperation GetOperation ()
    {
      var operation = ModelDAOHelper.DAOFactory.OperationDAO.FindById (1);
      Assert.That (operation, Is.Not.Null, "no operation with id 1");
      return operation;
    }

    void CreateOperationSlot (IMachine machine, IOperation operation, int start, int? end)
    {
      var operationSlot = ModelDAOHelper.ModelFactory
        .CreateOperationSlot (machine, operation, null, null, null, null, null, null, R (start, end));
      ModelDAOHelper.DAOFactory.OperationSlotDAO.MakePersistent (operationSlot);
      ModelDAOHelper.DAOFactory.Flush ();
    }

    IDynamicTimeResponse Get (IDynamicTimeExtension extension, DateTime at)
    {
      return extension.Get (at, new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime at, DateTime expectedFinal)
    {
      var response = Get (extension, at);
      Assert.Multiple (() => {
        Assert.That (response.Final.HasValue, Is.True, "no final date/time");
        Assert.That (response.Final.Value, Is.EqualTo (expectedFinal), "wrong final date/time");
        Assert.That (response.NoData, Is.False, "unexpected no data");
        Assert.That (response.NotApplicable, Is.False, "unexpected not applicable");
      });
    }

    void CheckNotApplicable (IDynamicTimeExtension extension, DateTime at)
    {
      var response = Get (extension, at);
      Assert.Multiple (() => {
        Assert.That (response.NotApplicable, Is.True, "the response should be not applicable");
        Assert.That (response.Final.HasValue, Is.False, "unexpected final date/time");
      });
    }

    void CheckPending (IDynamicTimeExtension extension, DateTime at)
    {
      var response = Get (extension, at);
      Assert.Multiple (() => {
        Assert.That (response.IsPending (), Is.True, "the response should be pending");
        Assert.That (response.NotApplicable, Is.False, "unexpected not applicable");
      });
    }

    [SetUp]
    public void SetUp ()
    {
      var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
      Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
      extensionsProvider.Add (typeof (OperationDetectionStatusExtension));
    }

    [TearDown]
    public void TearDown ()
    {
      OperationDetectionStatusExtension.SetOperationDetectionDateTime (null);
      Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
      Lemoine.Info.ConfigSet.ResetForceValues ();
    }
  }
}
