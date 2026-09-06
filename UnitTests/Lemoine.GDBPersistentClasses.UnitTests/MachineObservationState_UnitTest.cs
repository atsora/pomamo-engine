// Copyright (C) 2026 Atsora Solutions
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class MachineObservationState
  /// </summary>
  [TestFixture]
  public class MachineObservationState_UnitTest
  {
    string previousDSNName;

    static readonly ILog log = LogManager.GetLogger (typeof (MachineObservationState_UnitTest).FullName);

    /// <summary>
    /// The color and the operating time, which the 1927 and 1928 migrations added, are
    /// written and read back
    /// </summary>
    [Test]
    public void TestColorAndIsOperatingTime ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var daoSession = daoFactory.OpenSession ())
      using (var transaction = daoSession.BeginTransaction ()) {
        var attended = daoFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Attended);
        Assert.Multiple (() => {
          // No color and no operating time is set on the default rows
          Assert.That (attended.Color, Is.Null);
          Assert.That (attended.IsOperatingTime, Is.Null);
        });

        attended.Color = "#FF8000";
        attended.IsOperatingTime = true;
        daoFactory.MachineObservationStateDAO.MakePersistent (attended);
        daoFactory.FlushData ();

        var read = daoFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Attended);
        Assert.Multiple (() => {
          Assert.That (read.Color, Is.EqualTo ("#FF8000"));
          Assert.That (read.IsOperatingTime, Is.True);
        });

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// A color that is not in the #RRGGBB form is refused by the database
    /// </summary>
    [Test]
    public void TestColorConstraint ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var daoSession = daoFactory.OpenSession ())
      using (var transaction = daoSession.BeginTransaction ()) {
        var attended = daoFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Attended);
        attended.Color = "orange";
        daoFactory.MachineObservationStateDAO.MakePersistent (attended);
        Assert.Throws<NHibernate.Exceptions.GenericADOException> (() => daoFactory.FlushData ());

        transaction.Rollback ();
      }
    }

    [OneTimeSetUp]
    public void Init ()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }

    [OneTimeTearDown]
    public void Dispose ()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
