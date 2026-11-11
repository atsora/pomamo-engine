// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  /// <summary>
  /// Unit tests for <see cref="Lemoine.Plugin.ActivityIsProduction.NextProductionStart"/>
  /// </summary>
  public class ActivityIsProductionNextProductionStart_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger (typeof (ActivityIsProductionNextProductionStart_UnitTest).FullName);

    static readonly string CONFIGURATION = """
{
  "Manual": false,
  "NorManualNorAuto": true
}
""";

    /// <summary>
    /// Constructor
    /// </summary>
    public ActivityIsProductionNextProductionStart_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Nominal case: idle, then production
    /// </summary>
    [Test]
    public void TestNominal ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
          var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

          var extension = CreateExtension (machine);

          AddFact (machine, R (0, 1), inactive);
          var response = Get (extension, T (0), R (0), R (0, 2, "[]"));
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (T (1)));
          Assert.That (response.Final.HasValue, Is.False);

          AddFact (machine, R (1, 2), active);
          response = Get (extension, T (0), R (1), R (0, 2, "[]"));
          Assert.Multiple (() => {
            Assert.That (response.Final.HasValue, Is.True);
            Assert.That (response.Final.Value, Is.EqualTo (T (1)));
          });
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Gap in the facts after the hint, the next fact (not a production) is after the limit
    ///
    /// Before the fix, the next fact was searched from dateTime (and not from the hint),
    /// the fact before the hint was returned again and the hint never progressed
    /// </summary>
    [Test]
    public void TestGapAfterHintNextFactIdleAfterLimit ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
          var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

          var extension = CreateExtension (machine);

          AddFact (machine, R (0, 1), inactive);
          // Gap [1, 3)
          AddFact (machine, R (3, 4), inactive);
          AddFact (machine, R (4, 5), active);

          // Without any limit: production at T(4)
          {
            var response = Get (extension, T (0), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
            Assert.Multiple (() => {
              Assert.That (response.Final.HasValue, Is.True);
              Assert.That (response.Final.Value, Is.EqualTo (T (4)));
            });
          }

          // First call of the tracker: hint after the idle period
          {
            var response = Get (extension, T (0), R (0), R (0, 2, "[]"));
            Assert.Multiple (() => {
              Assert.That (response.Final.HasValue, Is.False);
              Assert.That (response.Hint.Lower.Value, Is.EqualTo (T (1)));
            });
          }

          // Next call with the hint: the gap must be skipped, the answer must progress
          {
            var response = Get (extension, T (0), R (1), R (0, 2, "[]"));
            Assert.That (response.NoData, Is.True);
          }
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Gap in the facts after the hint, the next fact is a production after the limit
    /// </summary>
    [Test]
    public void TestGapAfterHintNextFactProductionAfterLimit ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
          var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

          var extension = CreateExtension (machine);

          AddFact (machine, R (0, 1), inactive);
          // Gap [1, 3)
          AddFact (machine, R (3, 4), active);

          var response = Get (extension, T (0), R (1), R (0, 2, "[]"));
          Assert.That (response.NoData, Is.True);
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// No fact yet after the hint: pending, then the response progresses once new facts are available
    /// </summary>
    [Test]
    public void TestNoFactAfterHintYet ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);

          var extension = CreateExtension (machine);

          AddFact (machine, R (0, 1), inactive);
          {
            var response = Get (extension, T (0), R (1), R (0, 2, "[]"));
            Assert.Multiple (() => {
              Assert.That (response.NoData, Is.False);
              Assert.That (response.Final.HasValue, Is.False);
              Assert.That (response.Hint.Lower.Value, Is.EqualTo (T (1)));
            });
          }

          AddFact (machine, R (3, 4), inactive);
          {
            var response = Get (extension, T (0), R (1), R (0, 2, "[]"));
            Assert.That (response.NoData, Is.True);
          }
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Gap in the facts after the hint, the next idle fact ends in the limit: the hint progresses after the gap
    /// </summary>
    [Test]
    public void TestGapAfterHintNextFactIdleInLimit ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          var inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Inactive);
          var active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById ((int)MachineModeId.Active);

          var extension = CreateExtension (machine);

          AddFact (machine, R (0, 1), inactive);
          // Gap [1, 3)
          AddFact (machine, R (3, 4), inactive);
          AddFact (machine, R (4, 5), active);

          // Hint [1, 2): no fact in the search range [1, 2),
          // the next fact [3, 4) is idle and ends at T(4) in the limit => hint T(4)
          {
            var response = Get (extension, T (0), R (1, 2), R (0, 10, "[]"));
            Assert.Multiple (() => {
              Assert.That (response.Final.HasValue, Is.False);
              Assert.That (response.NoData, Is.False);
              Assert.That (response.Hint.Lower.Value, Is.EqualTo (T (4)));
            });
          }

          // Hint [1,): the facts after the gap are in the search range
          {
            var response = Get (extension, T (0), R (1), R (0, 10, "[]"));
            Assert.Multiple (() => {
              Assert.That (response.Final.HasValue, Is.True);
              Assert.That (response.Final.Value, Is.EqualTo (T (4)));
            });
          }
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }

    Lemoine.Plugin.ActivityIsProduction.NextProductionStart CreateExtension (IMonitoredMachine machine)
    {
      var extension = new Lemoine.Plugin.ActivityIsProduction.NextProductionStart ();
      extension.SetTestConfiguration (CONFIGURATION);
      var initializeResult = extension.Initialize (machine, "");
      Assert.That (initializeResult, Is.True);
      return extension;
    }

    IDynamicTimeResponse Get (IDynamicTimeExtension extension, DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      var response = extension.Get (dateTime, hint, limit);
      if (log.IsDebugEnabled) {
        log.Debug ($"Get: at={dateTime} hint={hint} limit={limit} => final={response.Final} hint={response.Hint} noData={response.NoData}");
      }
      return response;
    }

    void AddFact (IMonitoredMachine machine, UtcDateTimeRange range, IMachineMode machineMode)
    {
      var fact = ModelDAOHelper.ModelFactory.CreateFact (machine, range.Lower.Value, range.Upper.Value, machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
    }
  }
}
