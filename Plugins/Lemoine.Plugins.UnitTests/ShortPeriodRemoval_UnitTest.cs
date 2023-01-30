// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Extensions.Analysis;
using Lemoine.Model;
using Lemoine.ModelDAO;
using NUnit.Framework;
using Lemoine.Core.Log;
using Pulse.Extensions.Extension;

namespace Lemoine.Plugins.UnitTests
{
  /// <summary>
  /// Description of ShortPeriodRemoval_UnitTest.
  /// </summary>
  public class ShortPeriodRemoval_UnitTest
    : Lemoine.UnitTests.WithSecondTimeStamp
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ShortPeriodRemoval_UnitTest).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public ShortPeriodRemoval_UnitTest ()
      : base (new DateTime (2016, 04, 01, 00, 00, 00, DateTimeKind.Utc))
    { }
    
    [Test]
    public void TestShortInactivity ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById (1);
          IMachineObservationState production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById (9);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Inactive);
          IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Active);
          
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, R(-100, null));
            association.Apply ();
          }
          
          {
            var extension = new Lemoine.Plugin.ShortPeriodRemoval
              .ActivityAnalysisExtension ();
            extension.SetTestConfiguration (string.Format (@"
{{
  ""MaxDuration"": ""0:00:10"",
  ""OldMachineModeId"": {0},
  ""NewMachineModeId"": {1}
}}
",
                                                           (int)MachineModeId.Inactive,
                                                           (int)MachineModeId.Active));
            extension.Initialize (machine);
            
            extension.BeforeProcessingNewActivityPeriod (null);
            AddReasonSlot (machine, R(0, 100), active, attended);
            extension.AfterProcessingNewActivityPeriod (R(0,100),
                                                        active, null, attended, null);
            
            IMachineStatus machineStatus = ModelDAOHelper.ModelFactory
              .CreateMachineStatus (machine);
            machineStatus.CncMachineMode = active;
            machineStatus.MachineMode = active;
            machineStatus.ReasonSlotEnd = T(100);
            extension.BeforeProcessingNewActivityPeriod (machineStatus);
            AddReasonSlot (machine, R(100, 110), inactive, attended);
            extension.AfterProcessingNewActivityPeriod (R(100,110),
                                                        inactive, null, attended, null);
            
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.ReasonSlotEnd = T(110);
            extension.BeforeProcessingNewActivityPeriod (machineStatus);
            AddReasonSlot (machine, R(110, 120), active, attended);
            extension.AfterProcessingNewActivityPeriod (R(110,120),
                                                        active, null, attended, null);
            extension.BeforeActivitiesCommit ();
            
            
            {
              IList<IActivityManual> modifications = ModelDAOHelper.DAOFactory.ActivityManualDAO
                .FindAll ();
              Assert.AreEqual (1, modifications.Count);
              Assert.AreEqual (R(100, 110), modifications[0].Range);
              Assert.AreEqual (active, modifications[0].MachineMode);
            }
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    [Test]
    public void TestShortInactivity2 ()
    {
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
        using (IDAOTransaction transaction = session.BeginTransaction ())
      {
        try {
          // Reference data
          IMonitoredMachine machine = ModelDAOHelper.DAOFactory.MonitoredMachineDAO
            .FindById (2);
          ModelDAOHelper.DAOFactory.MonitoredMachineDAO.MakePersistent (machine);
          IMachineObservationState attended = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById (1);
          IMachineObservationState production = ModelDAOHelper.DAOFactory.MachineObservationStateDAO
            .FindById (9);
          IMachineMode inactive = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Inactive);
          IMachineMode notReady = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.NotReady);
          IMachineMode active = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Active);
          IMachineMode autoFeed = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.AutoFeed);
          IMachineMode machining = ModelDAOHelper.DAOFactory.MachineModeDAO
            .FindById ((int)Lemoine.Model.MachineModeId.Machining);
          
          {
            var association = ModelDAOHelper.ModelFactory
              .CreateMachineObservationStateAssociation (machine, attended, R(-100, null));
            association.Apply ();
          }
          
          {
            var extension = new Lemoine.Plugin.ShortPeriodRemoval
              .ActivityAnalysisExtension ();
            extension.SetTestConfiguration (string.Format (@"
{{
  ""MaxDuration"": ""0:00:10"",
  ""OldMachineModeId"": {0},
  ""NewMachineModeId"": {1}
}}
",
                                                           (int)MachineModeId.Inactive,
                                                           (int)MachineModeId.Active));
            extension.Initialize (machine);
            
            extension.BeforeProcessingNewActivityPeriod (null);
            AddReasonSlot (machine, R(0, 100), autoFeed, attended);
            extension.AfterProcessingNewActivityPeriod (R(0,100),
                                                        autoFeed, null, attended, null);
            
            IMachineStatus machineStatus = ModelDAOHelper.ModelFactory
              .CreateMachineStatus (machine);
            machineStatus.CncMachineMode = autoFeed;
            machineStatus.MachineMode = active;
            machineStatus.ReasonSlotEnd = T(100);
            extension.BeforeProcessingNewActivityPeriod (machineStatus);
            AddReasonSlot (machine, R(100, 110), inactive, attended);
            extension.AfterProcessingNewActivityPeriod (R(100,110),
                                                        inactive, null, attended, null);
            
            machineStatus.CncMachineMode = inactive;
            machineStatus.MachineMode = inactive;
            machineStatus.ReasonSlotEnd = T(110);
            extension.BeforeProcessingNewActivityPeriod (machineStatus);
            AddReasonSlot (machine, R(110, 120), machining, attended);
            extension.AfterProcessingNewActivityPeriod (R(110,120),
                                                        machining, null, attended, null);
            extension.BeforeActivitiesCommit ();
            
            
            {
              IList<IActivityManual> modifications = ModelDAOHelper.DAOFactory.ActivityManualDAO
                .FindAll ();
              Assert.AreEqual (1, modifications.Count);
              Assert.AreEqual (R(100, 110), modifications[0].Range);
              Assert.AreEqual (active, modifications[0].MachineMode);
            }
          }
        }
        finally {
          transaction.Rollback ();
        }
      }
    }
    
    void AddReasonSlot (IMachine machine, UtcDateTimeRange range, IMachineMode machineMode, IMachineObservationState machineObservationState)
    {
      IReasonSlot reasonSlot = ModelDAOHelper.ModelFactory.CreateReasonSlot (machine, range);
      reasonSlot.MachineMode = machineMode;
      reasonSlot.MachineObservationState = machineObservationState;
      ((Lemoine.GDBPersistentClasses.ReasonSlot)reasonSlot).Consolidate (null, null);
      ModelDAOHelper.DAOFactory.ReasonSlotDAO.MakePersistent (reasonSlot);
    }
  }
}
