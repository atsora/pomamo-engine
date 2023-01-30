// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Lemoine.Database.Persistent;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using NHibernate;
using NHibernate.Criterion;

namespace Lemoine.GDBPersistentClasses
{
  /// <summary>
  /// Implementation of <see cref="Lemoine.ModelDAO.IMachineModeDAO">IMachineModeDAO</see>
  /// </summary>
  public class MachineModeDAO
    : VersionableNHibernateDAO<MachineMode, IMachineMode, int>
    , IMachineModeDAO
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineModeDAO).FullName);
    
    #region DefaultValues
    /// <summary>
    /// Insert the default values
    /// </summary>
    internal void InsertDefaultValues ()
    {
      // Inactive
      IMachineMode inactive = new MachineMode ((int)MachineModeId.Inactive, "MachineModeInactive",
                                               false, MachineModeCategoryId.Inactive, null);
      inactive.AutoSequence = false;
      inactive.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (inactive);
      // Active
      IMachineMode active = new MachineMode ((int)MachineModeId.Active, "MachineModeActive",
                                             true, MachineModeCategoryId.Active, null);
      active.AutoSequence = true;
      active.Color = "#008000"; // Green
      InsertDefaultValue (active);
      // Unknown
      IMachineMode unknown = new MachineMode ((int)MachineModeId.Unknown, "MachineModeUnknown",
                                              null, MachineModeCategoryId.Unknown, null);
      unknown.AutoSequence = false;
      unknown.Color = "#C0C0C0"; // Silver
      InsertDefaultValue (unknown);
      // InactiveOn
      IMachineMode inactiveOn = new MachineMode ((int)MachineModeId.InactiveOn, "MachineModeInactiveOn",
                                                 false, MachineModeCategoryId.Inactive, inactive);
      inactiveOn.AutoSequence = false;
      inactiveOn.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (inactiveOn);
      // AutoInactive
      IMachineMode autoInactive = new MachineMode ((int)MachineModeId.AutoInactive, "MachineModeAutoInactive",
                                                   false, MachineModeCategoryId.Inactive, inactiveOn);
      autoInactive.AutoSequence = false;
      autoInactive.Auto = true;
      autoInactive.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (autoInactive);
      // AutoActive
      IMachineMode autoActive = new MachineMode ((int)MachineModeId.AutoActive, "MachineModeAutoActive",
                                                 true, MachineModeCategoryId.Active, active);
      autoActive.Auto = true;
      autoActive.AutoSequence = true;
      autoActive.Color = "#008000"; // Green
      InsertDefaultValue (autoActive);
      // AutoNormalActive
      IMachineMode autoNormalActive = new MachineMode ((int)MachineModeId.AutoNormalActive, "MachineModeAutoNormalActive",
                                                       true, MachineModeCategoryId.Active, autoActive);
      autoActive.Auto = true;
      autoActive.AutoSequence = true;
      autoActive.Color = "#008000"; // Green
      InsertDefaultValue (autoNormalActive);
      // AutoTestActive
      IMachineMode autoTestActive = new MachineMode ((int)MachineModeId.AutoTestActive, "MachineModeAutoTestActive",
                                                     true, MachineModeCategoryId.Active, autoActive);
      autoActive.Auto = true;
      autoActive.AutoSequence = true;
      autoActive.Color = "#993399"; // Purple
      InsertDefaultValue (autoTestActive);
      // LockActive
      IMachineMode machineLock = new MachineMode ((int)MachineModeId.MachineLock, "MachineModeMachineLock",
                                                  true, MachineModeCategoryId.Active, autoTestActive);
      autoActive.Auto = true;
      autoActive.AutoSequence = true;
      autoActive.Color = "#993399"; // Purple
      InsertDefaultValue (machineLock);
      // DryRun
      IMachineMode dryRun = new MachineMode ((int)MachineModeId.DryRun, "MachineModeDryRun",
                                             true, MachineModeCategoryId.Active, autoTestActive);
      autoActive.Auto = true;
      autoActive.AutoSequence = true;
      autoActive.Color = "#993399"; // Purple
      InsertDefaultValue (dryRun);
      // ManualActive
      IMachineMode manualActive = new MachineMode ((int)MachineModeId.ManualActive, "MachineModeManualActive",
                                                   true, MachineModeCategoryId.Active, active);
      manualActive.Manual = true;
      manualActive.AutoSequence = false;
      manualActive.Color = "#008000"; // Green
      InsertDefaultValue (manualActive);
      // JogActive (manual)
      IMachineMode jogActive = new MachineMode ((int)MachineModeId.JogActive, "MachineModeJogActive",
                                                true, MachineModeCategoryId.Active, manualActive);
      jogActive.Manual = true;
      jogActive.AutoSequence = false;
      jogActive.Color = "#008000"; // Green
      InsertDefaultValue (jogActive);
      // HandleActive / Handwheel (manual)
      IMachineMode handleActive = new MachineMode ((int)MachineModeId.HandleActive, "MachineModeHandleActive",
                                                   true, MachineModeCategoryId.Active, manualActive);
      handleActive.Manual = true;
      handleActive.AutoSequence = false;
      handleActive.Color = "#008000"; // Green
      InsertDefaultValue (handleActive);
      // MdiActive
      IMachineMode mdiActive = new MachineMode ((int)MachineModeId.MdiActive, "MachineModeMdiActive",
                                                true, MachineModeCategoryId.Active, manualActive);
      mdiActive.Manual = true;
      mdiActive.AutoSequence = false;
      mdiActive.Color = "#008000"; // Green
      InsertDefaultValue (mdiActive);
      // SingleBlockActive
      IMachineMode singleBlockActive = new MachineMode ((int)MachineModeId.SingleBlockActive, "MachineModeSingleBlockActive",
                                                        true, MachineModeCategoryId.Active, manualActive);
      singleBlockActive.Manual = true;
      singleBlockActive.AutoSequence = false;
      singleBlockActive.Color = "#008000"; // Green
      InsertDefaultValue (singleBlockActive);
      // Reference
      IMachineMode reference = new MachineMode ((int)MachineModeId.Reference, "MachineModeReference",
                                                true, MachineModeCategoryId.Active, manualActive);
      mdiActive.Manual = true;
      mdiActive.AutoSequence = false;
      mdiActive.Color = "#008000"; // Green
      InsertDefaultValue (reference);
      // NoData
      IMachineMode noData = new MachineMode ((int)MachineModeId.NoData, "MachineModeNoData",
                                             null, MachineModeCategoryId.Unknown, unknown);
      noData.AutoSequence = false;
      noData.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (noData);
      // Unavailable
      IMachineMode unavailable = new MachineMode ((int)MachineModeId.Unavailable, "MachineModeUnavailable",
                                                  null, MachineModeCategoryId.Unknown, unknown);
      unavailable.AutoSequence = false;
      unavailable.Color = "#736F6E";
      InsertDefaultValue (unavailable);
      // Error
      IMachineMode error = new MachineMode ((int)MachineModeId.Error, "MachineModeError",
                                            false, MachineModeCategoryId.Error, inactiveOn);
      error.AutoSequence = false;
      error.Color = "#FF0000";
      InsertDefaultValue (error);
      // Off
      IMachineMode off = new MachineMode ((int)MachineModeId.Off, "MachineModeOff",
                                          false, MachineModeCategoryId.Inactive, inactive);
      off.AutoSequence = false;
      off.Color = "#C0C0C0"; // Silver
      InsertDefaultValue (off);
      // AutoNoRunningProgram
      IMachineMode autoNoRunningProgram = new MachineMode ((int)MachineModeId.AutoNoRunningProgram, "MachineModeAutoNoRunningProgram",
                                                           false, MachineModeCategoryId.Inactive, autoInactive);
      autoNoRunningProgram.Auto = true;
      autoNoRunningProgram.AutoSequence = false;
      autoNoRunningProgram.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (autoNoRunningProgram);
      // Interrupted
      IMachineMode interrupted = new MachineMode ((int)MachineModeId.Interrupted, "MachineModeInterrupted",
                                                  false, MachineModeCategoryId.Inactive, autoInactive);
      interrupted.Auto = true;
      interrupted.AutoSequence = false;
      interrupted.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (interrupted);
      // Feed Hold (in Auto mode only)
      IMachineMode hold = new MachineMode ((int)MachineModeId.Hold, "MachineModeHold",
                                           false, MachineModeCategoryId.Inactive, interrupted);
      // Not necessarily in an auto context: do not set Auto or AutoSequence to true
      hold.AutoSequence = false;
      hold.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (hold);
      // Ready
      IMachineMode ready = new MachineMode ((int)MachineModeId.Ready, "MachineModeReady",
                                            false, MachineModeCategoryId.Inactive, autoNoRunningProgram);
      ready.Auto = true;
      ready.AutoSequence = false;
      ready.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (ready);
      // Stopped
      IMachineMode stopped = new MachineMode ((int)MachineModeId.Stopped, "MachineModeStopped",
                                              false, MachineModeCategoryId.Inactive, autoNoRunningProgram);
      stopped.Auto = true;
      stopped.AutoSequence = false;
      stopped.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (stopped);
      // Finished
      IMachineMode finished = new MachineMode ((int)MachineModeId.Finished, "MachineModeFinished",
                                               false, MachineModeCategoryId.Inactive, autoNoRunningProgram);
      finished.Auto = true;
      finished.AutoSequence = false;
      finished.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (finished);
      // Reset / Canceled
      IMachineMode reset = new MachineMode ((int)MachineModeId.Reset, "MachineModeReset",
                                            false, MachineModeCategoryId.Inactive, autoNoRunningProgram);
      reset.Auto = true;
      reset.AutoSequence = false;
      reset.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (reset);
      // NotReady
      IMachineMode notReady = new MachineMode ((int)MachineModeId.NotReady, "MachineModeNotReady",
                                               false, MachineModeCategoryId.Inactive, autoNoRunningProgram);
      notReady.Auto = true;
      notReady.AutoSequence = false;
      notReady.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (notReady);
      // AutoError
      IMachineMode autoError = new MachineMode ((int)MachineModeId.AutoError, "MachineModeAutoError",
                                                false, MachineModeCategoryId.Error, interrupted);
      autoError.Auto = true;
      autoError.AutoSequence = false;
      autoError.Color = "#FF0000"; // Red
      InsertDefaultValue (autoError);
      // AutoErrorCleared
      IMachineMode autoErrorCleared = new MachineMode ((int)MachineModeId.AutoErrorCleared, "MachineModeAutoErrorCleared",
                                                       false, MachineModeCategoryId.Inactive, interrupted);
      autoErrorCleared.Auto = true;
      autoErrorCleared.AutoSequence = false;
      autoErrorCleared.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (autoErrorCleared);
      // AutoEmergency
      IMachineMode autoEmergency = new MachineMode ((int)MachineModeId.AutoEmergency, "MachineModeAutoEmergency",
                                                    false, MachineModeCategoryId.Error, interrupted);
      autoEmergency.Auto = true;
      autoEmergency.AutoSequence = false;
      autoEmergency.Color = "#FF0000"; // Red
      InsertDefaultValue (autoEmergency);
      // AutoNullOverride
      IMachineMode autoNullOverride = new MachineMode ((int)MachineModeId.AutoNullOverride, "MachineModeAutoNullOverride",
                                                       false, MachineModeCategoryId.Inactive, interrupted);
      autoNullOverride.Auto = true;
      autoNullOverride.AutoSequence = false; // Safer not to consider them as an AutoSequence
      autoNullOverride.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (autoNullOverride);
      // AutoNullFeedrateOverride
      IMachineMode autoNullFeedrateOverride = new MachineMode ((int)MachineModeId.AutoNullFeedrateOverride, "MachineModeAutoNullFeedrateOverride",
                                                               false, MachineModeCategoryId.Inactive, autoNullOverride);
      autoNullFeedrateOverride.Auto = true;
      autoNullFeedrateOverride.AutoSequence = false; // Safer not to consider them as an AutoSequence
      autoNullFeedrateOverride.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (autoNullFeedrateOverride);
      // AutoNullRapidTraverseOverride
      IMachineMode autoNullRapidTraverseOverride = new MachineMode ((int)MachineModeId.AutoNullRapidTraverseOverride, "MachineModeAutoNullRapidTraverseOverride",
                                                                    false, MachineModeCategoryId.Inactive, autoNullOverride);
      autoNullRapidTraverseOverride.Auto = true;
      autoNullRapidTraverseOverride.AutoSequence = false; // Safer not to consider them as an AutoSequence
      autoNullRapidTraverseOverride.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (autoNullRapidTraverseOverride);
      // AutoMachining
      IMachineMode autoMachining = new MachineMode ((int)MachineModeId.AutoMachining, "MachineModeAutoMachining",
                                                    true, MachineModeCategoryId.Active, autoNormalActive);
      autoMachining.Auto = true;
      autoMachining.AutoSequence = true;
      autoMachining.Color = "#008000"; // Green
      InsertDefaultValue (autoMachining);
      // AutoFeed
      IMachineMode autoFeed = new MachineMode ((int)MachineModeId.AutoFeed, "MachineModeAutoFeed",
                                               true, MachineModeCategoryId.Active, autoMachining);
      autoFeed.Auto = true;
      autoFeed.AutoSequence = true;
      autoFeed.Color = "#008000"; // Green
      InsertDefaultValue (autoFeed);
      // AutoRapidTraverse
      IMachineMode autoRapidTraverse = new MachineMode ((int)MachineModeId.AutoRapidTraverse, "MachineModeAutoRapidTraverse",
                                                        true, MachineModeCategoryId.Active, autoMachining);
      autoRapidTraverse.Auto = true;
      autoRapidTraverse.AutoSequence = true;
      autoRapidTraverse.Color = "#008000"; // Green
      InsertDefaultValue (autoRapidTraverse);
      // Machining (auto/manual status unknown)
      IMachineMode machining = new MachineMode ((int)MachineModeId.Machining, "MachineModeMachining",
                                                true, MachineModeCategoryId.Active, active);
      machining.AutoSequence = true;
      machining.Color = "#008000"; // Green
      InsertDefaultValue (machining);
      // RapidTraverse
      IMachineMode rapidTraverse = new MachineMode ((int)MachineModeId.RapidTraverse, "MachineModeRapidTraverse",
                                                    true, MachineModeCategoryId.Active, machining);
      rapidTraverse.AutoSequence = true;
      rapidTraverse.Color = "#008000"; // Green
      InsertDefaultValue (rapidTraverse);
      // AutoOtherOperation
      IMachineMode autoOtherOperation = new MachineMode ((int)MachineModeId.AutoOtherOperation, "MachineModeAutoOtherOperation",
                                                         true, MachineModeCategoryId.Active, autoNormalActive);
      autoOtherOperation.Auto = true;
      autoOtherOperation.AutoSequence = true;
      autoOtherOperation.Color = "#008000"; // Green
      InsertDefaultValue (autoOtherOperation);
      // AutoToolChange
      IMachineMode autoToolChange = new MachineMode ((int)MachineModeId.AutoToolChange, "MachineModeAutoToolChange",
                                                     true, MachineModeCategoryId.Active, autoOtherOperation);
      autoToolChange.Auto = true;
      autoToolChange.AutoSequence = true;
      autoToolChange.Color = "#008000";
      InsertDefaultValue (autoToolChange);
      // AutoLaserCheck
      IMachineMode autoLaserCheck = new MachineMode ((int)MachineModeId.AutoLaserCheck, "MachineModeAutoLaserCheck",
                                                     true, MachineModeCategoryId.Active, autoOtherOperation);
      autoToolChange.Auto = true;
      autoLaserCheck.AutoSequence = true;
      autoLaserCheck.Color = "#008000";
      InsertDefaultValue (autoLaserCheck);
      // AutoPalletChange
      IMachineMode autoPalletChange = new MachineMode ((int)MachineModeId.AutoPalletChange, "MachineModeAutoPalletChange",
                                                       true, MachineModeCategoryId.Active, autoOtherOperation);
      autoPalletChange.Auto = true;
      autoPalletChange.AutoSequence = true;
      autoPalletChange.Color = "#008000";
      InsertDefaultValue (autoPalletChange);
      // AutoProbingCycle
      IMachineMode autoProbingCycle = new MachineMode ((int)MachineModeId.AutoProbingCycle, "MachineModeAutoProbingCycle",
                                                       true, MachineModeCategoryId.Active, autoOtherOperation);
      autoProbingCycle.Auto = true;
      autoProbingCycle.AutoSequence = true;
      autoProbingCycle.Color = "#008000"; // Green
      InsertDefaultValue (autoProbingCycle);
      // AutoHomePositioning
      IMachineMode autoHomePositioning = new MachineMode ((int)MachineModeId.AutoHomePositioning, "MachineModeAutoHomePositioning",
                                                          true, MachineModeCategoryId.Active, autoMachining);
      autoHomePositioning.Auto = true;
      autoHomePositioning.AutoSequence = true;
      autoHomePositioning.Color = "#008000"; // Green
      InsertDefaultValue (autoHomePositioning);
      // MStop
      IMachineMode mStop = new MachineMode ((int)MachineModeId.MStop, "MachineModeMStop",
                                            false, MachineModeCategoryId.Inactive, interrupted);
      mStop.Auto = true;
      mStop.AutoSequence = false;
      mStop.Color = "#FFFFFF"; // White
      InsertDefaultValue (mStop);
      // Stop (M0)
      IMachineMode m0 = new MachineMode ((int)MachineModeId.M0, "MachineModeM0",
                                         false, MachineModeCategoryId.Inactive, mStop);
      m0.Auto = true;
      m0.AutoSequence = false;
      m0.Color = "#FFFFFF"; // White
      InsertDefaultValue (m0);
      // OptionalStop (M1)
      IMachineMode m1 = new MachineMode ((int)MachineModeId.M1, "MachineModeM1",
                                         false, MachineModeCategoryId.Inactive, mStop);
      m1.Auto = true;
      m1.AutoSequence = false;
      m1.Color = "#FFFFFF"; // White
      InsertDefaultValue (m1);
      // M60
      IMachineMode m60 = new MachineMode ((int)MachineModeId.M60, "MachineModeM60",
                                          false, MachineModeCategoryId.Inactive, mStop);
      m0.Auto = true;
      m0.AutoSequence = false;
      m0.Color = "#FFFFFF"; // White
      InsertDefaultValue (m60);
      // MWait
      IMachineMode mWait = new MachineMode ((int)MachineModeId.MWait, "MachineModeMWait",
                                            false, MachineModeCategoryId.Inactive, mStop);
      mWait.Auto = true;
      mWait.AutoSequence = false;
      mWait.Color = "#FFFFFF"; // White
      InsertDefaultValue (mWait);
      // Emergency
      IMachineMode emergency = new MachineMode ((int)MachineModeId.Emergency, "MachineModeEmergency",
                                                false, MachineModeCategoryId.Error, error);
      emergency.AutoSequence = false;
      emergency.Color = "#FF0000"; // Red
      InsertDefaultValue (emergency);
      // ManualInactive
      IMachineMode manualInactive = new MachineMode ((int)MachineModeId.ManualInactive, "MachineModeManualInactive",
                                                     false, MachineModeCategoryId.Inactive, inactiveOn);
      manualActive.Manual = true;
      manualActive.AutoSequence = false;
      manualActive.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (manualInactive);
      // SingleBlockInactive
      IMachineMode singleBlockInactive = new MachineMode ((int)MachineModeId.SingleBlockInactive, "MachineModeSingleBlockInactive",
                                                          false, MachineModeCategoryId.Inactive, manualInactive);
      singleBlockInactive.Manual = true;
      singleBlockInactive.AutoSequence = false;
      singleBlockInactive.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (singleBlockInactive);
      // MdiInactive
      IMachineMode mdiInactive = new MachineMode ((int)MachineModeId.MdiInactive, "MachineModeMdiInactive",
                                                  false, MachineModeCategoryId.Inactive, manualInactive);
      mdiInactive.Manual = true;
      mdiInactive.AutoSequence = false;
      mdiInactive.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (mdiInactive);
      // JogInactive
      IMachineMode jogInactive = new MachineMode ((int)MachineModeId.JogInactive, "MachineModeJogInactive",
                                                  false, MachineModeCategoryId.Inactive, manualInactive);
      jogInactive.Manual = true;
      jogInactive.AutoSequence = false;
      jogInactive.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (jogInactive);
      // HandleInactive
      IMachineMode handleInactive = new MachineMode ((int)MachineModeId.HandleInactive, "MachineModeHandleInactive",
                                                     false, MachineModeCategoryId.Inactive, manualInactive);
      handleInactive.Manual = true;
      handleInactive.AutoSequence = false;
      handleInactive.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (handleInactive);
      // ProbingCycle (but auto/manual status unknown)
      IMachineMode probingCycle = new MachineMode ((int)MachineModeId.ProbingCycle, "MachineModeProbingCycle",
                                                   true, MachineModeCategoryId.Active, machining);
      probingCycle.AutoSequence = true;
      probingCycle.Color = "#008000"; // Green
      InsertDefaultValue (probingCycle);
      /* homePositioning, deprecated */
      // ManualUnknown
      IMachineMode manualUnknown = new MachineMode ((int)MachineModeId.ManualUnknown, "MachineModeManualUnknown",
                                                    null, MachineModeCategoryId.Unknown, unknown);
      manualUnknown.Manual = true;
      manualUnknown.AutoSequence = false;
      manualUnknown.Color = "#00FFFF"; // Light blue
      InsertDefaultValue (manualUnknown);
      // SingleBlock
      IMachineMode singleBlock = new MachineMode ((int)MachineModeId.SingleBlock, "MachineModeSingleBlock",
                                                  null, MachineModeCategoryId.Unknown, manualUnknown);
      singleBlock.Manual = true;
      singleBlock.AutoSequence = false;
      singleBlock.Color = "#00FFFF"; // Light blue
      InsertDefaultValue (singleBlock);
      // Mdi
      IMachineMode mdi = new MachineMode ((int)MachineModeId.Mdi, "MachineModeMdi",
                                          null, MachineModeCategoryId.Unknown, manualUnknown);
      mdi.Manual = true;
      mdi.AutoSequence = false;
      mdi.Color = "#00FFFF"; // Light blue
      InsertDefaultValue (mdi);
      // Jog
      IMachineMode jog = new MachineMode ((int)MachineModeId.Jog, "MachineModeJog",
                                          null, MachineModeCategoryId.Unknown, manualUnknown);
      jog.Manual = true;
      jog.AutoSequence = false;
      jog.Color = "#00FFFF"; // Light blue
      InsertDefaultValue (jog);
      // Handle
      IMachineMode handle = new MachineMode ((int)MachineModeId.Handle, "MachineModeHandle",
                                             null, MachineModeCategoryId.Unknown, manualUnknown);
      handle.Manual = true;
      handle.AutoSequence = false;
      handle.Color = "#00FFFF"; // Light blue
      InsertDefaultValue (handle);
      // AcquisitionError
      IMachineMode acquisitionError = new MachineMode ((int)MachineModeId.AcquisitionError, "MachineModeAcquisitionError",
                                                       null, MachineModeCategoryId.Unknown, unknown);
      acquisitionError.AutoSequence = false;
      acquisitionError.Color = "#FF0000"; // Red
      InsertDefaultValue (acquisitionError);
      // MissingInfo
      IMachineMode missingInfo = new MachineMode ((int)MachineModeId.MissingInfo, "MachineModeMissingInfo",
                                                  null, MachineModeCategoryId.Unknown, unknown);
      acquisitionError.AutoSequence = false;
      acquisitionError.Color = "#FFFFCC"; // Light yellow
      InsertDefaultValue (missingInfo);
      // NoMotion
      IMachineMode noMotion = new MachineMode ((int)MachineModeId.NoMotion, "MachineModeNoMotion",
                                               false, MachineModeCategoryId.Inactive, inactiveOn);
      noMotion.AutoSequence = false;
      noMotion.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (noMotion);
      // AutoNoMotion
      IMachineMode autoNoMotion = new MachineMode ((int)MachineModeId.AutoNoMotion, "MachineModeAutoNoMotion",
                                                   false, MachineModeCategoryId.Inactive, autoInactive);
      autoNoMotion.Auto = true;
      autoNoMotion.AutoSequence = false;
      autoNoMotion.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (autoNoMotion);
      // ManualNoMotion
      IMachineMode manualNoMotion = new MachineMode ((int)MachineModeId.ManualNoMotion, "MachineModeManualNoMotion",
                                                     false, MachineModeCategoryId.Inactive, manualInactive);
      manualNoMotion.Manual = true;
      manualNoMotion.AutoSequence = false;
      manualNoMotion.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (manualNoMotion);
      // SingleBlockNoMotion
      IMachineMode singleBlockNoMotion = new MachineMode ((int)MachineModeId.SingleBlockNoMotion, "MachineModeSingleBlockNoMotion",
                                                          false, MachineModeCategoryId.Inactive, manualNoMotion);
      singleBlockNoMotion.Manual = true;
      singleBlockNoMotion.AutoSequence = false;
      singleBlockNoMotion.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (singleBlockNoMotion);
      // MdiNoMotion
      IMachineMode mdiNoMotion = new MachineMode ((int)MachineModeId.MdiNoMotion, "MachineModeMdiNoMotion",
                                                  false, MachineModeCategoryId.Inactive, manualNoMotion);
      mdiNoMotion.Manual = true;
      mdiNoMotion.AutoSequence = false;
      mdiNoMotion.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (mdiNoMotion);
      // JogNoMotion
      IMachineMode jogNoMotion = new MachineMode ((int)MachineModeId.JogNoMotion, "MachineModeJogNoMotion",
                                                  false, MachineModeCategoryId.Inactive, manualNoMotion);
      jogNoMotion.Manual = true;
      jogNoMotion.AutoSequence = false;
      jogNoMotion.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (jogNoMotion);
      // HandleNoMotion
      IMachineMode handleNoMotion = new MachineMode ((int)MachineModeId.HandleNoMotion, "MachineModeHandleNoMotion",
                                                     false, MachineModeCategoryId.Inactive, manualNoMotion);
      handleNoMotion.Manual = true;
      handleNoMotion.AutoSequence = false;
      handleNoMotion.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (handleNoMotion);
      // ProbablyOff
      IMachineMode probablyOff = new MachineMode ((int)MachineModeId.ProbablyOff, "MachineModeProbablyOff",
                                                  false, MachineModeCategoryId.Inactive, off);
      probablyOff.AutoSequence = false;
      probablyOff.Color = "#FFFF00"; // Yellow
      InsertDefaultValue (probablyOff);
      // AutoUnknown
      IMachineMode autoUnknown = new MachineMode ((int)MachineModeId.AutoUnknown, "MachineModeAutoUnknown",
                                                  null, MachineModeCategoryId.Unknown, unknown);
      autoUnknown.Auto = true;
      autoUnknown.AutoSequence = false;
      autoUnknown.Color = "#C0C0C0"; // Silver
      InsertDefaultValue (autoUnknown);
      // AlarmStop
      IMachineMode alarmStop = new MachineMode ((int)MachineModeId.AlarmStop, "MachineModeAlarmStop",
                                                false, MachineModeCategoryId.Error, error);
      alarmStop.AutoSequence = false;
      alarmStop.Color = "#FF0000"; // Red
      InsertDefaultValue (alarmStop);
      // AutoAlarmStop
      IMachineMode autoAlarmStop = new MachineMode ((int)MachineModeId.AutoAlarmStop, "MachineModeAutoAlarmStop",
                                                    false, MachineModeCategoryId.Error, autoError);
      autoAlarmStop.Auto = true;
      autoAlarmStop.AutoSequence = false;
      autoAlarmStop.Color = "#FF0000"; // Red
      InsertDefaultValue (autoAlarmStop);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="machineMode">not null</param>
    private void InsertDefaultValue (IMachineMode machineMode)
    {
      Debug.Assert (null != machineMode);
      
      try {
        IMachineMode existingMachineMode = FindById (machineMode.Id);
        if (null == existingMachineMode) { // the config does not exist => create it
          log.InfoFormat ("InsertDefaultValue: " +
                          "add id={0} translationKey={1}",
                          machineMode.Id, machineMode.TranslationKey);
          // Use a raw SQL Command, else the Id is reset
          using (var command = NHibernateHelper.GetCurrentSession ().Connection.CreateCommand())
          {
            command.CommandText = string.Format (@"INSERT INTO MachineMode (machinemodeid, machinemodetranslationkey, machinemoderunning, machinemodeauto, machinemodemanual, machinemodeautosequence, machinemodecolor, machinemodecategoryid, parentmachinemodeid)
VALUES ({0}, '{1}', {2}, {3}, {4}, {5}, '{6}', {7}, {8})",
                                                 machineMode.Id, machineMode.TranslationKey,
                                                 machineMode.Running.HasValue?machineMode.Running.Value.ToString ():"NULL",
                                                 machineMode.Auto.HasValue?machineMode.Auto.Value.ToString ():"NULL",
                                                 machineMode.Manual.HasValue?machineMode.Manual.Value.ToString ():"NULL",
                                                 machineMode.AutoSequence, machineMode.Color,
                                                 (int)machineMode.MachineModeCategory,
                                                 (null != machineMode.Parent)?machineMode.Parent.Id.ToString ():"NULL");
            command.ExecuteNonQuery();
          }
          ModelDAOHelper.DAOFactory.FlushData ();
        }
        else { // The config already exists
          // Upgrade it if parent is null
          if ( ((int)MachineModeId.Inactive != existingMachineMode.Id)
              && ((int)MachineModeId.Active != existingMachineMode.Id)
              && ((int)MachineModeId.Unknown != existingMachineMode.Id)
              && (null == existingMachineMode.Parent)) {
            existingMachineMode.Parent = FindById (machineMode.Parent.Id);
            existingMachineMode.Running = machineMode.Running;
            existingMachineMode.TranslationKey = machineMode.TranslationKey;
            existingMachineMode.Auto = machineMode.Auto;
            existingMachineMode.Manual = machineMode.Manual;
            existingMachineMode.AutoSequence = machineMode.AutoSequence;
            MakePersistent (existingMachineMode);
            ModelDAOHelper.DAOFactory.FlushData ();
          }
        }
      }
      catch (Exception ex) {
        log.ErrorFormat ("InsertDefaultValue: " +
                         "inserting new machine mode {0} " +
                         "failed with {1}",
                         machineMode,
                         ex);
      }
    }
    #endregion // DefaultValues
    
    /// <summary>
    /// Implements IMachineModeDAO
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="translationKeyOrName"></param>
    /// <returns></returns>
    public IMachineMode FindByTranslationKeyOrName(string translationKeyOrName)
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineMode> ()
        .Add (Restrictions.Or (Restrictions.Eq ("TranslationKey", translationKeyOrName),
                               Restrictions.Eq ("Name", translationKeyOrName)))
        .SetCacheable (true)
        .UniqueResult<IMachineMode> ();
    }

    /// <summary>
    /// Implements IMachineModeDAO
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMachineMode> FindAutoSequence()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineMode> ()
        .Add (Restrictions.Eq ("AutoSequence", true))
        .SetCacheable (true)
        .List<IMachineMode> ();
    }

    /// <summary>
    /// Implements IMachineModeDAO
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <returns></returns>
    public IList<IMachineMode> FindRunning()
    {
      return NHibernateHelper.GetCurrentSession ()
        .CreateCriteria<MachineMode> ()
        .Add (Restrictions.Eq ("Running", true))
        .SetCacheable (true)
        .List<IMachineMode> ();
    }
  }
}
