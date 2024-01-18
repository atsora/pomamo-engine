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
        Assert.Multiple (() => {
          Assert.That (slots, Has.Count.EqualTo (1));
          Assert.That (!slots[0].DateTimeRange.Lower.HasValue, Is.True);
          Assert.That (!slots[0].DateTimeRange.Upper.HasValue, Is.True);
        });

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
          Assert.That (newSlots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (!newSlots[i].DateTimeRange.Lower.HasValue, Is.True);
            Assert.That (newSlots[i].DateTimeRange.Upper.HasValue, Is.True);
            Assert.That (newSlots[i].DateTimeRange.Upper.Value, Is.EqualTo (split));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (newSlots[i].DateTimeRange.Lower.HasValue, Is.True);
            Assert.That (!newSlots[i].DateTimeRange.Upper.HasValue, Is.True);
            Assert.That (newSlots[i].DateTimeRange.Lower.Value, Is.EqualTo (split));
          });
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
