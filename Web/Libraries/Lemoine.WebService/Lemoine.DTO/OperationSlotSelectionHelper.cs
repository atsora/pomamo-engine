// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Core.Log;
using Lemoine.Model;
using Lemoine.ModelDAO;

namespace Lemoine.DTO
{
  /// <summary>
  /// Description of OperationSlotSelectionHelper.
  /// </summary>
  public class OperationSlotSelectionHelper
  {
    static readonly ILog log = LogManager.GetLogger(typeof (OperationSlotSelectionHelper).FullName);

    
    /// <summary>
    /// Select last operation slot on machine which has a non-null operation
    /// and is not "too old".
    /// </summary>
    /// <param name="machine"></param>
    /// <param name="dateTime"></param>
    /// <returns></returns>
    public static IOperationSlot GetLastOperationNotNullOperationSlotWithinGap(IMonitoredMachine machine,
                                                                               DateTime dateTime)
    {
      IOperationSlot lastSlot =
        ModelDAOHelper.DAOFactory.OperationSlotDAO.GetLastOperationNotNullBefore(machine, dateTime);
      
      if (lastSlot == null) {
        log.InfoFormat("No last operation slot or no operation in slot for machine id {0}", machine.Id);
        return null;
      }
      
      if (lastSlot.EndDateTime.HasValue) {
        TimeSpan maxGap = (TimeSpan) ModelDAOHelper.DAOFactory.ConfigDAO.GetWebServiceConfigValue(WebServiceConfigKey.GapLastOperationSlot);
        if (dateTime.Subtract(lastSlot.EndDateTime.Value) > maxGap) {
          // reject slot since it's too old
          log.InfoFormat("Last operation slot having an operation is tool old for machine id {0}", machine.Id);
          return null;
        }
      }
      
      return lastSlot;
    }    
  }
}
