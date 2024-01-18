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
  /// Unit tests for the class UseGoalConfigHelper
  /// </summary>
  [TestFixture]
  public class UseGoalConfigHelper_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (UseGoalConfigHelper_UnitTest).FullName);

    /// <summary>
    /// Test GetTargetUtilizationPercentage
    /// </summary>
    [Test]
    public void TestGetTargetUtilizationPercentage()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
        IGoal goal = ModelDAOHelper.DAOFactory.GoalDAO
          .FindMatch (GoalTypeId.UtilizationPercentage, null, machine);
        Assert.IsNotNull (goal);
        Assert.That (goal.Value, Is.EqualTo (50.0));
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
