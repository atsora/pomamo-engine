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
  /// Unit tests for the class UserAttendance
  /// </summary>
  [TestFixture]
  public class UserAttendance_UnitTest: WithDayTimeStamp
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (UserAttendance_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UserAttendance_UnitTest ()
      : base (UtcDateTime.From (2011, 07, 31))
    {
    }

    /// <summary>
    /// Test the method MakeAnalysis - case clock-in
    /// </summary>
    [Test]
    public void TestMakeAnalysisClockIn1()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();

        // Reference data
        User user1 = session.Get<User> (1);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
        
        // Existing user slots
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(1, 3));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(4, 5));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(6, null));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        
        // New association 4 -> oo
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = UtcDateTime.From (2011, 08, 05);
          userAttendance.Begin = UtcDateTime.From (2011, 08, 04);
          userAttendance.End = null;
          ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .MakePersistent (userAttendance);
        }
        
        // Run MakeAnalysis
        IList<UserAttendance> userAttendances =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        foreach (UserAttendance userAttendance
                 in userAttendances ) {
          userAttendance.MakeAnalysis ();
        }
        
        // Check the values
        // - UserSlots
        IList<UserSlot> slots =
          session.CreateCriteria<UserSlot> ()
          .Add (Expression.Eq ("User", user1))
          .AddOrder (Order.Asc ("BeginDateTime"))
          .List<UserSlot> ();
        Assert.That (slots, Has.Count.EqualTo (2), "Number of slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 04)));
          Assert.That (slots[i].EndDateTime.HasValue, Is.False);
        });
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
        Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis - case clock-in
    /// </summary>
    [Test]
    public void TestMakeAnalysisClockIn2()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();

        // Reference data
        User user1 = session.Get<User> (1);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
        
        // Existing user slots
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(1, 3));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(4, 5));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(6, null));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        
        // New association 2 -> oo
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = UtcDateTime.From (2011, 08, 05);
          userAttendance.Begin = UtcDateTime.From (2011, 08, 02);
          userAttendance.End = null;
          ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .MakePersistent (userAttendance);
        }
        
        // Run MakeAnalysis
        IList<UserAttendance> userAttendances =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        foreach (UserAttendance userAttendance
                 in userAttendances ) {
          userAttendance.MakeAnalysis ();
        }
        
        // Check the values
        // - UserSlots
        IList<UserSlot> slots =
          session.CreateCriteria<UserSlot> ()
          .Add (Expression.Eq ("User", user1))
          .AddOrder (Order.Asc ("BeginDateTime"))
          .List<UserSlot> ();
        Assert.That (slots, Has.Count.EqualTo (1), "Number of slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (slots[i].EndDateTime.HasValue, Is.False);
        });
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
        Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis - case clock-in
    /// with UserShiftSlot
    /// 
    ///                  1      2      3      4      5      6      oo
    /// UserSlots        |-s2----------|
    /// UserShiftSlots                        |-s3---|-s2---|
    /// Association             |-u1--------------------------------
    /// Result           |-s2----------|-s1---|-s3---|-s2---|-s1----
    /// </summary>
    [Test]
    public void TestMakeAnalysisClockIn3()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
        user1.Shift = shift1;
        ModelDAOHelper.DAOFactory.UserDAO.MakePersistent (user1);
        
        // Existing user slots
        {
          IUserSlot existingUserSlot = ModelDAOHelper.ModelFactory
            .CreateUserSlot (user1, R(1, 3));
          existingUserSlot.Shift = shift2;
          ModelDAOHelper.DAOFactory.UserSlotDAO
            .MakePersistent (existingUserSlot);
        }
        // Existing UserShiftSlots
        {
          IUserShiftSlot existingUserShiftSlot = ModelDAOHelper.ModelFactory
            .CreateUserShiftSlot (user1, R(4, 5), shift3);
          ModelDAOHelper.DAOFactory.UserShiftSlotDAO
            .MakePersistent (existingUserShiftSlot);
        }
        {
          IUserShiftSlot existingUserShiftSlot = ModelDAOHelper.ModelFactory
            .CreateUserShiftSlot (user1, R(5, 6), shift2);
          ModelDAOHelper.DAOFactory.UserShiftSlotDAO
            .MakePersistent (existingUserShiftSlot);
        }
        
        // New association 2 -> oo
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = T(3);
          userAttendance.Begin = T(2);
          userAttendance.End = null;
          ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .MakePersistent (userAttendance);
        }
        
        // Run MakeAnalysis
        IList<IUserAttendance> userAttendances = ModelDAOHelper.DAOFactory.UserAttendanceDAO
          .FindAll ();
        foreach (IUserAttendance userAttendance
                 in userAttendances) {
          ((UserAttendance)userAttendance).MakeAnalysis ();
        }
        
        // Check the values
        // - UserSlots
        IList<IUserSlot> slots = ModelDAOHelper.DAOFactory.UserSlotDAO
          .FindAll (user1);
        Assert.That (slots, Has.Count.EqualTo (5), "Number of slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
          Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
          Assert.That (slots[i].Shift, Is.EqualTo (shift2));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (3)));
          Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (4)));
          Assert.That (slots[i].Shift, Is.EqualTo (shift1));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (4)));
          Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (5)));
          Assert.That (slots[i].Shift, Is.EqualTo (shift3));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (5)));
          Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (6)));
          Assert.That (slots[i].Shift, Is.EqualTo (shift2));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (6)));
          Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          Assert.That (slots[i].Shift, Is.EqualTo (shift1));
        });
        ++i;
        // TODO: test the machine observation states
        // - Modifications
        IList<IUserAttendance> modifications = ModelDAOHelper.DAOFactory.UserAttendanceDAO
          .FindAll ();
        Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
        Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis - case clock-in
    /// with UserShiftSlot
    /// 
    ///                  1      2      3      4      5      6      oo
    /// UserSlots        |-s2----------|
    /// UserShiftSlots                        |-s3---|-s2---|
    /// Association             |-u1/s3-----------------------------
    /// Result           |-s2---|-s3--------------------------------
    /// </summary>
    [Test]
    public void TestMakeAnalysisClockIn4()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
        user1.Shift = shift1;
        ModelDAOHelper.DAOFactory.UserDAO.MakePersistent (user1);
        
        // Existing user slots
        {
          IUserSlot existingUserSlot = ModelDAOHelper.ModelFactory
            .CreateUserSlot (user1, R(1, 3));
          existingUserSlot.Shift = shift2;
          ModelDAOHelper.DAOFactory.UserSlotDAO
            .MakePersistent (existingUserSlot);
        }
        // Existing UserShiftSlots
        {
          IUserShiftSlot existingUserShiftSlot = ModelDAOHelper.ModelFactory
            .CreateUserShiftSlot (user1, R(4, 5), shift3);
          ModelDAOHelper.DAOFactory.UserShiftSlotDAO
            .MakePersistent (existingUserShiftSlot);
        }
        {
          IUserShiftSlot existingUserShiftSlot = ModelDAOHelper.ModelFactory
            .CreateUserShiftSlot (user1, R(5, 6), shift2);
          ModelDAOHelper.DAOFactory.UserShiftSlotDAO
            .MakePersistent (existingUserShiftSlot);
        }
        
        // New association 2 -> oo
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = T(3);
          userAttendance.Begin = T(2);
          userAttendance.End = null;
          userAttendance.Shift = shift3;
          ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .MakePersistent (userAttendance);
        }
        
        // Run MakeAnalysis
        IList<IUserAttendance> userAttendances = ModelDAOHelper.DAOFactory.UserAttendanceDAO
          .FindAll ();
        foreach (IUserAttendance userAttendance
                 in userAttendances) {
          ((UserAttendance)userAttendance).MakeAnalysis ();
        }
        
        // Check the values
        // - UserSlots
        IList<IUserSlot> slots = ModelDAOHelper.DAOFactory.UserSlotDAO
          .FindAll (user1);
        Assert.That (slots, Has.Count.EqualTo (2), "Number of slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
          Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (2)));
          Assert.That (slots[i].Shift, Is.EqualTo (shift2));
        });
        ++i;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
          Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          Assert.That (slots[i].Shift, Is.EqualTo (shift3));
        });
        ++i;
        // TODO: test the machine observation states
        // - Modifications
        IList<IUserAttendance> modifications = ModelDAOHelper.DAOFactory.UserAttendanceDAO
          .FindAll ();
        Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
        Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis - case clock-in
    /// with a UserMachineSlot
    /// </summary>
    [Test]
    public void TestMakeAnalysisClockInUserMachineSlot()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IMachine machine1 = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
        IMachine machine2 = ModelDAOHelper.DAOFactory.MachineDAO.FindById (2);
        IMachineStateTemplate attended = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (1);
        IMachineStateTemplate onSite = ModelDAOHelper.DAOFactory.MachineStateTemplateDAO
          .FindById (2);
        
        // Add a UserMachineAssociation
        {
          IUserMachineAssociation association = ModelDAOHelper.ModelFactory
            .CreateUserMachineAssociation (user1, R(1, 3));
          association.DateTime = T(1);
          association.Add (machine1, attended);
          association.Add (machine2, onSite);
          ModelDAOHelper.DAOFactory.UserMachineAssociationDAO
            .MakePersistent (association);
          ((UserMachineAssociation)association).MakeAnalysis ();
        }
        
        // New association 2 -> oo
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = T(2);
          userAttendance.Begin = T(2);
          userAttendance.End = null;
          ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .MakePersistent (userAttendance);
          AnalysisUnitTests.RunMakeAnalysis<UserAttendance> ();
        }
        
        // Check the values
        { // - UserSlots
          IList<IUserSlot> slots = ModelDAOHelper.DAOFactory.UserSlotDAO
            .FindOverlapsRange (user1, new UtcDateTimeRange (T(1)));
          Assert.That (slots, Has.Count.EqualTo (1), "Number of slots");
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        { // - Modifications
          IList<IUserAttendance> modifications = ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .FindAll ();
          Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
          Assert.Multiple (() => {
            Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
            Assert.That (modifications[0].AnalysisSubModifications, Is.True);
          });
        }
        { // - AnalysisLogs
          AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        }
        { // - ObservationStateSlot
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindByUserInRange (user1, new UtcDateTimeRange (T(1)));
          Assert.That (slots, Has.Count.EqualTo (2));
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (attended));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (3)));
            Assert.That (slots[i].Machine, Is.EqualTo (machine2));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (onSite));
          });
        }
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis - case clock-out
    /// </summary>
    [Test]
    public void TestMakeAnalysisClockOut1()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();

        // Reference data
        User user1 = session.Get<User> (1);
        
        // Existing user slots
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(1, 3));
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(4, 5));
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(6, null));
          session.Save (existingSlot);
        }
        
        // New association 4 -> oo
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = UtcDateTime.From (2011, 08, 05);
          userAttendance.Begin = null;
          userAttendance.End = UtcDateTime.From (2011, 08, 04);
          ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .MakePersistent (userAttendance);
        }
        
        // Run MakeAnalysis
        IList<UserAttendance> userAttendances =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        foreach (UserAttendance userAttendance
                 in userAttendances ) {
          userAttendance.MakeAnalysis ();
        }
        
        // Check the values
        // - UserSlots
        IList<UserSlot> slots =
          session.CreateCriteria<UserSlot> ()
          .Add (Expression.Eq ("User", user1))
          .AddOrder (Order.Asc ("BeginDateTime"))
          .List<UserSlot> ();
        Assert.That (slots, Has.Count.EqualTo (1), "Number of slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 03)));
        });
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
        Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (3);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis - case clock-out
    /// 
    /// Check also the ObservationStateSlot is updated accordingly
    /// </summary>
    [Test]
    public void TestMakeAnalysisClockOut2()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();

        // Reference data
        IUser user1 = daoFactory.UserDAO.FindById (1);
        IMachine machine1 = daoFactory.MachineDAO.FindById (3);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
        IMachineObservationState mosAttended = daoFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Attended);
        IMachineObservationState mosUnattended = daoFactory.MachineObservationStateDAO
          .FindById ((int) MachineObservationStateId.Unattended);
        IMachineStateTemplate attended = daoFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Attended);
        IMachineStateTemplate unattended = daoFactory.MachineStateTemplateDAO
          .FindById ((int) StateTemplate.Unattended);
        
        // Existing user slots
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(1, 3));
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(4, 5));
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(6, null));
          session.Save (existingSlot);
        }
        // Existing MachineStateTemplate
        {
          IMachineStateTemplateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineStateTemplateAssociation (machine1, attended, T(1));
          association.End = T(3);
          association.User = user1;
          association.Shift = shift3;
          ((MachineStateTemplateAssociation)association).Apply ();
        }
        Lemoine.GDBPersistentClasses.UnitTests.AnalysisUnitTests.RunMachineStateTemplateAnalysis (machine1);
        
        // New association 2 -> oo (clock out)
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = T(5);
          userAttendance.Begin = null;
          userAttendance.End = T(2);
          ModelDAOHelper.DAOFactory.UserAttendanceDAO.MakePersistent (userAttendance);
        }
        
        // Run MakeAnalysis
        AnalysisUnitTests.RunMakeAnalysis<UserAttendance> ();
        AnalysisUnitTests.RunMachineStateTemplateAnalysis (machine1);
        
        // Check the values
        {
          // - UserSlots
          IList<UserSlot> slots =
            session.CreateCriteria<UserSlot> ()
            .Add (Expression.Eq ("User", user1))
            .AddOrder (Order.Asc ("BeginDateTime"))
            .List<UserSlot> ();
          Assert.That (slots, Has.Count.EqualTo (1), "Number of user slots");
          int i = 0;
          Assert.Multiple (() => {
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (2)));
          });
        }
        {
          // - ObservationStateSlots
          IList<ObservationStateSlot> slots =
            session.CreateCriteria<ObservationStateSlot> ()
            .Add (Expression.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<ObservationStateSlot> ();
          Assert.That (slots, Has.Count.EqualTo (3), "Number of observation state slots");
          int i = 1;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (attended));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (mosAttended));
            Assert.That (slots[i].User, Is.EqualTo (user1));
            Assert.That (slots[i].Shift, Is.EqualTo (shift3));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (1)));
            Assert.That (slots[i].EndDateTime.Value, Is.EqualTo (T (2)));
          });
          ++i;
          Assert.Multiple (() => {
            Assert.That (slots[i].Machine, Is.EqualTo (machine1));
            Assert.That (slots[i].MachineStateTemplate, Is.EqualTo (unattended));
            Assert.That (slots[i].MachineObservationState, Is.EqualTo (mosUnattended));
            Assert.That (slots[i].User, Is.EqualTo (null));
            Assert.That (slots[i].Shift, Is.EqualTo (null));
            Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (T (2)));
            Assert.That (slots[i].EndDateTime.HasValue, Is.False);
          });
        }
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
        Assert.Multiple (() => {
          Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
          Assert.That (modifications[0].AnalysisSubModifications, Is.True);
        });
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (2);
        
        transaction.Rollback ();
      }
    }
    
    /// <summary>
    /// Test the method MakeAnalysis - case period
    /// </summary>
    [Test]
    public void TestMakeAnalysisPeriod()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        ISession session = NHibernateHelper.GetCurrentSession ();
        
        // Reference data
        User user1 = session.Get<User> (1);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
        
        // Existing user slots
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(1, 3));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(4, 5));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        {
          UserSlot existingSlot =
            new UserSlot (user1,
                          R(6, null));
          existingSlot.Shift = shift3;
          session.Save (existingSlot);
        }
        
        // New association 2 -> 7
        {
          IUserAttendance userAttendance = ModelDAOHelper.ModelFactory
            .CreateUserAttendance (user1);
          userAttendance.DateTime = UtcDateTime.From (2011, 08, 05);
          userAttendance.Begin = UtcDateTime.From (2011, 08, 02);
          userAttendance.End = UtcDateTime.From (2011, 08, 07);
          ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .MakePersistent (userAttendance);
        }
        
        // Run MakeAnalysis
        IList<UserAttendance> userAttendances =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        foreach (UserAttendance userAttendance
                 in userAttendances ) {
          userAttendance.MakeAnalysis ();
        }
        
        // Check the values
        // - UserSlots
        IList<UserSlot> slots =
          session.CreateCriteria<UserSlot> ()
          .Add (Expression.Eq ("User", user1))
          .AddOrder (Order.Asc ("BeginDateTime"))
          .List<UserSlot> ();
        Assert.That (slots, Has.Count.EqualTo (1), "Number of slots");
        int i = 0;
        Assert.Multiple (() => {
          Assert.That (slots[i].User, Is.EqualTo (user1));
          Assert.That (slots[i].BeginDateTime.Value, Is.EqualTo (UtcDateTime.From (2011, 08, 01)));
          Assert.That (slots[i].EndDateTime.HasValue, Is.False);
        });
        ++i;
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.That (modifications, Has.Count.EqualTo (1), "Number of modifications");
        Assert.That (modifications[0].AnalysisStatus, Is.EqualTo (AnalysisStatus.Done), "1st modification status");
        // - AnalysisLogs
        AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        
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
