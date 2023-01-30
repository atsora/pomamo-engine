// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NUnit.Framework;
using Lemoine.Business.Config;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DataStructureOption
  /// </summary>
  [TestFixture]
  public class DataStructureOption_UnitTest
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (DataStructureOption_UnitTest).FullName);

    /// <summary>
    /// Test the method GetOptionValue
    /// </summary>
    [Test]
    public void TestGetOptionValue()
    {
      using (ISession session = NHibernateHelper.OpenSession ())
      {
        bool result = Lemoine.Info.ConfigSet
          .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob));
        Assert.AreEqual (false, result);
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
