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
          Assert.That (slots, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].ShiftTemplate, Is.EqualTo (template));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        {
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R(1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].ShiftTemplate, Is.EqualTo (template));
            Assert.That (slots[i].TemplateProcessed, Is.False);
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
          });
          i = slots.Count-1;
          Assert.That (slots [i].EndDateTime.HasValue, Is.False);
        }
        
        { // Process the templates
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .GetNotProcessTemplate (R(1), 1).ToList ();
          Assert.That (slots, Has.Count.EqualTo (1));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].ShiftTemplate, Is.EqualTo (template));
            Assert.That (slots[i].TemplateProcessed, Is.False);
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
          });
          ((ShiftSlot)slots [0]).ProcessTemplate (System.Threading.CancellationToken.None, R (1, 2), null, true, null, null);
        }
        
        { // Second check
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R(1, 2));
          Assert.That (slots, Has.Count.EqualTo (4));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].ShiftTemplate, Is.EqualTo (template));
            Assert.That (slots[i].Shift, Is.Null);
          });
          Assert.Multiple (() => {
            Assert.That (slots[i].TemplateProcessed, Is.True);
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (8))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (1).Date));
            Assert.That (slots[i].Breaks, Is.Empty);
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].ShiftTemplate, Is.EqualTo (template));
            Assert.That (slots[i].Shift, Is.EqualTo (shift1));
            Assert.That (slots[i].TemplateProcessed, Is.True);
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (8))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (15))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (1).Date));
            Assert.That (slots[i].Breaks, Has.Count.EqualTo (1));
          });
          foreach (IShiftSlotBreak b in slots [i].Breaks) {
            Assert.Multiple (() => {
              Assert.That (b.Range.Lower.Value, Is.EqualTo (D (1, TimeSpan.FromHours (12))));
              Assert.That (b.Range.Upper.Value, Is.EqualTo (D (1, TimeSpan.FromHours (13))));
            });
          }
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].ShiftTemplate, Is.EqualTo (template));
            Assert.That (slots[i].Shift, Is.Null);
          });
          Assert.Multiple (() => {
            Assert.That (slots[i].TemplateProcessed, Is.True);
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (15))));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (22))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (1).Date));
            Assert.That (slots[i].Breaks, Is.Empty);
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].ShiftTemplate, Is.EqualTo (template));
            Assert.That (slots[i].Shift, Is.Null);
            Assert.That (slots[i].TemplateProcessed, Is.False); // Processed only until D(1, TimeSpan.FromHours (22)
          });
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
          Assert.That (slots, Has.Count.EqualTo (2));
          foreach (IShiftSlot slot in slots) {
            slot.ProcessTemplate (System.Threading.CancellationToken.None, R (1, 2));
          }
        }
        
        { // Check
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R(1, 2));
          Assert.That (slots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.That (slots [i].ShiftTemplate, Is.Null);
          Assert.Multiple (() => {
            Assert.That (slots[i].Shift, Is.EqualTo (shift1));
            Assert.That (slots[i].TemplateProcessed, Is.True);
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (22))));
            Assert.That (slots[i].Day.Value, Is.EqualTo (T (1).Date));
          });
          ++i;
          Assert.That (slots [i].ShiftTemplate, Is.Null);
          Assert.Multiple (() => {
            Assert.That (slots[i].Shift, Is.EqualTo (shift1));
            Assert.That (slots[i].TemplateProcessed, Is.True);
          });
          ++i;
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test ProcessTemplate with a shift template that references two other shift templates,
    /// the second one being restricted to a specific week
    ///
    /// From that week, the whole week is reset before the second template is applied:
    /// only the shifts of the second template remain
    /// </summary>
    [Test]
    public void TestProcessTemplateSubTemplateOnAWeek ()
    {
      CheckProcessTemplateSubTemplate (0);
    }

    /// <summary>
    /// Same as <see cref="TestProcessTemplateSubTemplateOnAWeek"/>, but the second template
    /// is restricted to the next week: it does not apply yet and the shifts of the first
    /// template are kept
    /// </summary>
    [Test]
    public void TestProcessTemplateSubTemplateOnAnotherWeek ()
    {
      CheckProcessTemplateSubTemplate (1);
    }

    /// <summary>
    /// A global shift template references:
    /// <item>a template A that defines a morning shift 08:00-13:00, without any restriction</item>
    /// <item>a template B that defines an afternoon shift 13:00-21:00, from a specific week</item>
    /// </summary>
    /// <param name="weekOffset">0: template B applies on the processed day, 1: only the week after</param>
    void CheckProcessTemplateSubTemplate (int weekOffset)
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IShift morningShift = ModelDAOHelper.DAOFactory.ShiftDAO
          .FindById (1);
        IShift afternoonShift = ModelDAOHelper.DAOFactory.ShiftDAO
          .FindById (2);

        // Template A: morning 08:00-13:00
        IShiftTemplate templateA = ModelDAOHelper.ModelFactory.CreateShiftTemplate ("A");
        IShiftTemplateItem itemA = templateA.AddItem (morningShift);
        itemA.TimePeriod = new TimePeriodOfDay ("08:00-13:00");
        ModelDAOHelper.DAOFactory.ShiftTemplateDAO.MakePersistent (templateA);

        // Template B: afternoon 13:00-21:00
        IShiftTemplate templateB = ModelDAOHelper.ModelFactory.CreateShiftTemplate ("B");
        IShiftTemplateItem itemB = templateB.AddItem (afternoonShift);
        itemB.TimePeriod = new TimePeriodOfDay ("13:00-21:00");
        ModelDAOHelper.DAOFactory.ShiftTemplateDAO.MakePersistent (templateB);

        // Global template: A always, B from the specified week
        WeekNumberHelper.GetWeek (T (1).ToLocalTime ().Date, out var weekYear, out var weekNumber);
        IShiftTemplate template = ModelDAOHelper.ModelFactory.CreateShiftTemplate ("global");
        template.AddItem (templateA);
        IShiftTemplateItem subItemB = template.AddItem (templateB);
        subItemB.WeekYear = weekYear;
        subItemB.WeekNumber = weekNumber + weekOffset;
        subItemB.WeekFrequency = 1;
        ModelDAOHelper.DAOFactory.ShiftTemplateDAO.MakePersistent (template);

        { // Add the change
          IShiftTemplateAssociation change = ModelDAOHelper.ModelFactory
            .CreateShiftTemplateAssociation (template, T (1));
          ModelDAOHelper.DAOFactory.ShiftTemplateAssociationDAO
            .MakePersistent (change);
        }

        { // Run MakeAnalysis
          AnalysisUnitTests.RunMakeAnalysis<ShiftTemplateAssociation> ();
        }
        DAOFactory.EmptyAccumulators ();

        { // Process the templates
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .GetNotProcessTemplate (R (1), 1).ToList ();
          Assert.That (slots, Has.Count.EqualTo (1));
          ((ShiftSlot)slots[0]).ProcessTemplate (System.Threading.CancellationToken.None, R (1, 2), null, true, null, null);
        }

        { // Check: one single shift on the processed day
          IList<IShiftSlot> slots = ModelDAOHelper.DAOFactory.ShiftSlotDAO
            .FindOverlapsRange (R (1, 2));
          IList<IShiftSlot> withShift = slots
            .Where (s => null != s.Shift)
            .ToList ();
          Assert.That (withShift, Has.Count.EqualTo (1));
          if (0 == weekOffset) {
            Assert.Multiple (() => {
              Assert.That (withShift[0].Shift, Is.EqualTo (afternoonShift),
                           "The morning shift of the template A has been reset by the template B");
              Assert.That (withShift[0].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (13))));
              Assert.That (withShift[0].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (21))));
            });
          }
          else {
            Assert.Multiple (() => {
              Assert.That (withShift[0].Shift, Is.EqualTo (morningShift),
                           "The template B does not apply on this week");
              Assert.That (withShift[0].BeginDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (8))));
              Assert.That (withShift[0].EndDateTime.Value, Is.EqualTo (D (1, TimeSpan.FromHours (13))));
            });
          }
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
