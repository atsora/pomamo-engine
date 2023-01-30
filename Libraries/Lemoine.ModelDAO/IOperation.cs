// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Core.Log;

namespace Lemoine.Model
{
  /// <summary>
  /// Model of table Operation
  /// </summary>
  public interface IOperation : IVersionable, IDataWithIdentifiers, IDisplayable, IEquatable<IOperation>, ISerializableModel, Lemoine.Collections.IDataWithId
  {
    /// <summary>
    /// Associated SimpleOperation
    /// </summary>
    ISimpleOperation SimpleOperation { get; }

    /// <summary>
    /// Long display
    /// </summary>
    string LongDisplay { get; }

    /// <summary>
    /// Short display
    /// </summary>
    string ShortDisplay { get; }

    /// <summary>
    /// Full name of the operation as used in the shop (written in the planning)
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Operation code
    /// </summary>
    string Code { get; set; }

    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing data with en external database
    /// </summary>
    string ExternalCode { get; set; }

    /// <summary>
    /// Link to the documentation in the network
    /// </summary>
    string DocumentLink { get; set; }

    /// <summary>
    /// Associated operation type
    /// </summary>
    IOperationType Type { get; set; }

    /// <summary>
    /// Active revision (latest one)
    /// 
    /// not null
    /// </summary>
    IOperationRevision ActiveRevision { get; }

    /// <summary>
    /// Operation revisions
    /// </summary>
    IList<IOperationRevision> Revisions { get; }

    /// <summary>
    /// Default active model
    /// 
    /// not null
    /// </summary>
    IOperationModel DefaultActiveModel { get; }

    /// <summary>
    /// Estimated machining duration
    /// </summary>
    TimeSpan? MachiningDuration { get; set; }

    /// <summary>
    /// Estimated setup duration
    /// </summary>
    TimeSpan? SetUpDuration { get; set; }

    /// <summary>
    /// Estimated tear down duration
    /// </summary>
    TimeSpan? TearDownDuration { get; set; }

    /// <summary>
    /// Estimated loading duration
    /// </summary>
    TimeSpan? LoadingDuration { get; set; }

    /// <summary>
    /// Estimated unloading duration
    /// </summary>
    TimeSpan? UnloadingDuration { get; set; }

    /// <summary>
    /// Creation date/time
    /// </summary>
    DateTime CreationDateTime { get; }

    /// <summary>
    /// Lock the operation for auto-update
    /// </summary>
    bool Lock { get; set; }

    /// <summary>
    /// Associated machine filter if applicable
    /// </summary>
    IMachineFilter MachineFilter { get; set; }

    /// <summary>
    /// Set of intermediate work pieces this operation makes
    /// </summary>
    ICollection<IIntermediateWorkPiece> IntermediateWorkPieces { get; }

    /// <summary>
    /// Set of intermediate work pieces that are need for this operation
    /// </summary>
    ICollection<IIntermediateWorkPiece> Sources { get; }

    /// <summary>
    /// Set of path that are associated to this operation
    /// </summary>
    ICollection<IPath> Paths { get; }

    /// <summary>
    /// Set of sequences that are associated to this operation
    /// </summary>
    ICollection<ISequence> Sequences { get; }

    /// <summary>
    /// List of stamps (and then ISO files) that are associated to this operation
    /// </summary>
    ICollection<IStamp> Stamps { get; }

    /// <summary>
    /// Return a value if the operation has been archived
    /// </summary>
    DateTime? ArchiveDateTime { get; set; }

    /// <summary>
    /// Associated durations
    /// </summary>
    ICollection<IOperationDuration> Durations { get; }

    /// <summary>
    /// Add an intermediate work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    void AddIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece);

    /// <summary>
    /// Add a source work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    void AddSource (IIntermediateWorkPiece intermediateWorkPiece);

    /// <summary>
    /// Remove a source work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    void RemoveSource (IIntermediateWorkPiece intermediateWorkPiece);

    /// <summary>
    /// Remove a path
    /// </summary>
    /// <param name="path"></param>
    void RemovePath (IPath path);

    /// <summary>
    /// Get the total number of intermediate work pieces
    /// </summary>
    /// <returns></returns>
    int GetTotalNumberOfIntermediateWorkPieces ();
  }

  /// <summary>
  /// Extension methods to IOperation
  /// </summary>
  public static class IOperationExtensions
  {
    static readonly ILog log = LogManager.GetLogger (typeof (IOperationExtensions).FullName);

    /// <summary>
    /// Get the standard 'between cycles' duration, either the pallet changing time or the loading/unloading time
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="monitoredMachine">null if not monitored</param>
    /// <returns></returns>
    public static TimeSpan? GetStandardBetweenCyclesDuration (this IOperation operation, IMonitoredMachine monitoredMachine)
    {
      if ((null != monitoredMachine)
          && monitoredMachine.PalletChangingDuration.HasValue
          && (0 < monitoredMachine.PalletChangingDuration.Value.Ticks)) {
        return monitoredMachine.PalletChangingDuration.Value;
      }
      else {
        TimeSpan? standardDuration = null;
        if (operation.LoadingDuration.HasValue) {
          standardDuration = standardDuration.HasValue
            ? standardDuration.Value.Add (operation.LoadingDuration.Value)
            : operation.LoadingDuration.Value;
        }
        if (operation.UnloadingDuration.HasValue) {
          standardDuration = standardDuration.HasValue
            ? standardDuration.Value.Add (operation.UnloadingDuration.Value)
            : operation.UnloadingDuration.Value;
        }
        return standardDuration;
      }
    }

    /// <summary>
    /// Get the standard cycle duration that includes both the machining duration and the loading/unloading time
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="monitoredMachine">null if not monitored</param>
    /// <returns></returns>
    public static TimeSpan? GetStandardCycleDuration (this IOperation operation, IMonitoredMachine monitoredMachine)
    {
      if (operation.MachiningDuration.HasValue) {
        TimeSpan standardCycleDuration = operation.MachiningDuration.Value;
        if (standardCycleDuration < TimeSpan.FromSeconds (1)) { // Wrong machining duration
          log.Warn ($"GetStandardCycleDuration: the machining duration {standardCycleDuration} is less than 1s, do not consider it");
          return null;
        }
        var standardBetweenCyclesDuration = operation.GetStandardBetweenCyclesDuration (monitoredMachine);
        if (standardBetweenCyclesDuration.HasValue) {
          standardCycleDuration = standardCycleDuration.Add (standardBetweenCyclesDuration.Value);
        }
        return standardCycleDuration;
      }
      else {
        return null;
      }
    }
  }
}
