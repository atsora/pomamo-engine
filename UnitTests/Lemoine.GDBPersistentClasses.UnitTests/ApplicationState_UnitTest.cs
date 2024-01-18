// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class ApplicationState_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (ApplicationState_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ApplicationState_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestNullableDateTimeNull ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var key = "Test";
          DateTime? dateTime = null;
          var applicationState = ModelDAOHelper.ModelFactory.CreateApplicationState (key);
          applicationState.Value = dateTime;
          ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (applicationState);

          ModelDAOHelper.DAOFactory.Flush ();

          var applicationState2 = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (key);
          Assert.That (applicationState2.Value, Is.EqualTo (dateTime));

          Assert.That (applicationState2.Value is null, Is.True);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void TestNullableDateTimeNotNull ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var key = "Test";
          DateTime? dateTime = DateTime.UtcNow;
          var applicationState = ModelDAOHelper.ModelFactory.CreateApplicationState (key);
          applicationState.Value = dateTime;
          ModelDAOHelper.DAOFactory.ApplicationStateDAO.MakePersistent (applicationState);

          var applicationState2 = ModelDAOHelper.DAOFactory.ApplicationStateDAO
            .GetApplicationState (key);
          Assert.That (applicationState2.Value, Is.EqualTo (dateTime));
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
