// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class Tool
  /// </summary>
  [TestFixture]
  public class Tool_UnitTest
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (Tool_UnitTest).FullName);

    /// <summary>
    /// Test the select with quote
    /// </summary>
    [Test]
    public void TestSelectQuote()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IList<ITool> tools = ModelDAOHelper.DAOFactory.ToolDAO
          .FindByName (".532 x .866 x15*  C'BORE");
        Assert.AreEqual (1, tools.Count);
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
