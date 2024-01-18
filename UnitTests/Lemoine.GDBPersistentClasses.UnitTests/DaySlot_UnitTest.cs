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
          Assert.That (slots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate.Id, Is.EqualTo (1));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (5))));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (5))));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        {
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.That (slots, Has.Count.EqualTo (2));
          int i = 0;
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].Day.HasValue, Is.False);
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (5))));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        
        { // Process the templates
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetNotProcessTemplate (R(1), 1);
          Assert.That (slots, Has.Count.EqualTo (1));
          ((DaySlot)slots [0]).ProcessTemplate (System.Threading.CancellationToken.None, R(1, 2), null, true, null, null);
        }
        
        { // Second check
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.That (slots, Has.Count.EqualTo (5));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate.Id, Is.EqualTo (1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (-1, TimeSpan.FromHours (22))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (5))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (0).Date));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (5))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (2, TimeSpan.FromHours (5))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (1).Date));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (2, TimeSpan.FromHours (5))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (3, TimeSpan.FromHours (5))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (2).Date));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (3, TimeSpan.FromHours (5))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (4, TimeSpan.FromHours (5))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (3).Date));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (4, TimeSpan.FromHours (5))));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
            Assert.That (slots[i].Day.HasValue, Is.False);
          });
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
          Assert.That (slots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate.Id, Is.EqualTo (1));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (23))));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (23))));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        {
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.That (slots, Has.Count.EqualTo (2));
          int i = 0;
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].Day.HasValue, Is.False);
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (23))));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        
        { // Process the templates
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .GetNotProcessTemplate (R(1), 1);
          Assert.That (slots, Has.Count.EqualTo (1));
          ((DaySlot)slots [0]).ProcessTemplate (System.Threading.CancellationToken.None, R (1, 2), null, true, null, null);
        }
        
        { // Second check
          IList<IDaySlot> slots = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindOverlapsRange (R(1));
          Assert.That (slots, Has.Count.EqualTo (4));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate.Id, Is.EqualTo (1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (0, TimeSpan.FromHours (22))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (23))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (1).Date));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (23))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (2, TimeSpan.FromHours (23))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (2).Date));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (2, TimeSpan.FromHours (23))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (3, TimeSpan.FromHours (23))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (3).Date));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].DayTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (3, TimeSpan.FromHours (23))));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
            Assert.That (slots[i].Day.HasValue, Is.False);
          });
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
          Assert.Multiple (() => {
            Assert.That (daySlot.Day, Is.EqualTo (new DateTime (2017, 05, 06)));
            Assert.That (daySlot.BeginDateTime.Value.ToLocalTime (), Is.EqualTo (new DateTime (2017, 05, 05, 22, 00, 00, DateTimeKind.Local)));
            Assert.That (daySlot.EndDateTime.Value.ToLocalTime (), Is.EqualTo (new DateTime (2017, 05, 06, 22, 00, 00, DateTimeKind.Local)));
          });
        }        

        using (IDAOTransaction transaction = session.BeginReadOnlyTransaction ())
        {
          IDaySlot daySlot = ModelDAOHelper.DAOFactory.DaySlotDAO
            .FindProcessedAt (new DateTime (2017, 05, 05, 02, 00, 00, DateTimeKind.Local));
          Assert.Multiple (() => {
            Assert.That (daySlot.Day, Is.EqualTo (new DateTime (2017, 05, 05)));
            Assert.That (daySlot.BeginDateTime.Value.ToLocalTime (), Is.EqualTo (new DateTime (2017, 05, 04, 22, 00, 00, DateTimeKind.Local)));
            Assert.That (daySlot.EndDateTime.Value.ToLocalTime (), Is.EqualTo (new DateTime (2017, 05, 05, 22, 00, 00, DateTimeKind.Local)));
          });
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
          Assert.Multiple (() => {
            Assert.That (daySlot.Day, Is.EqualTo (new DateTime (2017, 05, 05)));
            Assert.That (daySlot.BeginDateTime.Value.ToLocalTime (), Is.EqualTo (new DateTime (2017, 05, 04, 22, 00, 00, DateTimeKind.Local)));
            Assert.That (daySlot.EndDateTime.Value.ToLocalTime (), Is.EqualTo (new DateTime (2017, 05, 05, 22, 00, 00, DateTimeKind.Local)));
          });
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
