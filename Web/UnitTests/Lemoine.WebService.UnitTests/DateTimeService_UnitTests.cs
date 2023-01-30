// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using NUnit.Framework;
//using Lemoine.GDBPersistentClasses;
using Lemoine.ModelDAO;
using Lemoine.Model;
using Lemoine.DTO;
using System.Collections.Generic;
using Lemoine.WebService;

namespace Lemoine.WebService.UnitTests
{
  /// <summary>
  /// Unit tests for Lemoine.WebService.DateTimeService
  /// </summary>
  [TestFixture]
  public class DateTimeService_UnitTests
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DateTimeService_UnitTests).FullName);

    #region TestGetRangeAroundSuccess success
    /// <summary>
    /// Tests GetRangeAroundSuccess service of DateTimeService (successful response)
    /// </summary>
    [Test]
    public void TestGetRangeAroundSuccess()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction())
      {
        try {
          // value exists in DB
          
          // create an input DTO
          GetRangeAround getRangeAround = new GetRangeAround();
          getRangeAround.RangeSize = 1;
          getRangeAround.RangeType = "month";
          getRangeAround.Around    = "2015-09-06T14:59:00Z";
          
          RangeDTO getRangeAroundResponse = new GetRangeAroundService ().GetWithoutCache (getRangeAround) as RangeDTO;
          Assert.IsNotNull(getRangeAroundResponse, "No getRangeAroundResponse");
          
          // Verify
          Assert.AreEqual("2015-08-31T20:00:00Z", getRangeAroundResponse.DateTimeRange.Begin, "rangeAround.Begin is not 2015-08-31T20:00:00Z");
          Assert.AreEqual("2015-09-30T20:00:00Z", getRangeAroundResponse.DateTimeRange.End, "rangeAround.End is not 2015-09-30T20:00:00Z");
          Assert.AreEqual("2015-09-01", getRangeAroundResponse.DayRange.Begin, "rangeAround.Begin is not 2015-09-01");
          Assert.AreEqual("2015-09-30", getRangeAroundResponse.DayRange.End, "rangeAround.End is not 2015-09-30");
          
        } finally {
          transaction.Rollback();
        }
        
        try {
          // value exists in DB
          
          // create an input DTO
          GetRangeAround getRangeAround = new GetRangeAround();
          getRangeAround.RangeSize = 1;
          getRangeAround.RangeType = "quarter";
          getRangeAround.Around    = "2016-05-16T17:00:00Z";
          
          RangeDTO getRangeAroundResponse = new GetRangeAroundService ().GetWithoutCache (getRangeAround) as RangeDTO;
          Assert.IsNotNull(getRangeAroundResponse, "No getRangeAroundResponse");
          
          // Verify
          Assert.AreEqual("2016-03-31T20:00:00Z", getRangeAroundResponse.DateTimeRange.Begin, "rangeAround.Begin is not ");
          Assert.AreEqual("2016-06-30T20:00:00Z", getRangeAroundResponse.DateTimeRange.End, "rangeAround.End is not ");
          Assert.AreEqual("2016-04-01", getRangeAroundResponse.DayRange.Begin, "rangeAround.Begin is not ");
          Assert.AreEqual("2016-06-30", getRangeAroundResponse.DayRange.End, "rangeAround.End is not ");
          
        } finally {
          transaction.Rollback();
        }
        
         try {
          // value exists in DB
          
          // create an input DTO
          GetRangeAround getRangeAround = new GetRangeAround();
          getRangeAround.RangeSize = 2;
          getRangeAround.RangeType = "day";
          getRangeAround.Around    = "2015-09-06T14:59:00Z";
          
          RangeDTO getRangeAroundResponse = new GetRangeAroundService ().GetWithoutCache (getRangeAround) as RangeDTO;
          Assert.IsNotNull(getRangeAroundResponse, "No getRangeAroundResponse");
          
          // Verify
          Assert.AreEqual("2015-09-04T20:00:00Z", getRangeAroundResponse.DateTimeRange.Begin, "rangeAround.Begin is not 2015-09-04T20:00:00Z");
          Assert.AreEqual("2015-09-06T20:00:00Z", getRangeAroundResponse.DateTimeRange.End, "rangeAround.End is not 2015-09-06T20:00:00Z");
          Assert.AreEqual("2015-09-05", getRangeAroundResponse.DayRange.Begin, "rangeAround.Begin is not 2015-09-05");
          Assert.AreEqual("2015-09-06", getRangeAroundResponse.DayRange.End, "rangeAround.End is not 2015-09-06");
          
        } finally {
          transaction.Rollback();
        }
      }
    }
    #endregion
    
    [OneTimeSetUp]
    public void Init()
    {
    }

    [OneTimeTearDown]
    public void Dispose()
    {
    }
  }
}
