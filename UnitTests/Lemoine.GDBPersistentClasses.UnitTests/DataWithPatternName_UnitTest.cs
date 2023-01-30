// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Database.Persistent;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DataWithPatternName
  /// </summary>
  [TestFixture]
  public class DataWithPatternName_UnitTest
  {
    private string previousDSNName;

    static readonly ILog log = LogManager.GetLogger(typeof (DataWithPatternName_UnitTest).FullName);

    /// <summary>
    /// Test the Display method
    /// </summary>
    [Test]
    public void TestDisplay()
    {
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IComponent component = ModelDAOHelper.DAOFactory.ComponentDAO.FindById(1);
        Assert.AreEqual ("COMPONENT1 <CAVITY>", component.Display);
        component = ModelDAOHelper.DAOFactory.ComponentDAO.FindById(4);
        Assert.AreEqual ("C3A02-2 <Undefined>", component.Display);
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
