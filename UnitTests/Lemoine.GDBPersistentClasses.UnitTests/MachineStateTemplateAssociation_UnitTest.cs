// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Linq;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using Lemoine.Extensions.Business.DynamicTimes;
using NHibernate;
using NHibernate.Criterion;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{
  /// <summary>
  /// Unit tests for the class MachineStateTemplateAssociation
  /// </summary>
  [TestFixture]
  public class MachineStateTemplateAssociation_UnitTest: WithDayTimeStamp
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateAssociation_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineStateTemplateAssociation_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestMakeAnalysis()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
        IMachineStateTemplate attended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Attended);
        IMachineStateTemplate unattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Unattended);
        IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Attended);
        IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Unattended);
        IReason reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (2);
        IReason reasonShort = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (3);
        IReason reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (4);
        IReason reasonUnattended = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (5);
        IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (1);
        IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (2);
        IMachineMode auto = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (3);
        
        // Existing ObservationStateSlot
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, attendedMOS, UtcDateTime.From (2011, 08, 01));
          association.End = UtcDateTime.From (2011, 08, 03);
          association.User = user1;
          association.Shift = shift1;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, unattendedMOS, UtcDateTime.From (2011, 08, 03));
          association.End = UtcDateTime.From (2011, 08, 05);
          association.User = null;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, attendedMOS, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05)));
          association.User = user1;
          association.Shift = shift2;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        // Run MakeAnalysis to initialize the MachineObservationStates
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (NHibernateHelper.GetCurrentSession ());
        }
        // Existing ReasonSlot
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine1,
                            R(1, 3));
          existingSlot.MachineMode = active;
          existingSlot.MachineObservationState = attendedMOS;
          existingSlot.SetDefaultReason (reasonMotion, 10.0, false, true);
          session.Save (existingSlot);
        }
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine1,
                            R(3, 5));
          existingSlot.MachineMode = inactive;
          existingSlot.MachineObservationState = unattendedMOS;
          existingSlot.SetDefaultReason (reasonUnattended, 10.0, false, true);
          session.Save (existingSlot);
        }
        {
          ReasonSlot existingSlot =
            new ReasonSlot (machine1,
                            new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05),
                                                  UtcDateTime.From (2011, 08, 05, 00, 01, 00)));
          existingSlot.MachineMode = inactive;
          existingSlot.MachineObservationState = attendedMOS;
          existingSlot.Shift = shift2;
          existingSlot.SetDefaultReason (reasonShort, 10.0, false, false);
          session.Save (existingSlot);
        }
        // Existing MachineActivitySummary
        {
          IMachineActivitySummary summary;
          summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                              UtcDateTime.From (2011, 08, 01),
                                                                              attendedMOS, active);
          summary.Time = TimeSpan.FromHours (20); // Note: cut-off of time is at 20:00 UTC, 22:00 Local
          ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent(summary);
          summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                              UtcDateTime.From (2011, 08, 02),
                                                                              attendedMOS, active);
          summary.Time = TimeSpan.FromDays (1);
          ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent(summary);
          summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                              UtcDateTime.From (2011, 08, 03),
                                                                              attendedMOS, active);
          summary.Time = TimeSpan.FromHours (4);
          ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent(summary);
          summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                              UtcDateTime.From (2011, 08, 03),
                                                                              unattendedMOS, inactive);
          summary.Time = TimeSpan.FromHours (20);
          ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent(summary);
          summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                              UtcDateTime.From (2011, 08, 04),
                                                                              unattendedMOS, inactive);
          summary.Time = TimeSpan.FromDays (1);
          ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent(summary);
          summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                              UtcDateTime.From (2011, 08, 05),
                                                                              unattendedMOS, inactive);
          summary.Time = TimeSpan.FromHours (4);
          ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent(summary);
          summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                              UtcDateTime.From (2011, 08, 05),
                                                                              attendedMOS, inactive, shift2);
          summary.Time = TimeSpan.FromMinutes (1);
          ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent(summary);
        }
        // Existing ReasonSummary
        {
          ReasonSummary summary;
          summary =  new ReasonSummary (machine1,
                                        UtcDateTime.From (2011, 08, 01), null,
                                        attendedMOS, reasonMotion);
          summary.Time = TimeSpan.FromHours (20);
          summary.Number = 1;
          session.Save (summary);
          summary =  new ReasonSummary (machine1,
                                        UtcDateTime.From (2011, 08, 02), null,
                                        attendedMOS, reasonMotion);
          summary.Time = TimeSpan.FromDays (1);
          summary.Number = 1;
          session.Save (summary);
          summary =  new ReasonSummary (machine1,
                                        UtcDateTime.From (2011, 08, 03), null,
                                        attendedMOS, reasonMotion);
          summary.Time = TimeSpan.FromHours (4);
          summary.Number = 1;
          session.Save (summary);
          summary =  new ReasonSummary (machine1,
                                        UtcDateTime.From (2011, 08, 03), null,
                                        unattendedMOS, reasonUnattended);
          summary.Time = TimeSpan.FromHours (20);
          summary.Number = 1;
          session.Save (summary);
          summary =  new ReasonSummary (machine1,
                                        UtcDateTime.From (2011, 08, 04), null,
                                        unattendedMOS, reasonUnattended);
          summary.Time = TimeSpan.FromDays (1);
          summary.Number = 1;
          session.Save (summary);
          summary =  new ReasonSummary (machine1,
                                        UtcDateTime.From (2011, 08, 05), null,
                                        unattendedMOS, reasonUnattended);
          summary.Time = TimeSpan.FromHours (4);
          summary.Number = 1;
          session.Save (summary);
          summary =  new ReasonSummary (machine1,
                                        UtcDateTime.From (2011, 08, 05), shift2,
                                        attendedMOS, reasonShort);
          summary.Time = TimeSpan.FromMinutes (1);
          summary.Number = 1;
          session.Save (summary);
        }
        
        // New association 4 -> oo
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, attended, UtcDateTime.From (2011, 08, 04));
          association.User = user1;
          association.Shift = shift2;
          association.DateTime = UtcDateTime.From (2011, 08, 05);
          association.End = UtcDateTime.From (2011, 08, 06);
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
        }
        
        // Run MakeAnalysis
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
        }
        DAOFactory.EmptyAccumulators ();
        
        // Check the values
        {
          // - ObservationStateSlots
          IList<ObservationStateSlot> slots =
            session.CreateCriteria<ObservationStateSlot> ()
            .Add (Restrictions.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<ObservationStateSlot> ();
          Assert.That (slots, Has.Count.EqualTo (5), "Number of observation state slots");
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (null));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (attended));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 06)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 06)));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        {
          // - ReasonSlots
          IList<IReasonSlot> slots =
            session.CreateCriteria<IReasonSlot> ()
            .Add (Restrictions.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<IReasonSlot> ();
          Assert.That (slots, Has.Count.EqualTo (3), "Number of ReasonSlots");
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineMode, Is.EqualTo (active));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].Reason, Is.EqualTo (reasonMotion));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].Reason, Is.EqualTo (reasonUnattended));
            Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
            Assert.That (slots[i].OverwriteRequired, Is.EqualTo (false));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 05)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineMode, Is.EqualTo (inactive));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].Reason, Is.EqualTo (reasonShort));
            Assert.That (slots[i].DefaultReason, Is.EqualTo (true));
            Assert.That (slots[i].OverwriteRequired, Is.EqualTo (false));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 05)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 05, 00, 01, 00)));
          });
          ++i;
        }
        {
          // - MachineActivitySummary
          IList<IMachineActivitySummary> summaries =
            session.CreateCriteria<MachineActivitySummary> ()
            .Add (Restrictions.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("Day"))
            .AddOrder (Order.Asc ("MachineObservationState.Id"))
            .AddOrder (Order.Asc ("MachineMode.Id"))
            .List<IMachineActivitySummary> ();
          Assert.That (summaries, Has.Count.EqualTo (7), "Number of MachineActivitySummaries");
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
            Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (summaries[i].MachineMode, Is.EqualTo (active));
            Assert.That (summaries[i].Shift, Is.EqualTo (null));
            Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (20)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
            Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 08, 02)));
            Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (summaries[i].MachineMode, Is.EqualTo (active));
            Assert.That (summaries[i].Shift, Is.EqualTo (null));
            Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (24)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
            Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (summaries[i].MachineMode, Is.EqualTo (active));
            Assert.That (summaries[i].Shift, Is.EqualTo (null));
            Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (4)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
            Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (summaries[i].MachineMode, Is.EqualTo (inactive));
            Assert.That (summaries[i].Shift, Is.EqualTo (null));
            Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (20)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
            Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (summaries[i].MachineMode, Is.EqualTo (inactive));
            Assert.That (summaries[i].Shift, Is.EqualTo (null));
            Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromDays (1)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
            Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 08, 05)));
            Assert.That (summaries[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (summaries[i].MachineMode, Is.EqualTo (inactive));
            Assert.That (summaries[i].Shift, Is.EqualTo (shift2));
            Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromMinutes (1)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (summaries[i].Machine, Is.EqualTo (machine1));
            Assert.That (summaries[i].Day, Is.EqualTo (UtcDateTime.From (2011, 08, 05)));
            Assert.That (summaries[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (summaries[i].MachineMode, Is.EqualTo (inactive));
            Assert.That (summaries[i].Shift, Is.EqualTo (null));
            Assert.That (summaries[i].Time, Is.EqualTo (TimeSpan.FromHours (4)));
          });
          ++i;
        }
        
        // - Modifications
        AnalysisUnitTests.CheckAllModificationDone<MachineObservationStateAssociation> (session, 2*3);
        AnalysisUnitTests.CheckAllModificationDone<MachineStateTemplateAssociation> (session, 2*1);
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (session, 3); // No machine status found
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis with a machine state template with a stop
    /// </summary>
    [Test]
    public void TestMakeAnalysisWithStop1()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
        IMachineStateTemplate attended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Attended);
        IMachineStateTemplate unattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Unattended);
        IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Attended);
        IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Unattended);
        IReason reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (2);
        IReason reasonShort = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (3);
        IReason reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (4);
        IReason reasonUnattended = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (5);
        IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (1);
        IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (2);
        IMachineMode auto = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (3);
        // New machine state template
        IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("Test");
        mst.AddItem (attendedMOS);
        IMachineStateTemplateStop stop = mst.AddStop ();
        stop.LocalTime = TimeSpan.FromHours (4);
        stop.WeekDays = DayOfWeek.Monday.ConvertToWeekDay ();
        ModelDAOHelper.DAOFactory.MachineStateTemplateDAO.MakePersistent (mst);
        
        // Existing ObservationStateSlot
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, attendedMOS, UtcDateTime.From (2011, 08, 01));
          association.End = UtcDateTime.From (2011, 08, 03);
          association.User = user1;
          association.Shift = shift1;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, unattendedMOS, UtcDateTime.From (2011, 08, 03));
          association.End = UtcDateTime.From (2011, 08, 05);
          association.User = null;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, attendedMOS, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05)));
          association.User = user1;
          association.Shift = shift2;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        // Run MakeAnalysis to initialize the MachineObservationStates
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (NHibernateHelper.GetCurrentSession ());
        }
        
        // New association 4 -> oo
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, mst, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 04)));
          association.User = user1;
          association.Shift = shift2;
          association.DateTime = UtcDateTime.From (2011, 08, 05); // Friday
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
        }
        DateTime mstEnd = new DateTime (2011, 08, 08, 04, 00, 00, DateTimeKind.Local).ToUniversalTime ();
        
        // Run MakeAnalysis
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
        }
        DAOFactory.EmptyAccumulators ();
        
        // Check the values
        {
          // - ObservationStateSlots
          IList<ObservationStateSlot> slots =
            session.CreateCriteria<ObservationStateSlot> ()
            .Add (Restrictions.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<ObservationStateSlot> ();
          Assert.That (slots, Has.Count.EqualTo (5), "Number of observation state slots");
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (null));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (mst));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (mstEnd));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (mstEnd));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        
        // - Modifications
        AnalysisUnitTests.CheckAllModificationDone<MachineObservationStateAssociation> (session, 2*3);
        AnalysisUnitTests.CheckAllModificationDone<MachineStateTemplateAssociation> (session, 3);
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (session, 3); // No machine status found
        
        transaction.Rollback ();
      }
    }

    /// <summary>
    /// Test the method MakeAnalysis with the synchronous option
    /// </summary>
    [Test]
    public void TestSynchronousAnalysis()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IMonitoredMachine machine1 = ModelDAOHelper.DAOFactory.MonitoredMachineDAO.FindById (3);
        IMachineStateTemplate attended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Attended);
        IMachineStateTemplate unattended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Unattended);
        IMachineObservationState attendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Attended);
        IMachineObservationState unattendedMOS = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Unattended);
        IReason reasonMotion = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (2);
        IReason reasonShort = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (3);
        IReason reasonUnanswered = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (4);
        IReason reasonUnattended = ModelDAOHelper.DAOFactory.ReasonDAO.FindById (5);
        IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (1);
        IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (2);
        IMachineMode auto = ModelDAOHelper.DAOFactory.MachineModeDAO.FindById (3);
        
        // Existing ObservationStateSlot
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, attendedMOS, UtcDateTime.From (2011, 08, 01));
          association.End = UtcDateTime.From (2011, 08, 03);
          association.User = user1;
          association.Shift = shift1;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, unattendedMOS, UtcDateTime.From (2011, 08, 03));
          association.End = UtcDateTime.From (2011, 08, 05);
          association.User = null;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, attendedMOS, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05)));
          association.User = user1;
          association.Shift = shift2;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
            .MakePersistent (association);
        }
        // Run MakeAnalysis to initialize the MachineObservationStates
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (NHibernateHelper.GetCurrentSession ());
        }
        
        // New association 4 -> oo
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, attended, UtcDateTime.From (2011, 08, 04));
          association.User = user1;
          association.Shift = shift2;
          association.DateTime = UtcDateTime.From (2011, 08, 05);
          association.End = UtcDateTime.From (2011, 08, 06);
          association.Option = AssociationOption.Synchronous;
          ModelDAOHelper.DAOFactory.MachineStateTemplateAssociationDAO
            .MakePersistent (association);
        }
        
        // Run MakeAnalysis
        {
          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
        }
        DAOFactory.EmptyAccumulators ();
        
        // Check the values
        {
          // - ObservationStateSlots
          IList<ObservationStateSlot> slots =
            session.CreateCriteria<ObservationStateSlot> ()
            .Add (Restrictions.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<ObservationStateSlot> ();
          Assert.That (slots, Has.Count.EqualTo (5), "Number of observation state slots");
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (unattendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (attended));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 06)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (attendedMOS));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (null));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift2));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 06)));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        
        // - Modifications
        AnalysisUnitTests.CheckAllModificationDone<MachineObservationStateAssociation> (session, 2*3);
        AnalysisUnitTests.CheckAllModificationDone<MachineStateTemplateAssociation> (session, 2*1);
        
        transaction.Rollback ();
      }
    }    
    
    /// <summary>
    /// Dynamic time extension used by the tests of the dynamic end
    ///
    /// The successive responses are set by <see cref="SetResponses"/>. Once all of them have been
    /// returned, the last one is repeated, so that the tests do not depend on the exact number of calls.
    /// </summary>
    public class TestDynamicTime : IDynamicTimeExtension
    {
      static readonly IList<Func<TestDynamicTime, IDynamicTimeResponse>> DEFAULT_RESPONSES =
        new List<Func<TestDynamicTime, IDynamicTimeResponse>> { x => x.CreatePending () };

      static IList<Func<TestDynamicTime, IDynamicTimeResponse>> s_responses = DEFAULT_RESPONSES;
      static int s_step = 0;

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public IMachine Machine { get; set; }

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public string Name => "Test";

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public bool UniqueInstance => true;

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public bool Initialize (IMachine machine, string parameter)
      {
        this.Machine = machine;
        return true;
      }

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public bool IsApplicable () => true;

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public DynamicTimeApplicableStatus IsApplicableAt (DateTime at) => DynamicTimeApplicableStatus.Always;

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public IDynamicTimeResponse Get (DateTime dateTime, UtcDateTimeRange hint, UtcDateTimeRange limit)
      {
        var index = Math.Min (s_step++, s_responses.Count - 1);
        return s_responses[index] (this);
      }

      /// <summary>
      /// <see cref="IDynamicTimeExtension"/>
      /// </summary>
      public TimeSpan GetCacheTimeout (IDynamicTimeResponse data) => TimeSpan.FromTicks (0);

      /// <summary>
      /// Set the successive responses to return, the last one being repeated
      /// </summary>
      /// <param name="responses">not empty</param>
      public static void SetResponses (params Func<TestDynamicTime, IDynamicTimeResponse>[] responses)
      {
        s_responses = responses.ToList ();
        s_step = 0;
      }

      /// <summary>
      /// Reset the responses, to call in a finally block
      /// </summary>
      public static void Reset ()
      {
        s_responses = DEFAULT_RESPONSES;
        s_step = 0;
      }
    }

    /// <summary>
    /// Set an initial machine observation state from T(1), so that the machine has a known
    /// machine observation state before the tested machine state template associations are applied
    /// </summary>
    void InitializeObservationStateSlots (IMonitoredMachine machine, IMachineObservationState machineObservationState, IUser user)
    {
      var association = ModelDAOHelper.ModelFactory
        .CreateMachineObservationStateAssociation (machine, machineObservationState, new UtcDateTimeRange (T (1)));
      association.User = user;
      ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (association);
      AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (NHibernateHelper.GetCurrentSession ());
    }

    /// <summary>
    /// Get the machine state template of the observation state slot at a specific UTC date/time
    ///
    /// Note: the slots are searched one by one on purpose, so that the tests do not depend on the
    /// total number of slots
    /// </summary>
    /// <returns>the machine state template, or null if there is none at this date/time</returns>
    static IMachineStateTemplate GetMachineStateTemplateAt (ISession session, IMachine machine, DateTime at)
    {
      var slot = session.CreateCriteria<ObservationStateSlot> ()
        .Add (Restrictions.Eq ("Machine", machine))
        .List<ObservationStateSlot> ()
        .FirstOrDefault (s => s.DateTimeRange.ContainsElement (at));
      return slot?.MachineStateTemplate;
    }

    /// <summary>
    /// Run the analysis of the first pending modification several times
    ///
    /// This is required instead of RunMakeAnalysis when a modification remains pending,
    /// for example a dynamic end tracker whose dynamic start is not known yet:
    /// RunMakeAnalysis would loop for ever on it
    /// </summary>
    static void RunFirstSeveralTimes (int number)
    {
      for (int i = 0; i < number; ++i) {
        AnalysisUnitTests.RunFirst ();
      }
    }

    /// <summary>
    /// Test the analysis when the dynamic end is immediately known:
    /// the machine state template must be applied up to the dynamic end only
    /// </summary>
    [Test]
    public void TestDynamicEndFinal ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineStateTemplate attended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Attended);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          // The dynamic end is known at once: T(5)
          var dynamicEnd = T (5);
          TestDynamicTime.SetResponses (x => x.CreateFinal (dynamicEnd));

          // New association [T(4), T(6)) with the dynamic end Test
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, attended, R (4, 6));
            association.User = user1;
            association.DateTime = T (4);
            association.Dynamic = ",Test";
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.EqualTo (attended),
              "the machine state template is applied at T(4)");
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (5)), Is.Not.EqualTo (attended),
              "the machine state template is not applied at T(5), after the dynamic end");
          });

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
      }
    }

    /// <summary>
    /// Test the analysis with the aggressive strategy (the default one) when the dynamic end
    /// is not known yet: the machine state template must be applied at once on the whole range,
    /// and a dynamic end tracker with the next machine state template must be created
    /// </summary>
    [Test]
    public void TestDynamicEndAggressive ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineStateTemplate unattended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Unattended);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          // A machine state template with a next machine state template, used by the dynamic end tracker
          IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestDynamicEnd");
          mst.AddItem (attendedMOS);
          mst.NextMachineStateTemplate = unattended;
          daoFactory.MachineStateTemplateDAO.MakePersistent (mst);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          // The dynamic end is never known: only a hint is returned
          var hint = R (4);
          TestDynamicTime.SetResponses (x => x.CreateWithHint (hint));

          // New association [T(4), oo) with the dynamic end Test
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, mst, R (4));
            association.User = user1;
            association.DateTime = T (4);
            association.Dynamic = ",Test";
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          // Note: RunMakeAnalysis can't be used here, the dynamic end tracker remains pending
          RunFirstSeveralTimes (10);
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.EqualTo (mst),
              "the machine state template is applied aggressively at T(4)");
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (10)), Is.EqualTo (mst),
              "the machine state template is applied aggressively after the possible dynamic end");
          });

          // A dynamic end tracker with the next machine state template was created
          var associations = session.CreateCriteria<MachineStateTemplateAssociation> ()
            .List<MachineStateTemplateAssociation> ();
          Assert.That (associations.Any (a => object.Equals (a.MachineStateTemplate, unattended)), Is.True,
            "a dynamic end tracker with the next machine state template was created");

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
      }
    }

    /// <summary>
    /// Test the analysis when the dynamic end is not applicable: the machine state template
    /// must not be applied and the reason must be recorded in the analysis logs
    /// </summary>
    [Test]
    public void TestDynamicEndNotApplicable ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineStateTemplate attended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Attended);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          TestDynamicTime.SetResponses (x => x.CreateNotApplicable ());

          // New association [T(4), T(6)) with the dynamic end Test
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, attended, R (4, 6));
            association.User = user1;
            association.DateTime = T (4);
            association.Dynamic = ",Test";
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
          DAOFactory.EmptyAccumulators ();

          var associations = session.CreateCriteria<MachineStateTemplateAssociation> ()
            .List<MachineStateTemplateAssociation> ();
          Assert.Multiple (() => {
            Assert.That (associations.All (a => a.AnalysisStatus.Equals (AnalysisStatus.NotApplicable)), Is.True,
              "the association is not applicable");
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.Not.EqualTo (attended),
              "the machine state template is not applied");
          });

          // The reason why the machine state template was not applied must be traceable
          var logs = ModelDAOHelper.DAOFactory.MachineModificationLogDAO.FindAll ();
          Assert.That (logs.Any (l => l.Level.Equals (LogLevel.WARN)), Is.True,
            "a warning analysis log records why the dynamic end is not applicable");

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
      }
    }

    /// <summary>
    /// Test the analysis when the dynamic start of the dynamic end tracker is not applicable
    /// after the machine state template was applied aggressively
    ///
    /// The machine state template must remain applied, it is never reverted, but the reason
    /// must be recorded in the analysis logs
    /// </summary>
    [Test]
    public void TestDynamicEndTrackerNotApplicable ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineStateTemplate unattended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Unattended);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestDynamicEnd");
          mst.AddItem (attendedMOS);
          mst.NextMachineStateTemplate = unattended;
          daoFactory.MachineStateTemplateDAO.MakePersistent (mst);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          // First step: the dynamic end is not known yet, the machine state template is applied aggressively
          var hint = R (4);
          TestDynamicTime.SetResponses (x => x.CreateWithHint (hint));

          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, mst, R (4));
            association.User = user1;
            association.DateTime = T (4);
            association.Dynamic = ",Test";
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          RunFirstSeveralTimes (10);
          DAOFactory.EmptyAccumulators ();

          Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.EqualTo (mst),
            "the machine state template was applied aggressively");

          // Second step: the dynamic start of the tracker becomes not applicable
          TestDynamicTime.SetResponses (x => x.CreateNotApplicable ());
          RunFirstSeveralTimes (10);
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.EqualTo (mst),
              "the machine state template applied aggressively is kept, it is not reverted");
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (10)), Is.EqualTo (mst),
              "the machine state template remains effective after the expected dynamic end");
          });

          // The consequence must be traceable, since the data is not reverted
          var logs = ModelDAOHelper.DAOFactory.MachineModificationLogDAO.FindAll ();
          Assert.That (logs.Any (l => l.Level.Equals (LogLevel.WARN)), Is.True,
            "a warning analysis log records that the machine state template remains effective");

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
      }
    }

    /// <summary>
    /// Test the analysis when a next machine state template is set in the association:
    /// the dynamic end tracker must use it and not the next machine state template
    /// of the machine state template
    /// </summary>
    [Test]
    public void TestAssociationNextMachineStateTemplate ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineStateTemplate attended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Attended);
          IMachineStateTemplate unattended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Unattended);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          // The machine state template references unattended as next machine state template
          IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestDynamicEnd");
          mst.AddItem (attendedMOS);
          mst.NextMachineStateTemplate = unattended;
          daoFactory.MachineStateTemplateDAO.MakePersistent (mst);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          // The dynamic end is never known: only a hint is returned, so that a dynamic end tracker is created
          var hint = R (4);
          TestDynamicTime.SetResponses (x => x.CreateWithHint (hint));

          // New association [T(4), oo) with the dynamic end Test,
          // that references attended as next machine state template
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, mst, R (4));
            association.NextMachineStateTemplate = attended;
            association.User = user1;
            association.DateTime = T (4);
            association.Dynamic = ",Test";
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          // Note: RunMakeAnalysis can't be used here, the dynamic end tracker remains pending
          RunFirstSeveralTimes (10);
          DAOFactory.EmptyAccumulators ();

          var associations = session.CreateCriteria<MachineStateTemplateAssociation> ()
            .List<MachineStateTemplateAssociation> ();
          Assert.Multiple (() => {
            Assert.That (associations.Any (a => object.Equals (a.MachineStateTemplate, attended)), Is.True,
              "the dynamic end tracker uses the next machine state template of the association");
            Assert.That (associations.Any (a => object.Equals (a.MachineStateTemplate, unattended)), Is.False,
              "the next machine state template of the machine state template is not used");
          });

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
      }
    }

    /// <summary>
    /// Test the analysis when no dynamic end is set in the association while there is no upper
    /// bound in its range: the dynamic end of the machine state template must be considered
    /// </summary>
    [Test]
    public void TestMachineStateTemplateDynamicEnd ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          // A machine state template with a dynamic end
          IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestDynamicEnd");
          mst.AddItem (attendedMOS);
          mst.DynamicEnd = "Test";
          daoFactory.MachineStateTemplateDAO.MakePersistent (mst);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          // The dynamic end is known at once: T(5)
          var dynamicEnd = T (5);
          TestDynamicTime.SetResponses (x => x.CreateFinal (dynamicEnd));

          // New association [T(4), oo) with no dynamic end
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, mst, R (4));
            association.User = user1;
            association.DateTime = T (4);
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.EqualTo (mst),
              "the machine state template is applied at T(4)");
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (6)), Is.Not.EqualTo (mst),
              "the machine state template is not applied at T(6), after the dynamic end of the machine state template");
          });

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
      }
    }

    /// <summary>
    /// Test the analysis when the dynamic end is known at once:
    /// the next machine state template must be applied after the dynamic end, exactly like
    /// when the dynamic end is known only later (see <see cref="TestDynamicEndKnownLaterNextMachineStateTemplate"/>)
    /// </summary>
    [Test]
    public void TestDynamicEndKnownAtOnceNextMachineStateTemplate ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineStateTemplate unattended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Unattended);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestDynamicEnd");
          mst.AddItem (attendedMOS);
          mst.NextMachineStateTemplate = unattended;
          daoFactory.MachineStateTemplateDAO.MakePersistent (mst);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          // The dynamic end is known at once: T(5)
          TestDynamicTime.SetResponses (x => x.CreateFinal (T (5)));

          // New association [T(4), oo) with the dynamic end Test
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, mst, R (4));
            association.User = user1;
            association.DateTime = T (4);
            association.Dynamic = ",Test";
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.EqualTo (mst),
              "the machine state template is applied before the dynamic end");
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (6)), Is.EqualTo (unattended),
              "the next machine state template is applied after the dynamic end");
          });

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
      }
    }

    /// <summary>
    /// Test the analysis when the dynamic end is known only after the machine state template
    /// was applied aggressively: the result must be the same as when the dynamic end is known
    /// at once (see <see cref="TestDynamicEndKnownAtOnceNextMachineStateTemplate"/>)
    /// </summary>
    [Test]
    public void TestDynamicEndKnownLaterNextMachineStateTemplate ()
    {
      Lemoine.Extensions.ExtensionManager.Add (typeof (TestDynamicTime));

      try {
        IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
        using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ()) {
          ISession session = NHibernateHelper.GetCurrentSession ();
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineStateTemplate unattended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Unattended);
          IMachineObservationState attendedMOS = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);

          IMachineStateTemplate mst = ModelDAOHelper.ModelFactory.CreateMachineStateTemplate ("TestDynamicEnd");
          mst.AddItem (attendedMOS);
          mst.NextMachineStateTemplate = unattended;
          daoFactory.MachineStateTemplateDAO.MakePersistent (mst);

          InitializeObservationStateSlots (machine1, attendedMOS, user1);

          // The dynamic end is not known on the first call (only a hint), then it is known: T(5)
          TestDynamicTime.SetResponses (x => x.CreateWithHint (R (4)),
                                        x => x.CreateFinal (T (5)));

          // New association [T(4), oo) with the dynamic end Test
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineStateTemplateAssociation (machine1, mst, R (4));
            association.User = user1;
            association.DateTime = T (4);
            association.Dynamic = ",Test";
            daoFactory.MachineStateTemplateAssociationDAO.MakePersistent (association);
          }

          AnalysisUnitTests.RunMakeAnalysis<MachineStateTemplateAssociation> (session);
          DAOFactory.EmptyAccumulators ();

          Assert.Multiple (() => {
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (4)), Is.EqualTo (mst),
              "the machine state template is applied before the dynamic end");
            Assert.That (GetMachineStateTemplateAt (session, machine1, T (6)), Is.EqualTo (unattended),
              "the next machine state template is applied after the dynamic end");
          });

          transaction.Rollback ();
        }
      }
      finally {
        Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();
        TestDynamicTime.Reset ();
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
