// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IReasonProposal.
  /// </summary>
  public interface IReasonProposalDAO
    : IGenericByMachineUpdateDAO<IReasonProposal, int>
  {
    /// <summary>
    /// Insert a new row directly in the database
    /// </summary>
    /// <param name="reasonMachineAssociation"></param>
    /// <param name="range"></param>
    /// <returns>new id</returns>
    int Insert (IReasonMachineAssociation reasonMachineAssociation, UtcDateTimeRange range);

    /// <summary>
    /// Get the item that match the specified reason machine association
    /// </summary>
    /// <param name="reasonMachineAssociation"></param>
    /// <returns></returns>
    IReasonProposal Get (IReasonMachineAssociation reasonMachineAssociation);

    /// <summary>
    /// Find the items at a specific time for the specified machine
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IList<IReasonProposal> FindAt (IMachine machine,
                                   DateTime dateTime);

    /// <summary>
    /// Find all the items that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonProposal> FindOverlapsRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find all the manual items that overlap the specified range
    /// 
    /// Order them by ascending range
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IReasonProposal> FindManualOverlapsRange (IMachine machine, UtcDateTimeRange range);

    /// <summary>
    /// Find the manual item at a specific time for the specified machine
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    IReasonProposal FindManualAt (IMachine machine,
                                  DateTime dateTime);
  }
}
