// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
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
          Assert.AreEqual (5, slots.Count, "Number of observation state slots");
          int i = 1;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift1, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (null, slots [i].User);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (null, slots [i].MachineObservationState);
          Assert.AreEqual (attended, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 06), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 06), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        {
          // - ReasonSlots
          IList<IReasonSlot> slots =
            session.CreateCriteria<IReasonSlot> ()
            .Add (Restrictions.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<IReasonSlot> ();
          Assert.AreEqual (3, slots.Count, "Number of ReasonSlots");
          int i = 0;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (active, slots [i].MachineMode);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (reasonMotion, slots [i].Reason);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (inactive, slots [i].MachineMode);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (reasonUnattended, slots [i].Reason);
          Assert.AreEqual (true, slots [i].DefaultReason);
          Assert.AreEqual (false, slots [i].OverwriteRequired);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 05), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (inactive, slots [i].MachineMode);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (reasonShort, slots [i].Reason);
          Assert.AreEqual (true, slots [i].DefaultReason);
          Assert.AreEqual (false, slots [i].OverwriteRequired);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 05), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 05, 00, 01, 00), slots [i].EndDateTime.Value);
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
          Assert.AreEqual (7, summaries.Count, "Number of MachineActivitySummaries");
          int i = 0;
          Assert.AreEqual (machine1, summaries [i].Machine);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), summaries [i].Day);
          Assert.AreEqual (attendedMOS, summaries [i].MachineObservationState);
          Assert.AreEqual (active, summaries [i].MachineMode);
          Assert.AreEqual (null, summaries [i].Shift);
          Assert.AreEqual (TimeSpan.FromHours (20), summaries [i].Time);
          ++i;
          Assert.AreEqual (machine1, summaries [i].Machine);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 02), summaries [i].Day);
          Assert.AreEqual (attendedMOS, summaries [i].MachineObservationState);
          Assert.AreEqual (active, summaries [i].MachineMode);
          Assert.AreEqual (null, summaries [i].Shift);
          Assert.AreEqual (TimeSpan.FromHours (24), summaries [i].Time);
          ++i;
          Assert.AreEqual (machine1, summaries [i].Machine);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries [i].Day);
          Assert.AreEqual (attendedMOS, summaries [i].MachineObservationState);
          Assert.AreEqual (active, summaries [i].MachineMode);
          Assert.AreEqual (null, summaries [i].Shift);
          Assert.AreEqual (TimeSpan.FromHours (4), summaries [i].Time);
          ++i;
          Assert.AreEqual (machine1, summaries [i].Machine);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries [i].Day);
          Assert.AreEqual (unattendedMOS, summaries [i].MachineObservationState);
          Assert.AreEqual (inactive, summaries [i].MachineMode);
          Assert.AreEqual (null, summaries [i].Shift);
          Assert.AreEqual (TimeSpan.FromHours (20), summaries [i].Time);
          ++i;
          Assert.AreEqual (machine1, summaries [i].Machine);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), summaries [i].Day);
          Assert.AreEqual (unattendedMOS, summaries [i].MachineObservationState);
          Assert.AreEqual (inactive, summaries [i].MachineMode);
          Assert.AreEqual (null, summaries [i].Shift);
          Assert.AreEqual (TimeSpan.FromDays (1), summaries [i].Time);
          ++i;
          Assert.AreEqual (machine1, summaries [i].Machine);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 05), summaries [i].Day);
          Assert.AreEqual (attendedMOS, summaries [i].MachineObservationState);
          Assert.AreEqual (inactive, summaries [i].MachineMode);
          Assert.AreEqual (shift2, summaries [i].Shift);
          Assert.AreEqual (TimeSpan.FromMinutes (1), summaries [i].Time);
          ++i;
          Assert.AreEqual (machine1, summaries [i].Machine);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 05), summaries [i].Day);
          Assert.AreEqual (unattendedMOS, summaries [i].MachineObservationState);
          Assert.AreEqual (inactive, summaries [i].MachineMode);
          Assert.AreEqual (null, summaries [i].Shift);
          Assert.AreEqual (TimeSpan.FromHours (4), summaries [i].Time);
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
          Assert.AreEqual (5, slots.Count, "Number of observation state slots");
          int i = 1;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift1, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (null, slots [i].User);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (null, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots [i].BeginDateTime.Value);
          Assert.AreEqual (mstEnd, slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (mstEnd, slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
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
          Assert.AreEqual (5, slots.Count, "Number of observation state slots");
          int i = 1;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift1, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (null, slots [i].User);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (attended, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots [i].BeginDateTime.Value);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 06), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (UtcDateTime.From (2011, 08, 06), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        
        // - Modifications
        AnalysisUnitTests.CheckAllModificationDone<MachineObservationStateAssociation> (session, 2*3);
        AnalysisUnitTests.CheckAllModificationDone<MachineStateTemplateAssociation> (session, 2*1);
        
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
