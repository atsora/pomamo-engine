// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;
using System.Linq;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class ShiftSlot
  /// </summary>
  [TestFixture]
  public class ShiftSlot_UnitTest
  {
    string m_previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (ObservationStateSlot_UnitTest).FullName);

    /// <summary>
    /// Test ProcessTemplate with AllShifts
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
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO
          .FindById (1);
        
        // New Shift template
        // shift1: Mo-Su 08:00-15:00 - Break: 12:00-13:00
        IShiftTemplate template = ModelDAOHelper.ModelFactory.CreateShiftTemplate ("template");
        IShiftTemplateItem item = template.AddItem (shift1);
        item.TimePeriod = new TimePeriodOfDay ("08:00-15:00");
        IShiftTemplateBreak shiftBreak = template.AddBreak ();
        shiftBreak.TimePeriod = new TimePeriodOfDay ("12:00-13:00");
        ModelDAOHelper.DAOFactory.ShiftTemplateDAO.MakePersistent (template);
        
        { // Add the change
          IShiftTemplateAssociation change = ModelDAOHelper.ModelFactory
            .CreateShiftTemplateAssociation (template, T(1));
          ModelDAOHelper.DAOFactory.ShiftTemplateAssociationDAO
            .MakePersistent (change);
        }
        
        { // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<ShiftTemplateAssociation> ();
        }
        DAOFactory.EmptyAccumulators ();
        
        // First check
        {
          IList<IShiftTemplateSlot> slots = ModelDAOHelper.DAOFactory.ShiftTemplateSlotDAO
            .FindOverlapsRange (R(1));
          Assert.AreEqual (1, slots.Count);
          int i = 0;
          Assert.AreEqual (template, slots [i].ShiftTemplate);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        {
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R(1));
          int i = 0;
          Assert.AreEqual (template, slots [i].ShiftTemplate);
          Assert.IsFalse (slots [i].TemplateProcessed);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          i = slots.Count-1;
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        
        { // Process the templates
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .GetNotProcessTemplate (R(1), 1).ToList ();
          Assert.AreEqual (1, slots.Count);
          int i = 0;
          Assert.AreEqual (template, slots [i].ShiftTemplate);
          Assert.IsFalse (slots [i].TemplateProcessed);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          ((ShiftSlot)slots [0]).ProcessTemplate (System.Threading.CancellationToken.None, R (1, 2), null, true, null, null);
        }
        
        { // Second check
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R(1, 2));
          Assert.AreEqual (4, slots.Count);
          int i = 0;
          Assert.AreEqual (template, slots [i].ShiftTemplate);
          Assert.IsNull (slots [i].Shift);
          Assert.IsTrue (slots [i].TemplateProcessed);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(1, TimeSpan.FromHours(8)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(1).Date, slots [i].Day.Value);
          Assert.AreEqual (0, slots [i].Breaks.Count);
          ++i;
          Assert.AreEqual (template, slots [i].ShiftTemplate);
          Assert.AreEqual (shift1, slots [i].Shift);
          Assert.IsTrue (slots [i].TemplateProcessed);
          Assert.AreEqual (D(1, TimeSpan.FromHours (8)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(1, TimeSpan.FromHours(15)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(1).Date, slots [i].Day.Value);
          Assert.AreEqual (1, slots [i].Breaks.Count);
          foreach (IShiftSlotBreak b in slots [i].Breaks) {
            Assert.AreEqual (D(1, TimeSpan.FromHours (12)), b.Range.Lower.Value);
            Assert.AreEqual (D(1, TimeSpan.FromHours (13)), b.Range.Upper.Value);
          }
          ++i;
          Assert.AreEqual (template, slots [i].ShiftTemplate);
          Assert.IsNull (slots [i].Shift);
          Assert.IsTrue (slots [i].TemplateProcessed);
          Assert.AreEqual (D(1, TimeSpan.FromHours (15)), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(1, TimeSpan.FromHours (22)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(1).Date, slots [i].Day.Value);
          Assert.AreEqual (0, slots [i].Breaks.Count);
          ++i;
          Assert.AreEqual (template, slots [i].ShiftTemplate);
          Assert.IsNull (slots [i].Shift);
          Assert.IsFalse (slots [i].TemplateProcessed); // Processed only until D(1, TimeSpan.FromHours (22)
          ++i;
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test ProcessTemplate with an existing shift and no template
    /// </summary>
    [Test]
    public void TestProcessTemplateWithExistingShift()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO
          .FindById (1);
        
        { // Initialization
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R(1, 2));
          foreach (IShiftSlot slot in slots) {
            slot.Shift = shift1;
            slot.Day = null;
            slot.ShiftTemplate = null;
            slot.TemplateProcessed = false;
            ModelDAOHelper.DAOFactory.ShiftSlotDAO.MakePersistent (slot);
          }
        }
        
        { // Process the templates
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .GetNotProcessTemplate (R(1, 2));
          Assert.AreEqual (2, slots.Count);
          foreach (IShiftSlot slot in slots) {
            slot.ProcessTemplate (System.Threading.CancellationToken.None, R (1, 2));
          }
        }
        
        { // Check
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R(1, 2));
          Assert.AreEqual (2, slots.Count);
          int i = 0;
          Assert.IsNull (slots [i].ShiftTemplate);
          Assert.AreEqual (shift1, slots [i].Shift);
          Assert.IsTrue (slots [i].TemplateProcessed);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (D(1, TimeSpan.FromHours(22)), slots [i].EndDateTime.Value);
          Assert.AreEqual (T(1).Date, slots [i].Day.Value);
          ++i;
          Assert.IsNull (slots [i].ShiftTemplate);
          Assert.AreEqual (shift1, slots [i].Shift);
          Assert.IsTrue (slots [i].TemplateProcessed);
          ++i;
        }
        
        transaction.Rollback ();
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
    /// Day local time
    /// </summary>
    /// <param name="days"></param>
    /// <param name="localTime"></param>
    /// <returns></returns>
    DateTime D (int days, TimeSpan localTime)
    {
      return T (days).ToLocalTime ().Date.Add (localTime).ToUniversalTime ();
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
