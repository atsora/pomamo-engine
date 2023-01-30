// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.UnitTests;
using Lemoine.Core.Log;
using NUnit.Framework;

namespace Lemoine.GDBPersistentClasses.UnitTests
{

  /// <summary>
  /// Unit tests for the class UserShiftAssociation.
  /// </summary>
  [TestFixture]
  public class UserShiftAssociation_UnitTest: WithDayTimeStamp
  {
    string previousDSNName;
    
    static readonly ILog log = LogManager.GetLogger(typeof (UserShiftAssociation_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public UserShiftAssociation_UnitTest ()
      : base (UtcDateTime.From (2013, 07, 31))
    {
    }

    /// <summary>
    /// Test the method MakeAnalysis
    /// 
    ///                  1      2      3      4      5      6      oo
    /// UserSlots        |-s2----------|
    /// UserShiftSlots   |-s2----------|      |-s3---|-s2---|
    /// MOS              |-s2----------|
    /// Association             |-s3----------|
    /// -- Result --
    /// UserShiftSlots   |-s2---|-s3-----------------|-s2---|
    /// UserSlots        |-s2---|-s3---|
    /// MOS              |-s2---|-s3---|
    /// </summary>
    [Test]
    public void TestMakeAnalysis()
    {
      IDAOFactory daoFactory = ModelDAOHelper.DAOFactory;
      using (IDAOSession daoSession = daoFactory.OpenSession ())
        using (IDAOTransaction transaction = daoSession.BeginTransaction ())
      {
        // Reference data
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO.FindById (1);
        IUser user1 = ModelDAOHelper.DAOFactory.UserDAO.FindById (1);
        IShift shift1 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (1);
        IShift shift2 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (2);
        IShift shift3 = ModelDAOHelper.DAOFactory.ShiftDAO.FindById (3);
        user1.Shift = shift1;
        ModelDAOHelper.DAOFactory.UserDAO.MakePersistent (user1);
        IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO.FindById (1);
        
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
            .CreateUserShiftSlot (user1, R(1, 3), shift2);
          ModelDAOHelper.DAOFactory.UserShiftSlotDAO
            .MakePersistent (existingUserShiftSlot);
        }
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
        // Existing ObservationStateSlots
        {
          IMachineObservationStateAssociation association = ModelDAOHelper.ModelFactory
            .CreateMachineObservationStateAssociation (machine, attended, T(1));
          association.End = T(3);
          association.User = user1;
          association.Shift = shift2;
          ModelDAOHelper.DAOFactory.MachineObservationStateAssociationDAO.MakePersistent (association);
          AnalysisUnitTests.RunMakeAnalysis<MachineObservationStateAssociation> ();
        }
        
        // New association 2 -> oo
        {
          IUserShiftAssociation association = ModelDAOHelper.ModelFactory
            .CreateUserShiftAssociation (user1, R(2, 4), shift3);
          ModelDAOHelper.DAOFactory.UserShiftAssociationDAO
            .MakePersistent (association);
        }
        
        // Run MakeAnalysis
        {
          AnalysisUnitTests.RunMakeAnalysis<UserShiftAssociation> ();
        }
        
        // Check the values
        // - UserSlots
        {
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
          Assert.AreEqual (T(3), slots [i].EndDateTime.Value);
          Assert.AreEqual (shift3, slots [i].Shift);
          ++i;
        }
        // - UserShiftSlots
        {
          IList<IUserShiftSlot> slots = ModelDAOHelper.DAOFactory.UserShiftSlotDAO
            .FindAll (user1);
          Assert.AreEqual (3, slots.Count, "Number of slots");
          int i = 0;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(2), slots [i].EndDateTime.Value);
          Assert.AreEqual (shift2, slots [i].Shift);
          ++i;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(2), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(5), slots [i].EndDateTime.Value);
          Assert.AreEqual (shift3, slots [i].Shift);
          ++i;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(5), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(6), slots [i].EndDateTime.Value);
          Assert.AreEqual (shift2, slots [i].Shift);
          ++i;
        }
        // - ObservationStateSlots
        {
          IList<IObservationStateSlot> slots = ModelDAOHelper.DAOFactory.ObservationStateSlotDAO
            .FindByUserInRange (user1, new UtcDateTimeRange (T(1)));
          Assert.AreEqual (2, slots.Count, "Number of slots");
          int i = 0;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(1), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(2), slots [i].EndDateTime.Value);
          Assert.AreEqual (shift2, slots [i].Shift);
          ++i;
          Assert.AreEqual (user1, slots [i].User);
          Assert.AreEqual (T(2), slots [i].BeginDateTime.Value);
          Assert.AreEqual (T(3), slots [i].EndDateTime.Value);
          Assert.AreEqual (shift3, slots [i].Shift);
          ++i;
        }
        // - Modifications
        {
          IList<IUserShiftAssociation> modifications = ModelDAOHelper.DAOFactory.UserShiftAssociationDAO
            .FindAll ();
          Assert.AreEqual (1, modifications.Count, "Number of modifications");
          Assert.AreEqual (AnalysisStatus.Done, modifications[0].AnalysisStatus, "1st modification status");
          Assert.IsTrue (modifications[0].AnalysisSubModifications);
        }
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
