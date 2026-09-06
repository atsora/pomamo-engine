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
    /// The color, which the 1927 migration added, and the capacity level, which the 1920
    /// one did, are written and read back
    ///
    /// The capacity level is what tells whether the machine observation state corresponds
    /// to an operating time: there is no column of its own for that
    /// </summary>
    [Test]
    public void TestColorAndCapacityLevel ()
    {
      var daoFactory = ModelDAOHelper.DAOFactory;
      using (var daoSession = daoFactory.OpenSession ())
      using (var transaction = daoSession.BeginTransaction ()) {
        var attended = daoFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Attended);
        Assert.Multiple (() => {
          // No color and no capacity level is set on the default rows
          Assert.That (attended.Color, Is.Null);
          Assert.That (attended.CapacityLevel, Is.Null);
        });

        attended.Color = "#FF8000";
        attended.CapacityLevel = CapacityLevel.ExpectedProduction;
        daoFactory.MachineObservationStateDAO.MakePersistent (attended);
        daoFactory.FlushData ();

        var read = daoFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Attended);
        Assert.Multiple (() => {
          Assert.That (read.Color, Is.EqualTo ("#FF8000"));
          Assert.That (read.CapacityLevel, Is.EqualTo (CapacityLevel.ExpectedProduction));
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
