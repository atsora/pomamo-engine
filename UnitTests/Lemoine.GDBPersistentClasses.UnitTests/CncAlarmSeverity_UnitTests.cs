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
  /// Description of CncAlarmSeverity_UnitTests.
  /// </summary>
  [TestFixture]
  public class CncAlarmSeverity_UnitTests
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncAlarmSeverity_UnitTests).FullName);
    
    #region Setup and dispose
    string previousDSNName;
    
    [OneTimeSetUp]
    public void Init()
    {
      previousDSNName = System.Environment.GetEnvironmentVariable("DefaultDSNName");
      System.Environment.SetEnvironmentVariable("DefaultDSNName", "LemoineUnitTests");
      ModelDAOHelper.ModelFactory = new GDBPersistentClassFactory();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName", previousDSNName);
      }
    }
    #endregion // Setup and dispose
    
    /// <summary>
    /// Test if CncAlarmSeverities can be stored in / removed from the database
    /// </summary>
    [Test]
    public void TestInsertDelete()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession())
        using (IDAOTransaction transaction = daoSession.BeginTransaction())
      {
        var cAS_DAO = daoFactory.CncAlarmSeverityDAO;
        
        // Retrieve the differents alarms stored
        int count = cAS_DAO.FindAll().Count;
        
        // Create and add a new cnc alarm
        var cAS = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity("Cnc test", "severity test");
        cAS.Color = "#123456";
        cAS.Description = "desc";
        cAS.StopStatus = CncAlarmStopStatus.Possibly;
        cAS.Status = EditStatus.DEFAULT_VALUE;
        cAS.Focus = true;
        
        cAS_DAO.MakePersistent(cAS);
        ModelDAOHelper.DAOFactory.Flush();
        cAS_DAO.Reload(cAS);
        
        // Check that another element is stored
        Assert.AreEqual(count + 1, cAS_DAO.FindAll().Count, "Wrong count after insertion");
        
        // Check the properties
        Assert.AreEqual("Cnc test", cAS.CncInfo, "wrong CncInfo");
        Assert.AreEqual("severity test", cAS.Name, "wrong Name");
        Assert.AreEqual("#123456", cAS.Color, "wrong Color");
        Assert.AreEqual("desc", cAS.Description, "wrong Description");
        Assert.AreEqual(CncAlarmStopStatus.Possibly, cAS.StopStatus, "wrong StopStatus");
        Assert.AreEqual(true, cAS.Focus, "wrong Focus");
        Assert.AreEqual(EditStatus.DEFAULT_VALUE, cAS.Status, "wrong Status");
        
        // Remove the cnc alarm from the database
        cAS_DAO.MakeTransient(cAS);
        
        // Check the number of elements stored
        Assert.AreEqual(count, cAS_DAO.FindAll().Count, "Wrong count after deletion");
        
        transaction.Rollback();
      }
    }
  }
}
