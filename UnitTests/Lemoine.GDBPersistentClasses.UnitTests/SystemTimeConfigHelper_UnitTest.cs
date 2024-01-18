// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class SysteTimeConfigHelper
  /// </summary>
  [TestFixture]
  public class SystemTimeConfigHelper_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (SystemTimeConfigHelper_UnitTest).FullName);

    /// <summary>
    /// Test GetDay
    /// </summary>
    [Test]
    public void TestGetDay()
    {
      {
        DateTime dateTime = new DateTime (2011, 08, 23, 05, 00, 00, DateTimeKind.Local);
        DateTime day = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay (dateTime);
        DateTime expectedDay = new DateTime (2011, 08, 23);
        Assert.That (day, Is.EqualTo (expectedDay));
      }
      
      {
        DateTime dateTime = new DateTime (2011, 08, 23, 23, 00, 00, DateTimeKind.Local);
        DateTime day = ModelDAOHelper.DAOFactory.TimeConfigDAO.GetDay (dateTime);
        DateTime expectedDay = new DateTime (2011, 08, 24);
        Assert.That (day, Is.EqualTo (expectedDay));
      }
    }
  }
}
