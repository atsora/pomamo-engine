// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DayTemplateSlot
  /// </summary>
  [TestFixture]
  public class DayTemplateSlot_UnitTest: Lemoine.UnitTests.WithDayTimeStamp
  {
    private string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (DayTemplateSlot_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DayTemplateSlot_UnitTest ()
      : base (Lemoine.UnitTests.UtcDateTime.From (2015, 04, 30))
    {
    }
    
    /// <summary>
    /// Test insert / read
    /// </summary>
    [Test]
    public void TestRead()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        IList<IDayTemplateSlot> slots = ModelDAOHelper.DAOFactory.DayTemplateSlotDAO
          .FindAll ();
        Assert.AreEqual (1, slots.Count);
        Assert.IsTrue (!slots [0].DateTimeRange.Lower.HasValue);
        Assert.IsTrue (!slots [0].DateTimeRange.Upper.HasValue);
        
        DateTime split = new DateTime (2015, 05, 01, 00, 00, 00, DateTimeKind.Utc);
        
        {
          IDayTemplateSlot newSlot = ModelDAOHelper.ModelFactory
            .CreateDayTemplateSlot (slots[0].DayTemplate, new UtcDateTimeRange (new LowerBound<DateTime> (null), split));
          ModelDAOHelper.DAOFactory.DayTemplateSlotDAO.MakePersistent (newSlot);
        }
        {
          IDayTemplateSlot newSlot = ModelDAOHelper.ModelFactory
            .CreateDayTemplateSlot (slots[0].DayTemplate, new UtcDateTimeRange (split));
          ModelDAOHelper.DAOFactory.DayTemplateSlotDAO.MakePersistent (newSlot);
        }
        ModelDAOHelper.DAOFactory.DayTemplateSlotDAO.MakeTransient (slots[0]);
        ModelDAOHelper.DAOFactory.Flush ();

        {
          IList<IDayTemplateSlot> newSlots = ModelDAOHelper.DAOFactory.DayTemplateSlotDAO
            .FindAll ();
          Assert.AreEqual (2, newSlots.Count);
          int i = 0;
          Assert.IsTrue (!newSlots [i].DateTimeRange.Lower.HasValue);
          Assert.IsTrue (newSlots [i].DateTimeRange.Upper.HasValue);
          Assert.AreEqual (split, newSlots [i].DateTimeRange.Upper.Value);
          ++i;
          Assert.IsTrue (newSlots [i].DateTimeRange.Lower.HasValue);
          Assert.IsTrue (!newSlots [i].DateTimeRange.Upper.HasValue);
          Assert.AreEqual (split, newSlots [i].DateTimeRange.Lower.Value);
        }
        
        transaction.Rollback ();
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
