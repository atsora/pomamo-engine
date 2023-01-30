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
  /// Unit tests for the persistent class Line.
  /// </summary>
  [TestFixture]
  public class Line_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (Line_UnitTest).FullName);
    
    #region Setup and dispose
    string previousDSNName;
    
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
    #endregion // Setup and dispose
    
    /// <summary>
    /// Test if lines can be stored in / removed from the database
    /// </summary>
    [Test]
    public void TestInsertDelete()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ILineDAO lineDAO = daoFactory.LineDAO;
        
        // Retrieve the differents lines stored
        IList<ILine> lines = lineDAO.FindAll();
        int count = lines.Count;
        
        // Create and add a new line
        ILine line = ModelDAOHelper.ModelFactory.CreateLine();
        line.Name = "a name";
        line.Code = "a code";
        daoFactory.LineDAO.MakePersistent(line);
        
        // Check that another element is stored
        Assert.AreEqual(count + 1, lineDAO.FindAll().Count, "Wrong count after insertion");
        
        // Remove the line from the database
        lineDAO.MakeTransient(line);
        
        // Check the number of elements stored
        Assert.AreEqual(count, lineDAO.FindAll().Count, "Wrong count after deletion");
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test if components can be added and removed
    /// </summary>
    [Test]
    public void TestComponentList()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ILineDAO lineDAO = daoFactory.LineDAO;
        IComponentDAO componentDAO = daoFactory.ComponentDAO;
        
        // Create and add a new line with components
        ILine line = ModelDAOHelper.ModelFactory.CreateLine();
        line.Name = "a name";
        line.Code = "a code";
        line.AddComponent(componentDAO.FindAll()[0]);
        line.AddComponent(componentDAO.FindAll()[1]);
        daoFactory.LineDAO.MakePersistent(line);
        
        // Count the number of components stored
        Assert.AreEqual(2, lineDAO.FindById(line.Id).Components.Count,
                        "wrong count of components after insertion");
        
        // Remove one component from the line
        line.RemoveComponent(line.Components.First());
        
        // Count the new number of components stored
        Assert.AreEqual(1, lineDAO.FindById(line.Id).Components.Count,
                        "wrong count of components after deletion");
        
        transaction.Rollback ();
      }
    }
  }
}
