// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// 
  /// </summary>
  public class StampingConfigByName_UnitTest
    : Lemoine.UnitTests.WithMinuteTimeStamp
  {
    readonly ILog log = LogManager.GetLogger (typeof (StampingConfigByName_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public StampingConfigByName_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }

    /// <summary>
    /// Test 
    /// </summary>
    [Test]
    public void Test ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      using (IDAOTransaction transaction = session.BeginTransaction ()) {
        try {
          var stampingConfigByName = ModelDAOHelper.ModelFactory.CreateStampingConfigByName ("Test");
          stampingConfigByName.Config = new StampingConfig {
            Singletons = new List<StampingService> { new StampingService { 
              Name = "Singleton"
            } }
          };
          ModelDAOHelper.DAOFactory.StampingConfigByNameDAO.MakePersistent (stampingConfigByName);
          ModelDAOHelper.DAOFactory.Flush ();

          var read = ModelDAOHelper.DAOFactory.StampingConfigByNameDAO.FindById (stampingConfigByName.Id);
          Assert.AreEqual (stampingConfigByName.Name, read.Name);
          Assert.AreEqual ("Singleton", stampingConfigByName.Config.Singletons.First ().Name);
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
  }
}
