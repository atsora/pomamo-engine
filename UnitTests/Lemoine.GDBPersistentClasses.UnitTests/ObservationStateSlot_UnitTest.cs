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
          Assert.AreEqual (3, slots.Count, "Number of observation state slots");
          ((ObservationStateSlot)slots [1]).ProcessTemplate (System.Threading.CancellationToken.None, new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                                   T(13)),
                                                             null, false, null, null);
        }

        // Check the values
        {
          // - ObservationStateSlots
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T(0)));
          Assert.AreEqual (10, slots.Count, "Number of observation state slots");
          int i = 1;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (new DateTime (2014, 10, 03, 12, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (new DateTime (2014, 10, 03, 12, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].BeginDateTime.Value);
          Assert.AreEqual (new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (new DateTime (2014, 10, 06, 00, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].BeginDateTime.Value);
          Assert.AreEqual (new DateTime (2014, 10, 08, 06, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (new DateTime (2014, 10, 08, 06, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].BeginDateTime.Value);
          Assert.AreEqual (new DateTime (2014, 10, 08, 18, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (new DateTime (2014, 10, 08, 18, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].BeginDateTime.Value);
          Assert.AreEqual (new DateTime (2014, 10, 10, 12, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (new DateTime (2014, 10, 10, 12, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].BeginDateTime.Value);
          Assert.AreEqual (new DateTime (2014, 10, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (new DateTime (2014, 10, 13, 00, 00, 00, DateTimeKind.Local).ToUniversalTime (),
                           slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(13),
                           slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (null, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (T(13),
                           slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(15),
                           slots [i].EndDateTime.Value);
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
          Assert.AreEqual (3, slots.Count, "Number of observation state slots");
          ((ObservationStateSlot)slots [1]).ProcessTemplate (System.Threading.CancellationToken.None, new UtcDateTimeRange (new LowerBound<DateTime> (null),
                                                                                   T(25)),
                                                             null, false, null, null);
        }

        // Check the values
        {
          // - ObservationStateSlots
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindOverlapsRange (machine1, new UtcDateTimeRange (T(0)));
          Assert.AreEqual (3, slots.Count, "Number of observation state slots");
          int i = 1;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (mst, slots [i].MachineStateTemplate);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift2, slots [i].Shift);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(20), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattendedMOS, slots [i].MachineObservationState);
          Assert.AreEqual (unattended, slots [i].MachineStateTemplate);
          Assert.AreEqual (null, slots [i].User);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (T(20), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
          ++i;
        }
        
        transaction.Rollback ();
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
