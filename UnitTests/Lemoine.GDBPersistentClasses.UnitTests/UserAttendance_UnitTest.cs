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
        Assert.AreEqual (2, slots.Count, "Number of slots");
        int i = 0;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].EndDateTime.Value);
        ++i;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 04), slots [i].BeginDateTime.Value);
        Assert.IsFalse (slots [i].EndDateTime.HasValue);
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.AreEqual (1, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
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
        Assert.AreEqual (1, slots.Count, "Number of slots");
        int i = 0;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
        Assert.IsFalse (slots [i].EndDateTime.HasValue);
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.AreEqual (1, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
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
        Assert.AreEqual (5, slots.Count, "Number of slots");
        int i = 0;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
        Assert.AreEqual (T(3), slots [i].EndDateTime.Value);
        Assert.AreEqual (shift2, slots [i].Shift);
        ++i;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (T(3), slots [i].BeginDateTime.Value);
        Assert.AreEqual (T(4), slots [i].EndDateTime.Value);
        Assert.AreEqual (shift1, slots [i].Shift);
        ++i;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (T(4), slots [i].BeginDateTime.Value);
        Assert.AreEqual (T(5), slots [i].EndDateTime.Value);
        Assert.AreEqual (shift3, slots [i].Shift);
        ++i;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (T(5), slots [i].BeginDateTime.Value);
        Assert.AreEqual (T(6), slots [i].EndDateTime.Value);
        Assert.AreEqual (shift2, slots [i].Shift);
        ++i;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (T(6), slots [i].BeginDateTime.Value);
        Assert.IsFalse (slots [i].EndDateTime.HasValue);
        Assert.AreEqual (shift1, slots [i].Shift);
        ++i;
        // TODO: test the machine observation states
        // - Modifications
        IList<IUserAttendance> modifications = ModelDAOHelper.DAOFactory.UserAttendanceDAO
          .FindAll ();
        Assert.AreEqual (1, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
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
        Assert.AreEqual (2, slots.Count, "Number of slots");
        int i = 0;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
        Assert.AreEqual (T(2), slots [i].EndDateTime.Value);
        Assert.AreEqual (shift2, slots [i].Shift);
        ++i;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (T(2), slots [i].BeginDateTime.Value);
        Assert.IsFalse (slots [i].EndDateTime.HasValue);
        Assert.AreEqual (shift3, slots [i].Shift);
        ++i;
        // TODO: test the machine observation states
        // - Modifications
        IList<IUserAttendance> modifications = ModelDAOHelper.DAOFactory.UserAttendanceDAO
          .FindAll ();
        Assert.AreEqual (1, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
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
          Assert.AreEqual (1, slots.Count, "Number of slots");
          int i = 0;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(2), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        { // - Modifications
          IList<IUserAttendance> modifications = ModelDAOHelper.DAOFactory.UserAttendanceDAO
            .FindAll ();
          Assert.AreEqual (1, modifications.Count, "Number of modifications");
          Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
          Assert.IsTrue (modifications[0].AnalysisSubModifications);
        }
        { // - AnalysisLogs
          AnalysisUnitTests.CheckNumberOfAnalysisLogs (0);
        }
        { // - ObservationStateSlot
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindByUserInRange (user1, new UtcDateTimeRange (T(1)));
          Assert.AreEqual (2, slots.Count);
          int i = 0;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(2), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(3), slots [i].EndDateTime.Value);
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attended, slots [i].MachineStateTemplate);
          ++i;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(2), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(3), slots [i].EndDateTime.Value);
          Assert.AreEqual (machine2, slots [i].Machine);
          Assert.AreEqual (onSite, slots [i].MachineStateTemplate);
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
        Assert.AreEqual (1, slots.Count, "Number of slots");
        int i = 0;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 03), slots [i].EndDateTime.Value);
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.AreEqual (1, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
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
          Assert.AreEqual (1, slots.Count, "Number of user slots");
          int i = 0;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(2), slots [i].EndDateTime.Value);
        }
        {
          // - ObservationStateSlots
          IList<ObservationStateSlot> slots =
            session.CreateCriteria<ObservationStateSlot> ()
            .Add (Expression.Eq ("Machine", machine1))
            .AddOrder (Order.Asc ("DateTimeRange"))
            .List<ObservationStateSlot> ();
          Assert.AreEqual (3, slots.Count, "Number of observation state slots");
          int i = 1;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (attended, slots [i].MachineStateTemplate);
          Assert.AreEqual (mosAttended, slots [i].MachineObservationState);
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (shift3, slots [i].Shift);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(2), slots [i].EndDateTime.Value);
          ++i;
          Assert.AreEqual (machine1, slots [i].Machine);
          Assert.AreEqual (unattended, slots [i].MachineStateTemplate);
          Assert.AreEqual (mosUnattended, slots [i].MachineObservationState);
          Assert.AreEqual (null, slots [i].User);
          Assert.AreEqual (null, slots [i].Shift);
          Assert.AreEqual (T(2), slots [i].BeginDateTime.Value);
          Assert.IsFalse (slots [i].EndDateTime.HasValue);
        }
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.AreEqual (1, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
        Assert.IsTrue (modifications[0].AnalysisSubModifications);
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
        Assert.AreEqual (1, slots.Count, "Number of slots");
        int i = 0;
        Assert.AreEqual (user1, slots [i].User);
        Assert.AreEqual (UtcDateTime.From (2011, 08, 01), slots [i].BeginDateTime.Value);
        Assert.IsFalse (slots [i].EndDateTime.HasValue);
        ++i;
        // - Modifications
        IList<UserAttendance> modifications =
          session.CreateCriteria<UserAttendance> ()
          .AddOrder (Order.Asc ("DateTime"))
          .List<UserAttendance> ();
        Assert.AreEqual (1, modifications.Count, "Number of modifications");
        Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
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
