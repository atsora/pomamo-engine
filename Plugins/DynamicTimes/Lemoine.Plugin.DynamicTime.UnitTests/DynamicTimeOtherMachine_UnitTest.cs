// Copyright (C) 2026 Atsora Solutions

using System;
using Lemoine.Extensions;
using Lemoine.Extensions.Business.DynamicTimes;
using Lemoine.Extensions.ExtensionsProvider;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugin.DynamicTime.UnitTests
{
  class MockOtherMachineSourceDynamicTime
    : IDynamicTimeExtension
  {
    public enum ResponseMode
    {
      FinalFromMachineId,
      NotApplicableOnOtherFinalOnOwn
    }

    public static ResponseMode Mode { get; set; } = ResponseMode.FinalFromMachineId;

    public IMachine Machine { get; private set; }

    public string Name => "MockOtherMachineSource";

    public bool UniqueInstance => true;

    public bool Initialize (IMachine machine, string parameter)
    {
      this.Machine = machine;
      return true;
    }

    public bool IsApplicable () => true;

    public DynamicTimeApplicableStatus IsApplicableAt (DateTime dateTime)
      => DynamicTimeApplicableStatus.YesAtDateTime;

    public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
    {
      switch (Mode) {
        case ResponseMode.FinalFromMachineId:
          return this.CreateFinal (dateTime.AddMinutes (this.Machine.Id));
        case ResponseMode.NotApplicableOnOtherFinalOnOwn:
          if (this.Machine.Id == 1) {
            return this.CreateFinal (dateTime.AddMinutes (1));
          }
          return this.CreateNotApplicable ();
        default:
          return this.CreateNoData ();
      }
    }

    public TimeSpan GetCacheTimeout (IDynamicTimeResponse data)
      => TimeSpan.FromTicks (0);
  }

  [TestFixture]
  [NonParallelizable] // Uses static mode in mock extension
  public class DynamicTimeOtherMachine_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    public DynamicTimeOtherMachine_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    {
    }

    [Test]
    public void FinalComesFromOtherMachine ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (MockOtherMachineSourceDynamicTime));
          extensionsProvider.Add (typeof (Lemoine.Plugin.DynamicTimeOtherMachine.DynamicTimeExtension));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
          var otherMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);
          Assert.That (otherMachine, Is.Not.Null);

          AttachMachinesToTestCell (machine, otherMachine);

          MockOtherMachineSourceDynamicTime.Mode = MockOtherMachineSourceDynamicTime.ResponseMode.FinalFromMachineId;

          var extension = new Lemoine.Plugin.DynamicTimeOtherMachine.DynamicTimeExtension ();
          extension.SetTestConfiguration ("""
            {
              "Name": "DynamicTimeOtherMachine",
              "NameOnOtherMachine": "MockOtherMachineSource",
              "Select": "First"
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          // Source returns dateTime + machine.Id, and only machine #2 is "other"
          var response = extension.Get (T (0), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
          Assert.That (response.Final.HasValue, Is.True);
          if (response.Final.HasValue) {
            Assert.That (response.Final.Value, Is.EqualTo (T (2)));
          }
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          MockOtherMachineSourceDynamicTime.Mode = MockOtherMachineSourceDynamicTime.ResponseMode.FinalFromMachineId;
          transaction.Rollback ();
        }
      }
    }

    [Test]
    public void NotApplicableFromOtherMachineIsReturned ()
    {
      using (var session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (var transaction = session.BeginTransaction ()) {
        try {
          var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
          Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);
          extensionsProvider.Add (typeof (MockOtherMachineSourceDynamicTime));
          extensionsProvider.Add (typeof (Lemoine.Plugin.DynamicTimeOtherMachine.DynamicTimeExtension));

          var machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (1);
          var otherMachine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (2);
          Assert.That (machine, Is.Not.Null);
          Assert.That (otherMachine, Is.Not.Null);

          AttachMachinesToTestCell (machine, otherMachine);

          MockOtherMachineSourceDynamicTime.Mode = MockOtherMachineSourceDynamicTime.ResponseMode.NotApplicableOnOtherFinalOnOwn;

          var extension = new Lemoine.Plugin.DynamicTimeOtherMachine.DynamicTimeExtension ();
          extension.SetTestConfiguration ("""
            {
              "Name": "DynamicTimeOtherMachine",
              "NameOnOtherMachine": "MockOtherMachineSource",
              "Select": "First"
            }
            """);
          Assert.That (extension.Initialize (machine, null), Is.True);

          var response = extension.Get (T (0), new UtcDateTimeRange ("(,)"), new UtcDateTimeRange ("(,)"));
          Assert.That (response.NotApplicable, Is.True);
        }
        finally {
          Lemoine.Extensions.ExtensionManager.ClearDeactivate ();
          Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
          MockOtherMachineSourceDynamicTime.Mode = MockOtherMachineSourceDynamicTime.ResponseMode.FinalFromMachineId;
          transaction.Rollback ();
        }
      }
    }

    void AttachMachinesToTestCell (IMonitoredMachine machine, IMonitoredMachine otherMachine)
    {
      var cell = ModelDAOHelper.ModelFactory.CreateCell ();
      cell.Name = $"UT-DynamicTimeOtherMachine-{Guid.NewGuid ():N}";
      cell.Kind = CellKind.Sequential;
      ModelDAOHelper.DAOFactory.CellDAO.MakePersistent (cell);

      machine.Cell = cell;
      ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
      otherMachine.Cell = cell;
      ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (otherMachine);
      ModelDAOHelper.DAOFactory.Flush ();
    }
  }
}
