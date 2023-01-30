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
  public class NewActivityIsProductionStart_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NewActivityIsProductionStart_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NewActivityIsProductionStart_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test the activity analysis extension
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
              .NextProductionStart ();
            extension.SetTestConfiguration (@"
{
  ""Manual"": false,
  ""NorManualNorAuto"": true
}
");
            var initializeResult = extension.Initialize (machine, "");
            Assert.IsTrue (initializeResult);

            {
              var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
              Assert.AreEqual (R (0), response.Hint);
              Assert.IsFalse (response.Final.HasValue);
            }

            AddFact (machine, R (0, 1), inactive);
            {
              var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
              Assert.AreEqual (R (1), response.Hint);
              Assert.IsFalse (response.Final.HasValue);
            }

            AddFact (machine, R (1, 2), manualActive);
            {
              var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
              Assert.IsTrue (response.Hint.Lower.HasValue);
              Assert.AreEqual (T (2), response.Hint.Lower.Value);
              Assert.IsFalse (response.Final.HasValue);
            }
            AddFact (machine, R (2, 3), machining);
            {
              var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
              Assert.IsTrue (response.Final.HasValue);
              Assert.AreEqual (T (2), response.Final.Value);
            }
            {
              var response = extension.Get (T (0), R (1), new UtcDateTimeRange ("(,)"));
              Assert.IsTrue (response.Final.HasValue);
              Assert.AreEqual (T (2), response.Final.Value);
            }
            {
              var response = extension.Get (T (0), R (2), new UtcDateTimeRange ("(,)"));
              Assert.IsTrue (response.Final.HasValue);
              Assert.AreEqual (T (2), response.Final.Value);
            }
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

    void AddFact (IMonitoredMachine machine, UtcDateTimeRange range, IMachineMode machineMode)
    {
      IFact fact = ModelDAOHelper.ModelFactory.CreateFact (machine, range.Lower.Value, range.Upper.Value, machineMode);
      ModelDAOHelper.DAOFactory.FactDAO.MakePersistent (fact);
    }

    void CheckNoData (IDynamicTimeExtension extension, UtcDateTimeRange limit)
    {
      var response = extension.Get (T (0), R (0), limit);
      Assert.IsTrue (response.NoData || (response.Final.HasValue && !limit.ContainsElement (response.Final.Value)));
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
