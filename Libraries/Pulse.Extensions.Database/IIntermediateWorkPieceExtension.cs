// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Model;
using Lemoine.ModelDAO;
using System;
using System.Collections.Generic;

namespace Pulse.Extensions.Database
{
  /// <summary>
  /// 
  /// </summary>
  public interface IIntermediateWorkPieceExtension : Lemoine.Extensions.IExtension
  {
    /// <summary>
    /// Additional actions to take when two intermediate work pieces are merged
    /// </summary>
    /// <param name="oldIntermediateWorkPiece">not null</param>
    /// <param name="newIntermediateWorkPiece">not null</param>
    void Merge (IIntermediateWorkPiece oldIntermediateWorkPiece,
                IIntermediateWorkPiece newIntermediateWorkPiece);

    /// <summary>
    /// Change the quantity of intermediate work pieces
    /// an operation makes
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="oldQuantity"></param>
    /// <param name="newQuantity"></param>
    void UpdateQuantity (IIntermediateWorkPiece intermediateWorkPiece, int oldQuantity, int newQuantity);
  }
}
