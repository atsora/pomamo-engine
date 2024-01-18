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
  public class NextCncValue_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (NextCncValue_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public NextCncValue_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test the activity analysis extension
    /// </summary>
    [Test]
    public void TestNextCncValue ()
    {
      var assemblyLoader = new Lemoine.Core.Plugin.TargetSpecific.AssemblyLoader ();
      var extensionsProvider = new AdditionalExtensionsOnlyProvider ();
      Lemoine.Extensions.ExtensionManager.Initialize (extensionsProvider, force: true);

      extensionsProvider.Add (typeof (Lemoine.Plugin.SameCncValue.NextCncValue));

      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            var extension = new Lemoine.Plugin.SameCncValue
              .NextCncValue ();
            var initializeResult = extension.Initialize (machine, "118");
            Assert.That (initializeResult, Is.True);

            CheckAfter (extension, T (0));
            CheckPending (machine);

            AddCncValue (machineModule, field, R (0, 1), true);
            CheckAfter (extension, T (1));
            CheckAfter (machine, T (1));

            AddCncValue (machineModule, field, R (1, 2), true);
            CheckAfter (extension, T (2));
            CheckAfter (machine, T (2));

            AddCncValue (machineModule, field, R (2, 3), false);
            CheckFinal (extension, T (2));
            CheckFinal (machine, T (2));

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
    public void TestNextCncValueInitialGap ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            var extension = new Lemoine.Plugin.SameCncValue
              .NextCncValue ();
            var initializeResult = extension.Initialize (machine, "2, 118");
            Assert.That (initializeResult, Is.True);

            CheckAfter (extension, T (0));

            AddCncValue (machineModule, field, R (1, 2), true);
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
    public void TestNextCncValueLateGap ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          IMachineModule machineModule = machine.MainMachineModule;
          IField field = ModelDAOHelper.DAOFactory.FieldDAO
            .FindById ((int)FieldId.DryRun);

          {
            var extension = new Lemoine.Plugin.SameCncValue
              .NextCncValue ();
            var initializeResult = extension.Initialize (machine, "118 ");
            Assert.That (initializeResult, Is.True);

            CheckAfter (extension, T (0));

            AddCncValue (machineModule, field, R (0, 1), true);
            CheckAfter (extension, T (1));

            AddCncValue (machineModule, field, R (2, 3), true);
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

    void AddCncValue (IMachineModule machineModule, IField field, UtcDateTimeRange range, object v)
    {
      var cncValue = ModelDAOHelper.ModelFactory
        .CreateCncValue (machineModule, field, range.Lower.Value);
      cncValue.End = range.Upper.Value;
      cncValue.Value = v;
      ModelDAOHelper.DAOFactory.CncValueDAO.MakePersistent (cncValue);
    }

    void CheckPending (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.Multiple (() => {
        Assert.That (response.Hint.Lower.HasValue, Is.False);
        Assert.That (response.Hint.Upper.HasValue, Is.False);
        Assert.That (response.Final.HasValue, Is.False);
      });
    }

    void CheckPending (IMachine machine)
    {
      var response = Lemoine.Business.DynamicTimes.DynamicTime
        .GetDynamicTime ("NextCncValue(2,118)", machine, T (0));
      Assert.Multiple (() => {
        Assert.That (response.Hint.Lower.HasValue, Is.False);
        Assert.That (response.Hint.Upper.HasValue, Is.False);
        Assert.That (response.Final.HasValue, Is.False);
      });
    }

    void CheckAfter (IDynamicTimeExtension extension, DateTime after)
    {
      {
        var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      if (T (1) < after) {
        var response = extension.Get (T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
    }

    void CheckAfter (IMachine machine, DateTime after)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextCncValue(2, 118)", machine, T (0));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      if (T (1) < after) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextCncValue(118 )", machine, T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextCncValue(118)", machine, T (0), new UtcDateTimeRange (after), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Hint.Lower.HasValue, Is.True);
          Assert.That (response.Hint.Lower.Value, Is.EqualTo (after));
          Assert.That (response.Final.HasValue, Is.False);
        });
      }
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final)
    {
      {
        var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      if (T (1) < final) {
        var response = extension.Get (T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (final), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckFinal (IMachine machine, DateTime final)
    {
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextCncValue(118)", machine, T (0));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      if (T (1) < final) {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextCncValue(2, 118)", machine, T (0), R (1), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = Lemoine.Business.DynamicTimes.DynamicTime
          .GetDynamicTime ("NextCncValue(2,118 )", machine, T (0), new UtcDateTimeRange (final), new UtcDateTimeRange ("(,)"));
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }

    void CheckNoData (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NoData, Is.True);
    }

    void CheckNotApplicable (IDynamicTimeExtension extension)
    {
      var response = extension.Get (T (0), R (0), new UtcDateTimeRange ("(,)"));
      Assert.That (response.NotApplicable, Is.True);
    }

    void CheckNoData (IDynamicTimeExtension extension, UtcDateTimeRange limit)
    {
      var response = extension.Get (T (0), R (0), limit);
      Assert.That (response.NoData || (response.Final.HasValue && !limit.ContainsElement (response.Final.Value)) || !response.Hint.Overlaps (limit), Is.True);
    }

    void CheckFinal (IDynamicTimeExtension extension, DateTime final, UtcDateTimeRange limit)
    {
      {
        var response = extension.Get (T (0), R (0), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      if (T (1) < final) {
        var response = extension.Get (T (0), R (1), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
      {
        var response = extension.Get (T (0), new UtcDateTimeRange (final), limit);
        Assert.Multiple (() => {
          Assert.That (response.Final.HasValue, Is.True);
          Assert.That (response.Final.Value, Is.EqualTo (final));
        });
      }
    }
  }
}
