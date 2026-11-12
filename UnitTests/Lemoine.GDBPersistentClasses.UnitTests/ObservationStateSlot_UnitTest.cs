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
  /// Unit tests for the class ObservationStateSlot
  /// </summary>
  [TestFixture]
  public class ObservationStateSlot_UnitTest
  {
    string m_previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (ObservationStateSlot_UnitTest).FullName);

    /// <summary>
    /// Test ProcessTemplate with a WeekDay
    /// </summary>
    [Test]
    public void TestProcessTemplateWeekDay()
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
        }
        
        // Run MakeAnalysis
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (NHibernateHelper.GetCurrentSession ());
        }
        DAOFactory.EmptyAccumulators ();
        
        { // Process the templates
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T(0)));
          Assert.That (slots, Has.Count.EqualTo (3), "Number of observation state slots");
          ((ObservationStateSlot)slots [1]).ProcessTemplate (System.Threading.CancellationToken.None, new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                                   T(13)),
                                                             null, false, null, null);
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
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (13)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (null));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (13)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (15)));
          });
          ++i;
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test ProcessTemplate with a long period
    /// </summary>
    [Test]
    public void TestProcessTemplateLongPeriod()
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
        IMachineStateTemplate unattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Unattended);
        
        // New machine state template
        IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("Test");
        IMachineStateTemplateItem item1 = mst.AddItem (attendedMOS);
        item1.Shift = shift2;
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (mst);
        
        // Add a new observation state slot with a machine state template
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, mst, T(1));
          association.User = user1;
          association.DateTime = T(2);
          association.End = T(20);
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
        }
        
        // Run MakeAnalysis
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (NHibernateHelper.GetCurrentSession ());
        }
        DAOFactory.EmptyAccumulators ();
        
        { // Process the templates
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T(0)));
          Assert.That (slots, Has.Count.EqualTo (3), "Number of observation state slots");
          ((ObservationStateSlot)slots [1]).ProcessTemplate (System.Threading.CancellationToken.None, new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                                   T(25)),
                                                             null, false, null, null);
        }

        // Check the values
        {
          // - ObservationStateSlots
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T(0)));
          Assert.That (slots, Has.Count.EqualTo (3), "Number of observation state slots");
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (20)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (unattended));
            Assert.That (slots[i].User, Is.EqualTo (null));
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (20)));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
          ++i;
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test ProcessTemplate with a sub machine state template in which a machine observation state is replaced,
    /// and with a chained replacement
    /// </summary>
    [Test]
    public void TestProcessTemplateReplacedState ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
        IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Attended);
        IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Unattended);
        IMachineObservationState offMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Off);

        // Base machine state template: attended, except the week-ends that are unattended
        IMachineStateTemplate baseTemplate = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestBase");
        baseTemplate.AddItem (attendedMOS);
        IMachineStateTemplateItem weekEnds = baseTemplate.AddItem (unattendedMOS);
        weekEnds.WeekDays = DayOfWeek.Saturday.ConvertToWeekDay () | DayOfWeek.Sunday.ConvertToWeekDay ();
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (baseTemplate);

        // Same as the base template, but the machine is off instead of unattended
        IMachineStateTemplate offTemplate = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestOff");
        IMachineStateTemplateItem offItem = offTemplate.AddItem (baseTemplate);
        offItem.ReplacedMachineObservationState = unattendedMOS;
        offItem.MachineObservationState = offMOS;
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (offTemplate);

        // Chained replacement: attended is replaced by unattended,
        // the replacement unattended => off of offTemplate must not apply to it
        IMachineStateTemplate chainedTemplate = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestChained");
        IMachineStateTemplateItem chainedItem = chainedTemplate.AddItem (offTemplate);
        chainedItem.ReplacedMachineObservationState = attendedMOS;
        chainedItem.MachineObservationState = unattendedMOS;
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (chainedTemplate);

        CheckReplacement (machine1, user1, offTemplate, attendedMOS, offMOS);
        CheckReplacement (machine1, user1, chainedTemplate, unattendedMOS, offMOS);

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test ProcessTemplate with a sub machine state template that is applied every two weeks,
    /// with a replaced machine observation state
    ///
    /// The root template is attended, and every two weeks from the ISO week 40 of 2014
    /// (2014-09-29 to 2014-10-05), the week-ends of the sub template are applied,
    /// the unattended machine observation state of the sub template being replaced by off.
    /// The week-ends of the weeks 40 and 42 are then off, and the ones of the weeks 41 and 43 attended
    /// </summary>
    [Test]
    public void TestProcessTemplateSubTemplateEveryTwoWeeks ()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
      using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
        IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Attended);
        IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Unattended);
        IMachineObservationState offMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int)MachineObservationStateId.Off);

        // Sub machine state template: unattended during the week-ends only
        IMachineStateTemplate weekEndTemplate = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestWeekEnd");
        IMachineStateTemplateItem weekEnds = weekEndTemplate.AddItem (unattendedMOS);
        weekEnds.WeekDays = DayOfWeek.Saturday.ConvertToWeekDay () | DayOfWeek.Sunday.ConvertToWeekDay ();
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (weekEndTemplate);

        // Root machine state template: attended, and the sub template every two weeks with off instead of unattended
        IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestEveryTwoWeeks");
        mst.AddItem (attendedMOS);
        IMachineStateTemplateItem everyTwoWeeks = mst.AddItem (weekEndTemplate);
        everyTwoWeeks.WeekYear = 2014;
        everyTwoWeeks.WeekNumber = 40;
        everyTwoWeeks.WeekFrequency = 2;
        everyTwoWeeks.ReplacedMachineObservationState = unattendedMOS;
        everyTwoWeeks.MachineObservationState = offMOS;
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (mst);

        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, mst, T (1));
          association.User = user1;
          association.DateTime = T (2);
          association.End = T (30);
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
        }
        AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (NHibernateHelper.GetCurrentSession ());
        DAOFactory.EmptyAccumulators ();

        { // Process the template
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T (0)));
          Assert.That (slots, Has.Count.EqualTo (3), "Number of observation state slots");
          // Note: T(26) is in the week-end of the week 43, whatever the time zone is
          ((ObservationStateSlot)slots[1]).ProcessTemplate (System.Threading.CancellationToken.None,
                                                             new UtcDateTimeRange (new LowerBound<DateTime> (null), T (26)),
                                                             null, false, null, null);
        }

        {
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T (0)));
          Assert.That (slots, Has.Count.EqualTo (8), "Number of observation state slots");
          var saturdayWeek40 = new DateTime (2014, 10, 04, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
          var mondayWeek41 = new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
          var saturdayWeek42 = new DateTime (2014, 10, 18, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
          var mondayWeek43 = new DateTime (2014, 10, 20, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
          Assert.Multiple (() => {
            // Week 40 (applicable): off during the week-end
            Assert.That (slots[1].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[1].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[1].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[1].EndDateTime.Value, Is.EqualTo (saturdayWeek40));
            Assert.That (slots[2].MachineObservationState, Is.EqualTo (offMOS));
            Assert.That (slots[2].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[2].EndDateTime.Value, Is.EqualTo (mondayWeek41));
            // Week 41 (not applicable), including its week-end, and week 42 until its week-end: attended
            Assert.That (slots[3].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[3].EndDateTime.Value, Is.EqualTo (saturdayWeek42));
            // Week 42 (applicable): off during the week-end
            Assert.That (slots[4].MachineObservationState, Is.EqualTo (offMOS));
            Assert.That (slots[4].EndDateTime.Value, Is.EqualTo (mondayWeek43));
            // Week 43 (not applicable): attended, including its week-end
            Assert.That (slots[5].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[5].EndDateTime.Value, Is.EqualTo (T (26)));
            // Not processed yet
            Assert.That (slots[6].MachineObservationState, Is.Null);
            Assert.That (slots[6].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[6].EndDateTime.Value, Is.EqualTo (T (30)));
          });
        }

        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Apply a machine state template that is derived from the base template of
    /// <see cref="TestProcessTemplateReplacedState"/> and check the resulting machine observation states
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="user"></param>
    /// <param name="mst">machine state template to apply</param>
    /// <param name="weekMOS">expected machine observation state during the week</param>
    /// <param name="weekEndMOS">expected machine observation state during the week-end</param>
    void CheckReplacement (IMonitoredMachine machine, IUser user, IMachineStateTemplate mst,
                           IMachineObservationState weekMOS, IMachineObservationState weekEndMOS)
    {
      {
        IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
          .CreateMachineStateTemplateAssociation (machine, mst, T (1));
        association.User = user;
        association.DateTime = T (2);
        association.End = T (15);
        ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
          .MakePersistent (association);
      }
      AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (NHibernateHelper.GetCurrentSession ());
      DAOFactory.EmptyAccumulators ();

      { // Process the template
        IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindOverlapsRange (machine, new UtcDateTimeRange (T (0)));
        Assert.That (slots, Has.Count.EqualTo (3), "Number of observation state slots");
        // Note: T(12) is in a week-end, whatever the time zone is
        ((ObservationStateSlot)slots[1]).ProcessTemplate (System.Threading.CancellationToken.None,
                                                           new UtcDateTimeRange (new LowerBound<DateTime> (null), T (12)),
                                                           null, false, null, null);
      }

      {
        IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
          .FindOverlapsRange (machine, new UtcDateTimeRange (T (0)));
        Assert.That (slots, Has.Count.EqualTo (7), "Number of observation state slots");
        var saturday1 = new DateTime (2014, 10, 04, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
        var monday = new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
        var saturday2 = new DateTime (2014, 10, 11, 00, 00, 00, DateTimeKind.Local).ToUniversalTime ();
        Assert.Multiple (() => {
          Assert.That (slots[1].MachineObservationState, Is.EqualTo (weekMOS));
          Assert.That (slots[1].MachineStateTemplate, Is.EqualTo (mst));
          Assert.That (slots[1].BeginDateTime.Value, Is.EqualTo (T (1)));
          Assert.That (slots[1].EndDateTime.Value, Is.EqualTo (saturday1));
          Assert.That (slots[2].MachineObservationState, Is.EqualTo (weekEndMOS));
          Assert.That (slots[2].MachineStateTemplate, Is.EqualTo (mst));
          Assert.That (slots[2].EndDateTime.Value, Is.EqualTo (monday));
          Assert.That (slots[3].MachineObservationState, Is.EqualTo (weekMOS));
          Assert.That (slots[3].EndDateTime.Value, Is.EqualTo (saturday2));
          Assert.That (slots[4].MachineObservationState, Is.EqualTo (weekEndMOS));
          Assert.That (slots[4].EndDateTime.Value, Is.EqualTo (T (12)));
          Assert.That (slots[5].MachineObservationState, Is.Null);
          Assert.That (slots[5].MachineStateTemplate, Is.EqualTo (mst));
          Assert.That (slots[5].EndDateTime.Value, Is.EqualTo (T (15)));
        });
      }
    }

    DateTime T (int days)
    {
      return new DateTime (2014, 09, 30, 00, 00, 00, DateTimeKind.Utc).AddDays (days);
    }
    
    [OneTimeSetUp]
    public void Init()
    {
      m_previousDSNName = System.Environment.GetEnvironmentVariable ("DefaultDSNName");
      System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                 Lemoine.Info.Constants.DEFAULT_DSN_UNIT_TEST_NAME);
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
