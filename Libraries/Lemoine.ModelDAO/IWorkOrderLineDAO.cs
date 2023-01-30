// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IWorkOrderLine.
  /// </summary>
  public interface IWorkOrderLineDAO: IGenericByLineUpdateDAO<IWorkOrderLine, int>
  {
    /// <summary>
    /// Find all WorkOrderLine for a specific workorder
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    IList<IWorkOrderLine> FindAllByWorkOrder(IWorkOrder workOrder);
    
    /// <summary>
    /// Find all WorkOrderLine for a specific line
    /// </summary>
    /// <param name="line">not null</param>
    /// <returns></returns>
    IList<IWorkOrderLine> FindAllByLine(ILine line);
    
    /// <summary>
    /// Find the first IWorkOrderLine after a given date
    /// </summary>
    /// <param name="line"></param>
    /// <param name="beginAfter"></param>
    /// <returns></returns>
    IWorkOrderLine FindFirstAfter (ILine line, DateTime beginAfter);

    /// <summary>
    /// Get all the line slots for the specified line and the specified time range
    /// </summary>
    /// <param name="line"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IWorkOrderLine> GetListInRange (ILine line, UtcDateTimeRange range);
    
    /// <summary>
    /// Get all the line slots for the specified time range
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    IList<IWorkOrderLine> GetListInRange (UtcDateTimeRange range);
    
    /// <summary>
    /// Find the slot at the specified UTC date/time
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="dateTime">in UTC</param>
    /// <returns></returns>
    IWorkOrderLine FindAt (ILine line, DateTime dateTime);
    
    /// <summary>
    /// Find a specific WorkOrderLine for a specific (workorder + line)
    /// The result may be null
    /// </summary>
    /// <param name="line">not null</param>
    /// <param name="workOrder">not null</param>
    /// <returns></returns>
    IWorkOrderLine FindByLineAndWorkOrder(ILine line, IWorkOrder workOrder);

    /// <summary>
    /// Get all the line slots in progress
    /// </summary>
    /// <returns></returns>
    IList<IWorkOrderLine> GetWorkOrderLineInProgress ();
  }
}
