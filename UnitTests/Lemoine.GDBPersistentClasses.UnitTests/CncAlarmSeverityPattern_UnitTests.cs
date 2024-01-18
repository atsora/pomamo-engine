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
  /// Description of CncAlarmSeverityPattern_UnitTests.
  /// </summary>
  [TestFixture]
  public class CncAlarmSeverityPattern_UnitTests
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncAlarmSeverityPattern_UnitTests).FullName);
    
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
    /// Test if CncAlarmSeverityPattern can be stored in / removed from the database
    /// </summary>
    [Test]
    public void TestInsertDelete()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession())
        using (IDAOTransaction transaction = daoSession.BeginTransaction())
      {
        // Create a severity
        var cAS = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverity("Cnc test", "severity test");
        daoFactory.CncAlarmSeverityDAO.MakePersistent(cAS);
        
        var cASP_DAO = daoFactory.CncAlarmSeverityPatternDAO;
        
        // Retrieve the differents alarms stored
        int count = cASP_DAO.FindAll().Count;
        
        // Create and add a new cnc alarm severity pattern
        var pattern = new CncAlarmSeverityPatternRules ();
        pattern.Number = "123";
        pattern.Properties["severity"] = "high";
        var cASP = ModelDAOHelper.ModelFactory.CreateCncAlarmSeverityPattern("Cnc test", pattern, cAS);
        cASP.Status = EditStatus.DEFAULT_VALUE_DELETED;
        cASP.Name = "foo";
        
        cASP_DAO.MakePersistent(cASP);
        ModelDAOHelper.DAOFactory.Flush();
        
        // Mess the pattern before reloading it
        cASP.Rules.Number = "k";
        cASP.Rules.Properties = null;
        cASP_DAO.Reload(cASP);

        Assert.Multiple (() => {
          // Check that another element is stored
          Assert.That (cASP_DAO.FindAll (), Has.Count.EqualTo (count + 1), "Wrong count after insertion");

          // Check the properties
          Assert.That (cASP.CncInfo, Is.EqualTo ("Cnc test"), "Wrong cnc info");
          Assert.That (cASP.Severity.Id, Is.EqualTo (cAS.Id), "Wrong severity");
          Assert.That (cASP.Rules.Number, Is.EqualTo ("123"), "Wrong pattern 'number'");
          Assert.That (cASP.Rules.Properties["severity"], Is.EqualTo ("high"), "Wrong pattern 'properties -> severity'");
          Assert.That (cASP.Status, Is.EqualTo (EditStatus.DEFAULT_VALUE_DELETED), "wrong Status");
          Assert.That (cASP.Name, Is.EqualTo ("foo"), "wrong Name");
        });

        // Remove the cnc alarm from the database
        cASP_DAO.MakeTransient(cASP);

        // Check the number of elements stored
        Assert.That (cASP_DAO.FindAll(), Has.Count.EqualTo (count), "Wrong count after deletion");
        
        transaction.Rollback();
      }
    }
  }
}
