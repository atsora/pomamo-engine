// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class DaySlot
  /// </summary>
  [TestFixture]
  public class DaySlot_UnitTest
  {
    string m_previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (ObservationStateSlot_UnitTest).FullName);

    /// <summary>
    /// Test ProcessTemplate with AllDays
    /// </summary>
    [Test]
    public void TestProcessTemplateAllDays()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        
        // New day template
        IDayTemplate template = ModelDAOHelper.ModelFactory.CreateDayTemplate ();
        template.AddItem (TimeSpan.FromHours (5), WeekDay.AllDays);
        ModelDAOHelper.DAOFactory.DayTemplateDAO.MakePersistent (template);
        
        { // Add the change
          IDayTemplateChange change = ModelDAOHelper.ModelFactory
            .CreateDayTemplateChange (template, T(1));
          ModelDAOHelper.DAOFactory.DayTemplateChangeDAO
            .MakePersistent (change);
        }
        
        { // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<DayTemplateChange> ();
        }
        DAOFactory.EmptyAccumulators ();
        
        // First check
        {
          IList<IDayTemplateSlot> slots = ModelDAOHelper.DAOFactory.DayTemplateSlotDAO
            .FindOverlapsRange (R(1));
          Assert.AreEqual (2, slots.Count);
          int i = 0;
          Assert.AreEqual (1, slots [i].DayTemplate.Id);
          Assert.AreEqual (D(1, TimeSpan.FromHours (5)), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(1, TimeSpan.FromHours (5)), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        {
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.AreEqual (2, slots.Count);
          int i = 0;
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.IsFalse (slots [i].Day.HasValue);
          Assert.AreEqual (D(1, TimeSpan.FromHours (5)), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        
        { // Process the templates
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetNotProcessTemplate (R(1), 1);
          Assert.AreEqual (1, slots.Count);
          ((DaySlot)slots [0]).ProcessTemplate (System.Threading.CancellationToken.None, R(1, 2), null, true, null, null);
        }
        
        { // Second check
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.AreEqual (5, slots.Count);
          int i = 0;
          Assert.AreEqual (1, slots [i].DayTemplate.Id);
          Assert.AreEqual (D(-1, TimeSpan.FromHours (22)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(1, TimeSpan.FromHours(5)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(0).Date, slots [i].Day.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(1, TimeSpan.FromHours(5)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(2, TimeSpan.FromHours(5)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(1).Date, slots [i].Day.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(2, TimeSpan.FromHours(5)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(3, TimeSpan.FromHours(5)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(2).Date, slots [i].Day.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(3, TimeSpan.FromHours(5)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(4, TimeSpan.FromHours(5)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(3).Date, slots [i].Day.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(4, TimeSpan.FromHours(5)), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
          Assert.IsFalse (slots [i].Day.HasValue);
          ++i;
        }
        
        // TODO: check the shifts too
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test ProcessTemplate with AllDays with a negative cut-off
    /// where a day is extended
    /// </summary>
    [Test]
    public void TestProcessTemplateAllDaysWithNegativeCutOff()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        
        // New day template
        IDayTemplate template = ModelDAOHelper.ModelFactory.CreateDayTemplate ();
        template.AddItem (TimeSpan.FromHours (23), WeekDay.AllDays);
        ModelDAOHelper.DAOFactory.DayTemplateDAO.MakePersistent (template);
        
        { // Add the change
          IDayTemplateChange change = ModelDAOHelper.ModelFactory
            .CreateDayTemplateChange (template, T(1));
          ModelDAOHelper.DAOFactory.DayTemplateChangeDAO
            .MakePersistent (change);
        }
        
        { // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<DayTemplateChange> ();
        }
        DAOFactory.EmptyAccumulators ();
        
        // First check
        {
          IList<IDayTemplateSlot> slots = ModelDAOHelper.DAOFactory.DayTemplateSlotDAO
            .FindOverlapsRange (R(1));
          Assert.AreEqual (2, slots.Count);
          int i = 0;
          Assert.AreEqual (1, slots [i].DayTemplate.Id);
          Assert.AreEqual (D(1, TimeSpan.FromHours (23)), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(1, TimeSpan.FromHours (23)), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        {
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.AreEqual (2, slots.Count);
          int i = 0;
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.IsFalse (slots [i].Day.HasValue);
          Assert.AreEqual (D(1, TimeSpan.FromHours (23)), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        
        { // Process the templates
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetNotProcessTemplate (R(1), 1);
          Assert.AreEqual (1, slots.Count);
          ((DaySlot)slots [0]).ProcessTemplate (System.Threading.CancellationToken.None, R (1, 2), null, true, null, null);
        }
        
        { // Second check
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.AreEqual (4, slots.Count);
          int i = 0;
          Assert.AreEqual (1, slots [i].DayTemplate.Id);
          Assert.AreEqual (D(0, TimeSpan.FromHours (22)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(1, TimeSpan.FromHours(23)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(1).Date, slots [i].Day.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(1, TimeSpan.FromHours(23)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(2, TimeSpan.FromHours(23)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(2).Date, slots [i].Day.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(2, TimeSpan.FromHours(23)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(3, TimeSpan.FromHours(23)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(3).Date, slots [i].Day.Value);
          ++i;
          Assert.AreEqual (template, slots [i].DayTemplate);
          Assert.AreEqual (D(3, TimeSpan.FromHours(23)), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
          Assert.IsFalse (slots [i].Day.HasValue);
          ++i;
        }
        
        // TODO: check the shifts too
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the GetLiveAt method
    /// </summary>
    [Test]
    public void TestGetLiveAt ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ())
        {
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAt (new DateTime (2017, 05, 05, 23, 00, 00, DateTimeKind.Local));
          Assert.AreEqual (new DateTime (2017, 05, 06), daySlot.Day);
          Assert.AreEqual (new DateTime (2017, 05, 05, 22, 00, 00, DateTimeKind.Local),
                           daySlot.BeginDateTime.Value.ToLocalTime ());
          Assert.AreEqual (new DateTime (2017, 05, 06, 22, 00, 00, DateTimeKind.Local),
                           daySlot.EndDateTime.Value.ToLocalTime ());
        }        

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ())
        {
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAt (new DateTime (2017, 05, 05, 02, 00, 00, DateTimeKind.Local));
          Assert.AreEqual (new DateTime (2017, 05, 05), daySlot.Day);
          Assert.AreEqual (new DateTime (2017, 05, 04, 22, 00, 00, DateTimeKind.Local),
                           daySlot.BeginDateTime.Value.ToLocalTime ());
          Assert.AreEqual (new DateTime (2017, 05, 05, 22, 00, 00, DateTimeKind.Local),
                           daySlot.EndDateTime.Value.ToLocalTime ());
        }
      }
    }

    /// <summary>
    /// Test the GetLiveDay method
    /// </summary>
    [Test]
    public void TestGetLiveDay ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ())
        {
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedByDay (new DateTime (2017, 05, 05));
          Assert.AreEqual (new DateTime (2017, 05, 05), daySlot.Day);
          Assert.AreEqual (new DateTime (2017, 05, 04, 22, 00, 00, DateTimeKind.Local),
                           daySlot.BeginDateTime.Value.ToLocalTime ());
          Assert.AreEqual (new DateTime (2017, 05, 05, 22, 00, 00, DateTimeKind.Local),
                           daySlot.EndDateTime.Value.ToLocalTime ());
        }
      }
    }

    DateTime T (int days)
    {
      return new DateTime (2014, 09, 30, 00, 00, 00, DateTimeKind.Utc).AddDays (days);
    }
    
    UtcDateTimeRange R (int begin, int end)
    {
      return new UtcDateTimeRange (T(begin), T(end));
    }
    
    UtcDateTimeRange R (int begin)
    {
      return new UtcDateTimeRange (T(begin));
    }
    
    /// <summary>
    /// Day begin
    /// </summary>
    /// <param name="days"></param>
    /// <param name="cutOff"></param>
    /// <returns></returns>
    DateTime D (int days, TimeSpan cutOff)
    {
      return T (days).ToLocalTime ().Date.Add (cutOff).ToUniversalTime ();
    }

    [OneTimeSetUp]
    public void Init()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 "LemoineUnitTests");
      ModelDAOHelper.ModelFactory =
        new GDBPersistentClassFactory ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      if (m_previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   m_previousDSNName);
      }
    }
  }
}
