// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IComponentIntermediateWorkPiece.
  /// </summary>
  public interface IComponentIntermediateWorkPieceDAO: IGenericUpdateDAO<IComponentIntermediateWorkPiece, int>
  {
    /// <summary>
    /// Find ComponentIwp by component and iwp
    /// </summary>
    /// <param name="component"></param>
    /// <param name="iwp"></param>
    /// <returns></returns>
    IComponentIntermediateWorkPiece FindByComponentAndIwp(IComponent component, IIntermediateWorkPiece iwp);

    /// <summary>
    /// Find the component intermediate work piece relations by component and order
    /// with an early fetch of the intermediate work piece
    /// </summary>
    /// <param name="component">not null</param>
    /// <param name="order"></param>
    /// <returns></returns>
    IList<IComponentIntermediateWorkPiece> FindWithComponentOrder (IComponent component, int order);
  }
}
