// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IComponentIntermediateWorkPieceUpdate.
  /// </summary>
  public interface IComponentIntermediateWorkPieceUpdateDAO: IGenericDAO<IComponentIntermediateWorkPieceUpdate, long>
  {
    /// <summary>
    /// Find all the ComponentIntermediateWorkPieceUpdate for a specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<IComponentIntermediateWorkPieceUpdate> FindAllWithComponent (IComponent component);

    /// <summary>
    /// Find all the ComponentIntermediateWorkPieceUpdate for a specified IntermediateWorkPiece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    IList<IComponentIntermediateWorkPieceUpdate> FindAllWithIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece);
  }
}
