// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.GDBPersistentClasses;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.Analysis.UnitTests
{
  /// <summary>
  /// Unit tests for the class MachineStateTemplateAnalysis
  /// </summary>
  [TestFixture]
  public class MachineStateTemplateAnalysis_UnitTest
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateAnalysis_UnitTest).FullName);

    /// <summary>
    /// Documentation of the method Run
    /// </summary>
    [Test]
    public void TestRunInPresent()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3); // Associated to user1
        IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
        IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Attended);
        IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Unattended);
        
        // New machine state template
        IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("Test");
        IMachineStateTemplateItem item1 = mst.AddItem (attendedMOS);
        item1.Shift = shift2;
        IMachineStateTemplateItem weekEnds = mst.AddItem (unattendedMOS);
        weekEnds.WeekDays = DayOfWeek.Saturday.ConvertToWeekDay () | DayOfWeek.Sunday.ConvertToWeekDay ();
        IMachineStateTemplateItem friday = mst.AddItem (unattendedMOS);
        friday.WeekDays = DayOfWeek.Friday.ConvertToWeekDay ();
        friday.TimePeriod = new TimePeriodOfDay ("12:00-00:00");
        IMachineStateTemplateItem vacation = mst.AddItem (unattendedMOS);
        vacation.Day = new DateTime (2014, 10, 08, 00, 00, 00, DateTimeKind.Local);
        vacation.TimePeriod = new TimePeriodOfDay ("06:00-18:00");
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (mst);
        
        // Add a new observation state slot with a machine state template
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, mst, T(1));
          association.User = user1;
          association.Shift = shift3;
          association.DateTime = T(2);
          association.End = T(15);
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
          association.Apply ();
        }
        
        { // Process the templates
          Lemoine.Info.ConfigSet.Load<int> ("Analysis.MachineStateTemplateAnalysis.MaxSlotsByIteration",
                                            2,
                                            true);
          Lemoine.Info.ConfigSet.Load<TimeSpan> ("Analysis.MachineStateTemplateAnalysis.MaxAnalysisTimeRange",
                                                 TimeSpan.FromDays(3),
                                                 true);
          MachineStateTemplateAnalysis analysis =
            new MachineStateTemplateAnalysis (machine1, TransactionLevel.Serializable, null);
          analysis.Run (System.Threading.CancellationToken.None, T (2), T(13), DateTime.UtcNow.AddHours (1));
        }

        // Check the values
        {
          // - ObservationStateSlots
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T(0)));
          Assert.That (slots, Has.Count.EqualTo (10), "Number of observation state slots");
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 03, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift3));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 03, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 06, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift3));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 06, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 18, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 18, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 10, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift3));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 10, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (13)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (null));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift3));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (13)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (15)));
          });
          ++i;
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Documentation of the method Run
    /// </summary>
    [Test]
    public void TestRunInPast()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3); // Associated to user1
        IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
        IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Attended);
        IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Unattended);
        
        // New machine state template
        IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("Test");
        IMachineStateTemplateItem item1 = mst.AddItem (attendedMOS);
        item1.Shift = shift2;
        IMachineStateTemplateItem weekEnds = mst.AddItem (unattendedMOS);
        weekEnds.WeekDays = DayOfWeek.Saturday.ConvertToWeekDay () | DayOfWeek.Sunday.ConvertToWeekDay ();
        IMachineStateTemplateItem friday = mst.AddItem (unattendedMOS);
        friday.WeekDays = DayOfWeek.Friday.ConvertToWeekDay ();
        friday.TimePeriod = new TimePeriodOfDay ("12:00-00:00");
        IMachineStateTemplateItem vacation = mst.AddItem (unattendedMOS);
        vacation.Day = new DateTime (2014, 10, 08, 00, 00, 00, DateTimeKind.Local);
        vacation.TimePeriod = new TimePeriodOfDay ("06:00-18:00");
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (mst);
        
        // Add a new observation state slot with a machine state template
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, mst, T(1));
          association.User = user1;
          association.DateTime = T(2);
          association.End = T(15);
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
          association.Apply ();
        }
        
        { // Process the templates
          Lemoine.Info.ConfigSet.Load<int> ("Analysis.MachineStateTemplateAnalysis.MaxSlotsByIteration",
                                            2,
                                            true);
          Lemoine.Info.ConfigSet.Load<TimeSpan> ("Analysis.MachineStateTemplateAnalysis.MaxAnalysisTimeRange",
                                                 TimeSpan.FromDays(3),
                                                 true);
          MachineStateTemplateAnalysis analysis =
            new MachineStateTemplateAnalysis (machine1, TransactionLevel.Serializable, null);
          analysis.Run (System.Threading.CancellationToken.None, T (200), T(250), DateTime.UtcNow.AddHours (1));
        }

        // Check the values
        {
          // - ObservationStateSlots
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T(0)));
          Assert.That (slots, Has.Count.EqualTo (9), "Number of observation state slots");
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 03, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 03, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 06, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 06, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 18, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 08, 18, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 10, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 10, 12, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (new DateTime (2014, 10, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ()));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (15)));
          });
          ++i;
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the analysis of a machine state template that is defined from an existing one,
    /// in which a machine observation state is replaced by another one
    ///
    /// The base template is attended, except the week-ends that are unattended.
    /// The derived template applies the base template with off instead of unattended.
    /// The template is processed by several iterations of 3 days, so that the replacement
    /// is also checked when the sub template is applied on partial periods
    /// </summary>
    [Test]
    public void TestRunWithReplacedState ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        try {
          // Reference data
          IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
          IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
          IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);
          IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Unattended);
          IMachineObservationState offMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Off);

          // Base machine state template
          IMachineStateTemplate baseTemplate = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestBase");
          IMachineStateTemplateItem attended = baseTemplate.AddItem (attendedMOS);
          attended.Shift = shift2;
          IMachineStateTemplateItem weekEnds = baseTemplate.AddItem (unattendedMOS);
          weekEnds.WeekDays = DayOfWeek.Saturday.ConvertToWeekDay () | DayOfWeek.Sunday.ConvertToWeekDay ();
          ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (baseTemplate);

          // Derived machine state template: off instead of unattended
          IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestOff");
          IMachineStateTemplateItem offItem = mst.AddItem (baseTemplate);
          offItem.ReplacedMachineObservationState = unattendedMOS;
          offItem.MachineObservationState = offMOS;
          ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (mst);

          {
            IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, mst, T (1));
            association.User = user1;
            association.DateTime = T (2);
            association.End = T (15);
            ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
              .MakePersistent (association);
            association.Apply ();
          }

          { // Process the templates
            Lemoine.Info.ConfigSet.ForceValue<int> ("Analysis.MachineStateTemplateAnalysis.MaxSlotsByIteration", 2);
            Lemoine.Info.ConfigSet.ForceValue<TimeSpan> ("Analysis.MachineStateTemplateAnalysis.MaxAnalysisTimeRange",
                                                         TimeSpan.FromDays (3));
            MachineStateTemplateAnalysis analysis =
              new MachineStateTemplateAnalysis (machine1, TransactionLevel.Serializable, null);
            analysis.Run (System.Threading.CancellationToken.None, T (200), T (250), DateTime.UtcNow.AddHours (1));
          }

          // Check the values
          {
            IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
              .FindOverlapsRange (machine1, new UtcDateTimeRange (T (0)));
            Assert.That (slots, Has.Count.EqualTo (7), "Number of observation state slots");
            var saturday1 = new DateTime (2014, 10, 04, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
            var monday1 = new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
            var saturday2 = new DateTime (2014, 10, 11, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
            var monday2 = new DateTime (2014, 10, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
            Assert.Multiple (() => {
              Assert.That (slots[1].MachineObservationState, Is.EqualTo (attendedMOS));
              Assert.That (slots[1].MachineStateTemplate, Is.EqualTo (mst));
              Assert.That (slots[1].User, Is.EqualTo (user1));
              Assert.That (slots[1].Shift, Is.EqualTo (shift2));
              Assert.That (slots[1].BeginDateTime.Value, Is.EqualTo (T (1)));
              Assert.That (slots[1].EndDateTime.Value, Is.EqualTo (saturday1));
              Assert.That (slots[2].MachineObservationState, Is.EqualTo (offMOS));
              Assert.That (slots[2].MachineStateTemplate, Is.EqualTo (mst));
              Assert.That (slots[2].User, Is.EqualTo (user1));
              Assert.That (slots[2].Shift, Is.Null);
              Assert.That (slots[2].EndDateTime.Value, Is.EqualTo (monday1));
              Assert.That (slots[3].MachineObservationState, Is.EqualTo (attendedMOS));
              Assert.That (slots[3].Shift, Is.EqualTo (shift2));
              Assert.That (slots[3].EndDateTime.Value, Is.EqualTo (saturday2));
              Assert.That (slots[4].MachineObservationState, Is.EqualTo (offMOS));
              Assert.That (slots[4].EndDateTime.Value, Is.EqualTo (monday2));
              Assert.That (slots[5].MachineObservationState, Is.EqualTo (attendedMOS));
              Assert.That (slots[5].MachineStateTemplate, Is.EqualTo (mst));
              Assert.That (slots[5].EndDateTime.Value, Is.EqualTo (T (15)));
            });
          }
        }
        finally {
          Lemoine.Info.ConfigSet.RemoveForcedValue ("Analysis.MachineStateTemplateAnalysis.MaxSlotsByIteration");
          Lemoine.Info.ConfigSet.RemoveForcedValue ("Analysis.MachineStateTemplateAnalysis.MaxAnalysisTimeRange");
          transaction.Rollback ();
        }
      }
    }

    DateTime T (int days)
    {
      return new DateTime (2014, 09, 30, 00, 00, 00, DateTimeKind.Utc).AddDays (days);
    }
  }
}
