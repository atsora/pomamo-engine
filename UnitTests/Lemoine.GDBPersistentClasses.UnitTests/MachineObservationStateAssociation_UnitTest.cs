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
  /// Unit tests for the class MachineObservationStateAssociation
  /// </summary>
  [TestFixture]
  public class MachineObservationStateAssociation_UnitTest: WithDayTimeStamp
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (MachineObservationStateAssociation_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public MachineObservationStateAssociation_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }

    /// <summary>
    /// Test local time setter for begin/end
    /// </summary>
    [Test]
    public void TestLocalTimeSetter()
    {

      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        // Reference data
        User user1 = session.Get<User> (1);
        MonitoredMachine machine1 = session.Get<MonitoredMachine> (1);
        MachineObservationState attended =
          session.Get<MachineObservationState> ((int) MachineObservationStateId.Attended);
        MachineObservationState unattended =
          session.Get<MachineObservationState> ((int) MachineObservationStateId.Unattended);
        Reason reasonMotion = session.Get<Reason> (2);
        Reason reasonShort = session.Get<Reason> (3);
        Reason reasonUnanswered = session.Get<Reason> (4);
        Reason reasonUnattended = session.Get<Reason> (5);
        MachineMode inactive = session.Get<MachineMode> (1);
        MachineMode active = session.Get<MachineMode> (2);
        MachineMode auto = session.Get<MachineMode> (3);
        
        // Existing ObservationStateSlot
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine1, attended, UtcDateTime.From (0));
          association.User = user1;
          
          ((MachineObservationStateAssociation)association).LocalBegin = "01/08/2011" ; // UtcDateTime.From (2011, 08, 01);
          ((MachineObservationStateAssociation)association).LocalEnd = "03/08/2011"; // UtcDateTime.From (2011, 08, 03);
          
          Assert.AreEqual(association.Begin,
                          System.DateTime.Parse(((MachineObservationStateAssociation)association).LocalBegin).ToUniversalTime());
          
          Assert.AreEqual(true, association.End.HasValue);
          Assert.AreEqual(true, ((MachineObservationStateAssociation)association).LocalEnd != null);
          
          Assert.AreEqual(association.End.Value,
                          System.DateTime.Parse(((MachineObservationStateAssociation)association).LocalEnd).ToUniversalTime());

        }
        
        transaction.Rollback ();
      }
      
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
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
          IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
          IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
          MonitoredMachine machine1 = session.Get<MonitoredMachine> (3);
          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          MachineObservationState unattended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);
          Reason reasonMotion = session.Get<Reason> (2);
          Reason reasonShort = session.Get<Reason> (3);
          Reason reasonUnanswered = session.Get<Reason> (4);
          Reason reasonUnattended = session.Get<Reason> (5);
          MachineMode inactive = session.Get<MachineMode> (1);
          MachineMode active = session.Get<MachineMode> (2);
          MachineMode auto = session.Get<MachineMode> (3);

          // Existing ObservationStateSlot
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, attended, UtcDateTime.From (2011, 08, 01));
            association.End = UtcDateTime.From (2011, 08, 03);
            association.User = user1;
            association.Shift = shift1;
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, unattended, UtcDateTime.From (2011, 08, 03));
            association.End = UtcDateTime.From (2011, 08, 05);
            association.User = null;
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, attended, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05)));
            association.User = user1;
            association.Shift = shift2;
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }
          // Run MakeAnalysis to initialize the MachineObservationStates
          {
            AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (session);
          }
          // Existing ReasonSlot
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              R (1, 3));
            existingSlot.MachineMode = active;
            existingSlot.MachineObservationState = attended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonMotion, 10.0, false, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              R (3, 5));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = unattended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnattended, 10.0, false, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              new UtcDateTimeRange (T (5), T (5).AddMinutes (1)));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            existingSlot.Shift = shift2;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonShort, 10.0, false, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 01),
                                                                                attended, active);
            summary.Time = TimeSpan.FromHours (20); // Note: cut-off of time is at 20:00 UTC, 22:00 Local
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 02),
                                                                                attended, active);
            summary.Time = TimeSpan.FromDays (1);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 03),
                                                                                attended, active);
            summary.Time = TimeSpan.FromHours (4);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 03),
                                                                                unattended, inactive);
            summary.Time = TimeSpan.FromHours (20);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 04),
                                                                                unattended, inactive);
            summary.Time = TimeSpan.FromDays (1);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 05),
                                                                                unattended, inactive);
            summary.Time = TimeSpan.FromHours (4);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 05),
                                                                                attended, inactive, shift2);
            summary.Time = TimeSpan.FromMinutes (1);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            ReasonSummary summary;
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 01), null,
                                          attended, reasonMotion);
            summary.Time = TimeSpan.FromHours (20);
            summary.Number = 1;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 02), null,
                                          attended, reasonMotion);
            summary.Time = TimeSpan.FromDays (1);
            summary.Number = 1;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 03), null,
                                          attended, reasonMotion);
            summary.Time = TimeSpan.FromHours (4);
            summary.Number = 1;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 03), null,
                                          unattended, reasonUnattended);
            summary.Time = TimeSpan.FromHours (20);
            summary.Number = 1;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 04), null,
                                          unattended, reasonUnattended);
            summary.Time = TimeSpan.FromDays (1);
            summary.Number = 1;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 05), null,
                                          unattended, reasonUnattended);
            summary.Time = TimeSpan.FromHours (4);
            summary.Number = 1;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 05), shift2,
                                          attended, reasonShort);
            summary.Time = TimeSpan.FromMinutes (1);
            summary.Number = 1;
            session.Save (summary);
          }

          // New association 4 -> oo
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, attended, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 04)));
            association.User = user1;
            association.Shift = shift2;
            association.DateTime = UtcDateTime.From (2011, 08, 05);
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }

          // Run MakeAnalysis
          {
            AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (session);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
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
            Assert.AreEqual (4, slots.Count, "Number of observation state slots");
            int i = 1;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (user1, slots[i].User);
            Assert.AreEqual (shift1, slots[i].Shift);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (unattended, slots[i].MachineObservationState);
            Assert.AreEqual (null, slots[i].User);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (user1, slots[i].User);
            Assert.AreEqual (shift2, slots[i].Shift);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots[i].BeginDateTime.Value);
            Assert.IsFalse (slots[i].EndDateTime.HasValue);
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
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (active, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonMotion, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (unattended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (false, slots[i].OverwriteRequired);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (true, slots[i].OverwriteRequired);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 05, 00, 01, 00), slots[i].EndDateTime.Value);
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
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (active, summaries[i].MachineMode);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (20), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (active, summaries[i].MachineMode);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (24), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (active, summaries[i].MachineMode);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (4), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (20), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (shift2, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (20), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (null, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromHours (4), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 05), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (shift2, summaries[i].Shift);
            Assert.AreEqual (TimeSpan.FromMinutes (1) + TimeSpan.FromHours (4), summaries[i].Time);
            ++i;
          }
          // - Modifications
          AnalysisUnitTests.CheckAllModificationDone<MachineObservationStateAssociation> (session, 2 * 4);
          // - AnalysisLogs
          AnalysisUnitTests.CheckNumberOfAnalysisLogs (session, 4); // No machine status found
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis
    /// </summary>
    [Test]
    public void TestMakeAnalysis2()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = daoFactory.UserDAO.FindById (1);
          IShift shift3 = daoFactory.ShiftDAO.FindById (3);
          IMonitoredMachine machine1 = daoFactory.MonitoredMachineDAO.FindById (3);
          IMachineObservationState attended = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Attended);
          IMachineObservationState unattended = daoFactory.MachineObservationStateDAO
            .FindById ((int)MachineObservationStateId.Unattended);
          IMachineStateTemplate mstAttended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Attended);
          IMachineStateTemplate mstUnattended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Unattended);
          IReason reasonMotion = daoFactory.ReasonDAO.FindById (2);
          IReason reasonShort = daoFactory.ReasonDAO.FindById (3);
          IReason reasonUnanswered = daoFactory.ReasonDAO.FindById (4);
          IReason reasonUnattended = daoFactory.ReasonDAO.FindById (5);
          IMachineMode inactive = daoFactory.MachineModeDAO.FindById (1);
          IMachineMode active = daoFactory.MachineModeDAO.FindById (2);
          IMachineMode auto = daoFactory.MachineModeDAO.FindById (3);

          // Existing ObservationStateSlot
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, unattended, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 03)));
            ((MachineObservationStateAssociation)association).MachineStateTemplate = mstUnattended;
            association.MachineObservationState = unattended;
            association.User = null;
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }
          // Run MakeAnalysis to initialize the MachineObservationStates
          {
            AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (session);
          }
          // Existing ReasonSlot
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              new UtcDateTimeRange (UtcDateTime.From (2011, 08, 03, 10, 00, 00),
                                                    UtcDateTime.From (2011, 08, 03, 12, 00, 00)));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = unattended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnattended, 10.0, false, true);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              new UtcDateTimeRange (UtcDateTime.From (2011, 08, 03, 12, 00, 00),
                                                    UtcDateTime.From (2011, 08, 03, 13, 00, 00)));
            existingSlot.MachineMode = active;
            existingSlot.MachineObservationState = unattended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonMotion, 10.0, false, true);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              new UtcDateTimeRange (UtcDateTime.From (2011, 08, 03, 13, 00, 00),
                                                    UtcDateTime.From (2011, 08, 03, 14, 00, 00)));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = unattended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnattended, 10.0, false, true);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              new UtcDateTimeRange (UtcDateTime.From (2011, 08, 03, 14, 00, 00),
                                                    UtcDateTime.From (2011, 08, 03, 16, 00, 00)));
            existingSlot.MachineMode = active;
            existingSlot.MachineObservationState = unattended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonMotion, 10.0, false, true);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }

          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 03),
                                                                                unattended, active);
            summary.Time = TimeSpan.FromHours (3);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                UtcDateTime.From (2011, 08, 03),
                                                                                unattended, inactive);
            summary.Time = TimeSpan.FromHours (3);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            ReasonSummary summary;
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 03), null,
                                          unattended, reasonMotion);
            summary.Time = TimeSpan.FromHours (3);
            summary.Number = 2;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          UtcDateTime.From (2011, 08, 03), null,
                                          unattended, reasonUnattended);
            summary.Time = TimeSpan.FromHours (3);
            summary.Number = 2;
            session.Save (summary);
          }

          // New association 11:00 -> 15:00
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, attended, UtcDateTime.From (2011, 08, 03, 11, 00, 00));
            association.User = user1;
            association.Shift = user1.Shift;
            association.DateTime = UtcDateTime.From (2011, 08, 03, 11, 00, 00);
            association.End = UtcDateTime.From (2011, 08, 03, 15, 00, 00); ;
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }

          // Run MakeAnalysis
          {
            AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (session);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          }
          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            // - ObservationStateSlots
            IList<ObservationStateSlot> slots =
              session.CreateCriteria<ObservationStateSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ObservationStateSlot> ();
            Assert.AreEqual (3, slots.Count, "Number of observation state slots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (unattended, slots[i].MachineObservationState);
            Assert.AreEqual (null, slots[i].User);
            Assert.AreEqual (null, slots[i].Shift);
            Assert.IsFalse (slots[i].BeginDateTime.HasValue);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 11, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (user1, slots[i].User);
            Assert.AreEqual (shift3, slots[i].Shift);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 11, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 15, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (unattended, slots[i].MachineObservationState);
            Assert.AreEqual (null, slots[i].User);
            Assert.AreEqual (null, slots[i].Shift);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 15, 00, 00), slots[i].BeginDateTime.Value);
            Assert.IsFalse (slots[i].EndDateTime.HasValue);
          }
          {
            // - ReasonSlots
            IList<IReasonSlot> slots =
              session.CreateCriteria<IReasonSlot> ()
              .Add (Restrictions.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<IReasonSlot> ();
            Assert.AreEqual (6, slots.Count, "Number of ReasonSlots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (unattended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 10, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 11, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 11, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 12, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (active, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonMotion, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 12, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 13, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 13, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 14, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (active, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonMotion, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 14, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 15, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (active, slots[i].MachineMode);
            Assert.AreEqual (unattended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonMotion, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 15, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03, 16, 00, 00), slots[i].EndDateTime.Value);
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
            Assert.AreEqual (4, summaries.Count, "Number of MachineActivitySummaries");
            int i = 0;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (active, summaries[i].MachineMode);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (TimeSpan.FromHours (1), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (active, summaries[i].MachineMode);
            Assert.AreEqual (TimeSpan.FromHours (1), summaries[i].Time);
          }
          {
            // - ReasonSummary
            IList<IReasonSummary> summaries =
              session.CreateCriteria<ReasonSummary> ()
              .Add (Restrictions.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("Day"))
              .AddOrder (Order.Asc ("MachineObservationState.Id"))
              .AddOrder (Order.Asc ("Reason.Id"))
              .List<IReasonSummary> ();
            Assert.AreEqual (4, summaries.Count, "Number of ReasonSummaries");
            int i = 0;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonMotion, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            Assert.AreEqual (2, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (2), summaries[i].Time);
            Assert.AreEqual (2, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonMotion, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (1), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 03), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (1), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
          }
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis with
    /// - before: unattended, attended
    /// - after: attended (shift 2)
    /// </summary>
    [Test]
    public void TestMakeAnalysis3()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));

          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
          IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
          IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
          MonitoredMachine machine1 = session.Get<MonitoredMachine> (3);
          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          MachineObservationState unattended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);
          IMachineStateTemplate mstAttended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Attended);
          IMachineStateTemplate mstUnattended = daoFactory.MachineStateTemplateDAO
            .FindById ((int)StateTemplate.Unattended);
          Reason reasonMotion = session.Get<Reason> (2);
          Reason reasonShort = session.Get<Reason> (3);
          Reason reasonUnanswered = session.Get<Reason> (4);
          Reason reasonUnattended = session.Get<Reason> (5);
          MachineMode inactive = session.Get<MachineMode> (1);
          MachineMode active = session.Get<MachineMode> (2);
          MachineMode auto = session.Get<MachineMode> (3);

          // Existing ObservationStateSlot
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, unattended, UtcDateTime.From (2011, 08, 03));
            association.End = UtcDateTime.From (2011, 08, 05);
            association.MachineObservationState = unattended;
            ((MachineObservationStateAssociation)association).MachineStateTemplate = mstUnattended;
            association.User = null;
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, attended, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05)));
            association.MachineObservationState = attended;
            ((MachineObservationStateAssociation)association).MachineStateTemplate = mstAttended;
            association.User = user1;
            association.Shift = shift2;
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }
          // Run MakeAnalysis to initialize the MachineObservationStates
          {
            AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (session);
          }
          // Existing ReasonSlot
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              R (3, 5));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = unattended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnattended, 10.0, false, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              new UtcDateTimeRange (UtcDateTime.From (2011, 08, 05),
                                                    UtcDateTime.From (2011, 08, 05, 00, 01, 00)));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = attended;
            existingSlot.Shift = shift2;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonShort, 10.0, false, false);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }

          // New association 4 -> oo
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, attended, new UtcDateTimeRange (UtcDateTime.From (2011, 08, 04)));
            association.User = user1;
            association.Shift = shift2;
            association.DateTime = UtcDateTime.From (2011, 08, 05);
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }

          // Run MakeAnalysis
          {
            AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (session);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          }

          // Check the values
          {
            // - ObservationStateSlots
            IList<ObservationStateSlot> slots =
              session.CreateCriteria<ObservationStateSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ObservationStateSlot> ();
            Assert.AreEqual (2, slots.Count, "Number of observation state slots");
            int i = 1;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (user1, slots[i].User);
            Assert.AreEqual (shift2, slots[i].Shift);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots[i].BeginDateTime.Value);
            Assert.IsFalse (slots[i].EndDateTime.HasValue);
          }
          {
            // - ReasonSlots
            IList<IReasonSlot> slots =
              session.CreateCriteria<IReasonSlot> ()
              .Add (Restrictions.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<IReasonSlot> ();
            Assert.AreEqual (2, slots.Count, "Number of ReasonSlots");
            int i = 1;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (true, slots[i].OverwriteRequired);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 05, 00, 01, 00), slots[i].EndDateTime.Value);
            ++i;
          }
          // - Modifications
          AnalysisUnitTests.CheckAllModificationDone<MachineObservationStateAssociation> (session, 5);
          // - AnalysisLogs
          AnalysisUnitTests.CheckNumberOfAnalysisLogs (session, 2); // No machine status found
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis when there is a new Machine Observation State
    /// at cut-off time
    /// </summary>
    [Test]
    public void TestNewMachineObservationStateAtCutOffTime()
    {
      IModelFactory modelFactory = ModelDAOHelper.ModelFactory;
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        try {
          Lemoine.Info.ConfigSet
            .ForceValue ("ReasonSlotDAO.FindProcessing.LowerLimit", TimeSpan.FromDays (20 * 365));
          ISession session = NHibernateHelper.GetCurrentSession ();
          // Reference data
          IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
          IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
          MonitoredMachine machine1 = session.Get<MonitoredMachine> (3);
          MachineObservationState attended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Attended);
          MachineObservationState unattended =
            session.Get<MachineObservationState> ((int)MachineObservationStateId.Unattended);
          Reason reasonMotion = session.Get<Reason> (2);
          Reason reasonShort = session.Get<Reason> (3);
          Reason reasonUnanswered = session.Get<Reason> (4);
          Reason reasonUnattended = session.Get<Reason> (5);
          MachineMode inactive = session.Get<MachineMode> (1);
          MachineMode active = session.Get<MachineMode> (2);
          MachineMode auto = session.Get<MachineMode> (3);

          // Existing ReasonSlot
          {
            ReasonSlot existingSlot =
              new ReasonSlot (machine1,
                              new UtcDateTimeRange (T (1), UtcDateTime.From (2011, 08, 01, 23, 00, 00)));
            existingSlot.MachineMode = inactive;
            existingSlot.MachineObservationState = unattended;
            ((ReasonSlot)existingSlot).SetDefaultReason (reasonUnattended, 10.0, false, true);
            ((ReasonSlot)existingSlot).Consolidate (null, null);
            session.Save (existingSlot);
          }
          // Existing MachineActivitySummary
          {
            IMachineActivitySummary summary;
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                 Day.From (2011, 08, 01),
                                                                                 unattended, inactive);
            summary.Time = TimeSpan.FromHours (20); // Note: cut-off of time is at 20:00 UTC, 22:00 Local
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
            summary = ModelDAOHelper.ModelFactory.CreateMachineActivitySummary (machine1,
                                                                                 Day.From (2011, 08, 02),
                                                                                 unattended, inactive);
            summary.Time = TimeSpan.FromHours (3);
            ModelDAOHelper.DAOFactory.MachineActivitySummaryDAO.MakePersistent (summary);
          }
          // Existing ReasonSummary
          {
            ReasonSummary summary;
            summary = new ReasonSummary (machine1,
                                          Day.From (2011, 08, 01), null,
                                          unattended, reasonUnattended);
            summary.Time = TimeSpan.FromHours (20);
            summary.Number = 1;
            session.Save (summary);
            summary = new ReasonSummary (machine1,
                                          Day.From (2011, 08, 02), null,
                                          unattended, reasonUnattended);
            summary.Time = TimeSpan.FromHours (3);
            summary.Number = 1;
            session.Save (summary);
          }
          // MachineStatus
          {
            MachineStatus machineStatus =
              new MachineStatus (machine1);
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.MachineObservationState = unattended;
            machineStatus.ManualActivity = false;
            machineStatus.Reason = reasonUnattended;
            machineStatus.ReasonSlotEnd =
              UtcDateTime.From (2011, 08, 01, 23, 00, 00);
            session.Save (machineStatus);
          }

          // New association 20:00 -> 02:00
          {
            IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine1, attended, UtcDateTime.From (2011, 08, 01, 20, 00, 00));
            association.User = user1;
            association.Shift = user1.Shift;
            association.DateTime = UtcDateTime.From (2011, 08, 01, 23, 10, 00);
            association.End = UtcDateTime.From (2011, 08, 02, 02, 00, 00);
            ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO
              .MakePersistent (association);
          }

          // Run MakeAnalysis
          {
            AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> (session);
            AnalysisUnitTests.RunProcessingReasonSlotsAnalysis (System.Threading.CancellationToken.None, machine1);
          }
          DAOFactory.EmptyAccumulators ();

          // Check the values
          {
            // - ObservationStateSlots
            IList<ObservationStateSlot> slots =
              session.CreateCriteria<ObservationStateSlot> ()
              .Add (Expression.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<ObservationStateSlot> ();
            Assert.AreEqual (3, slots.Count, "Number of observation state slots");
            int i = 1;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (user1, slots[i].User);
            Assert.AreEqual (shift3, slots[i].Shift); // Shift that is associated to the user
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 20, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 02, 02, 00, 00), slots[i].EndDateTime.Value);
          }
          {
            // - ReasonSlots
            IList<IReasonSlot> slots =
              session.CreateCriteria<IReasonSlot> ()
              .Add (Restrictions.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("DateTimeRange"))
              .List<IReasonSlot> ();
            Assert.AreEqual (2, slots.Count, "Number of ReasonSlots");
            int i = 0;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (unattended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, slots[i].Reason);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 20, 00, 00), slots[i].EndDateTime.Value);
            ++i;
            Assert.AreEqual (machine1, slots[i].Machine);
            Assert.AreEqual (inactive, slots[i].MachineMode);
            Assert.AreEqual (attended, slots[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, slots[i].Reason);
            Assert.AreEqual (true, slots[i].DefaultReason);
            Assert.AreEqual (true, slots[i].OverwriteRequired);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 20, 00, 00), slots[i].BeginDateTime.Value);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 23, 00, 00), slots[i].EndDateTime.Value);
          }
          {
            // - ReasonSummary
            IList<IReasonSummary> summaries =
              session.CreateCriteria<ReasonSummary> ()
              .Add (Restrictions.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("Day"))
              .List<IReasonSummary> ();
            Assert.AreEqual (2, summaries.Count, "Number of ReasonSummaries");
            int i = 0;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (Day.From (2011, 08, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnattended, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (20), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (Day.From (2011, 08, 02), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (reasonUnanswered, summaries[i].Reason);
            Assert.AreEqual (TimeSpan.FromHours (3), summaries[i].Time);
            Assert.AreEqual (1, summaries[i].Number);
          }
          {
            // - MachineActivitySummary
            IList<IMachineActivitySummary> summaries =
              session.CreateCriteria<MachineActivitySummary> ()
              .Add (Restrictions.Eq ("Machine", machine1))
              .AddOrder (Order.Asc ("Day"))
              .List<IMachineActivitySummary> ();
            Assert.AreEqual (2, summaries.Count, "Number of ReasonSummaries");
            int i = 0;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (Day.From (2011, 08, 01), summaries[i].Day);
            Assert.AreEqual (unattended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (TimeSpan.FromHours (20), summaries[i].Time);
            ++i;
            Assert.AreEqual (machine1, summaries[i].Machine);
            Assert.AreEqual (Day.From (2011, 08, 02), summaries[i].Day);
            Assert.AreEqual (attended, summaries[i].MachineObservationState);
            Assert.AreEqual (inactive, summaries[i].MachineMode);
            Assert.AreEqual (TimeSpan.FromHours (3), summaries[i].Time);
          }
          {
            // - MachineStatus
            IMachineStatus machineStatus =
              session.Get<MachineStatus> (machine1.Id);
            Assert.IsNotNull (machineStatus);
            Assert.AreEqual (attended, machineStatus.MachineObservationState);
            Assert.AreEqual (inactive, machineStatus.CncMachineMode);
            Assert.AreEqual (reasonUnanswered, machineStatus.Reason);
            Assert.AreEqual (ReasonSource.Default, machineStatus.ReasonSource);
            Assert.AreEqual (UtcDateTime.From (2011, 08, 01, 23, 00, 00), machineStatus.ReasonSlotEnd);
          }
          // - Modifications
          AnalysisUnitTests.CheckAllModificationDone<MachineObservationStateAssociation> (session, 2);
          // - AnalysisLogs
          AnalysisUnitTests.CheckNumberOfAnalysisLogs (session, 0);
        }
        finally {
          Lemoine.Info.ConfigSet.ResetForceValues ();
          transaction.Rollback ();
        }
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

      Lemoine.Plugin.ReasonDefaultManagement.ReasonDefaultManagement.Install ();
      Lemoine.Plugin.DefaultAccumulators.DefaultAccumulators.Install ();
    }
    
    [OneTimeTearDown]
    public void Dispose()
    {
      Lemoine.Extensions.ExtensionManager.ClearAdditionalExtensions ();

      if (previousDSNName != null) {
        System.Environment.SetEnvironmentVariable ("DefaultDSNName",
                                                   previousDSNName);
      }
    }
  }
}
