// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IIntermediateWorkPieceTarget.
  /// </summary>
  public interface IIntermediateWorkPieceTargetDAO: IGenericDAO<IIntermediateWorkPieceTarget, int>
  {
    /// <summary>
    /// Find all the items that match the line / intermediate work piece / shift
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="line">not null</param>
    /// <param name="day">not null</param>
    /// <param name="shift">may be null</param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> FindByLineIntermediateWorkPieceShift (IIntermediateWorkPiece intermediateWorkPiece,
                                                                               ILine line,
                                                                               DateTime day,
                                                                               IShift shift);
    
    /// <summary>
    /// Find the unique item that may match the natural key
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <param name="component"></param>
    /// <param name="workOrder">nullable</param>
    /// <param name="line">nullable</param>
    /// <param name="day">nullable</param>
    /// <param name="shift">nullable</param>
    /// <returns></returns>
    IIntermediateWorkPieceTarget FindByKey (IIntermediateWorkPiece intermediateWorkPiece,
                                             IComponent component,
                                             IWorkOrder workOrder,
                                             ILine line,
                                             DateTime? day,
                                             IShift shift);
    
    /// <summary>
    /// Find all the items that match the specified intermediate work piece
    /// </summary>
    /// <param name="intermediateWorkPiece"></param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> FindByIntermediateWorkPiece (IIntermediateWorkPiece intermediateWorkPiece);

    /// <summary>
    /// Find all the items that match the specified component
    /// </summary>
    /// <param name="component"></param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> FindByComponent (IComponent component);

    /// <summary>
    /// Find all the items that match the specified work order
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> FindByWorkOrder (IWorkOrder workOrder);
    
    /// <summary>
    /// Find all the items that match the specified line
    /// </summary>
    /// <param name="line">not null</param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> FindByLine (ILine line);

    /// <summary>
    /// Find all the items that match the specified intermediate work piece, work order and line
    /// </summary>
    /// <param name="intermediateWorkPiece">not null</param>
    /// <param name="workOrder">not null</param>
    /// <param name="line">nullable</param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> FindByIwpWorkOrderLine (IIntermediateWorkPiece intermediateWorkPiece, IWorkOrder workOrder, ILine line);
    
    /// <summary>
    /// Find all the items that match the specified work order and line
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <param name="line">nullable</param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> FindByWorkOrderLine (IWorkOrder workOrder, ILine line);
    
    /// <summary>
    /// Get all the intermediate work piece summary for the specified time range
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    IList<IIntermediateWorkPieceTarget> GetListInRange (DateTime begin, DateTime end);
    
    /// <summary>
    /// Count all targets that match the line / shift
    /// Targets taken into account are not null and > 0
    /// All targets are attached to a workorder
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="day">not null</param>
    /// <param name="shift">may be null</param>
    /// <returns></returns>
    int CountTargetsByLineShift (ILine line, DateTime day, IShift shift);
  }
}
