// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// IDeliverablePieceMachineAssociation: associate a DeliverablePiece
  /// to a machine on an interval of time
  /// </summary>
  public interface IDeliverablePieceMachineAssociation : IMachineAssociation
  {
    /// <summary>
    /// Reference to the DeliverablePiece
    /// </summary>
    IDeliverablePiece DeliverablePiece { get; set; }
  }
}