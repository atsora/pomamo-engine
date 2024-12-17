// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReasonMachineAssociation.
  /// </summary>
  public interface IReasonMachineAssociationDAO : ISpecificMachineModificationDAO<IReasonMachineAssociation>
  {
    /// <summary>
    /// Insert a new row in database with a new manual reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    /// <returns></returns>
    long InsertManualReason (IMachine machine, UtcDateTimeRange range, IReason reason, double reasonScore, string details, string jsonData);

    /// <summary>
    /// Insert a new row in database to reset a reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    long InsertResetReason (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Insert a new row in database with an auto-reason
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <param name="reason"></param>
    /// <param name="reasonScore"></param>
    /// <param name="details"></param>
    /// <param name="dynamic"></param>
    /// <param name="overwriteRequired"></param>
    /// <param name="option"></param>
    /// <returns></returns>
    long InsertAutoReason (IMachine machine, UtcDateTimeRange range, IReason reason, double reasonScore, string details, string dynamic, bool overwriteRequired, AssociationOption? option);

    /// <summary>
    /// Insert a row in database that corresponds to a sub-modification
    /// 
    /// Check first range is not null
    /// </summary>
    /// <param name="association"></param>
    /// <param name="range">not null</param>
    /// <param name="preChange"></param>
    /// <param name="parent">optional: alternative parent</param>
    /// <returns></returns>
    IMachineModification InsertSub (IReasonMachineAssociation association, UtcDateTimeRange range, Action<IReasonMachineAssociation> preChange, IMachineModification parent);

    /// <summary>
    /// Get the next auto association with no parent
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="modificationId"></param>
    /// <returns></returns>
    IReasonMachineAssociation GetNextAncestorAuto (IMachine machine, long modificationId);

    /// <summary>
    /// Find all the applied manual reason machine associations in the specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonMachineAssociation> FindAppliedManualInRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the applied auto reason machine associations in the specified range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonMachineAssociation> FindAppliedAutoInRange (IMachine machine, UtcDateTimeRange range);
  }

  /// <summary>
  /// Extensions to <see cref="IReasonMachineAssociationDAO"/>
  /// </summary>
  public static class ReasonMachineAssocationDAOExtensions
  {
  }
}
