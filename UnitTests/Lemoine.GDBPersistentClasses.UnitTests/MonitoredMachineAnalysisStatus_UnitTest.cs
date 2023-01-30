// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class MonitoredMachineAnalysisStatus.
  /// </summary>
  [TestFixture]
  public class MonitoredMachineAnalysisStatus_UnitTest
  {
    string previousDSNName;
    static readonly ILog log = LogManager.GetLogger(typeof (MonitoredMachineAnalysisStatus_UnitTest).FullName);

    /// <summary>
    /// Test the reload of MonitoredMachineAnalysisStatus
    /// </summary>
    [Test]
    public void TestReload()
    {
      IMonitoredMachineAnalysisStatus v;
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        v = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
          .FindById (1);
      }
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ()) {
        v = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
          .Reload (v);
        Assert.AreEqual (new DateTime (2013, 01, 01, 00, 00, 00), v.ActivityAnalysisDateTime);
        
        using (IDAOTransaction transaction = session.BeginTransaction ()) {
          v.ActivityAnalysisDateTime = DateTime.UtcNow;
          transaction.Rollback ();
        }
        
        v = ModelDAOHelper.DAOFactory.MonitoredMachineAnalysisStatusDAO
          .Reload (v);
        Assert.AreEqual (new DateTime (2013, 01, 01, 00, 00, 00), v.ActivityAnalysisDateTime);
      }
    }

    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
