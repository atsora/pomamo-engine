// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Model;
using Lemoine.ModelDAO;
using Lemoine.Core.Log;
using Lemoine.Extensions.Web.Responses;

namespace Lemoine.Web.ProductionMachiningStatus.CurrentMachineStateTemplateOperation
{
  /// <summary>
  /// Description of CurrentMachineStateTemplateOperationService.
  /// </summary>
  public class CurrentMachineStateTemplateOperationService
    : GenericCachedService<CurrentMachineStateTemplateOperationRequestDTO>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CurrentMachineStateTemplateOperationService).FullName);

    #region Constructors
    /// <summary>
    /// 
    /// </summary>
    public CurrentMachineStateTemplateOperationService ()
      : base(Lemoine.Core.Cache.CacheTimeOut.CurrentLong)
    {
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Response to GET request (no cache)
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public override object GetWithoutCache(CurrentMachineStateTemplateOperationRequestDTO request)
    {
      Debug.Assert (null != request);
      
      using (IDAOSession session = ModelDAOHelper.DAOFactory.OpenSession ())
      {
        IMachine machine = ModelDAOHelper.DAOFactory.MachineDAO
          .FindById (request.MachineId);
        if (null == machine) {
          log.ErrorFormat ("GetWithoutCache: " +
                           "unknown machine with ID {0}",
                           request.MachineId);
          return new ErrorDTO ("No machine with the specified ID",
                               ErrorStatus.WrongRequestParameter);
        }
        
        IMachineStateTemplateSlot machineStateTemplateSlot = ModelDAOHelper.DAOFactory.MachineStateTemplateSlotDAO
          .FindAt (machine, DateTime.UtcNow);
        
        // Get the work information
        // Alternative: use OperationSlotDAO.GetEffective to get all the operationSlots that match the same properties 
        IEnumerable<IOperationSlot> operationSlots = ModelDAOHelper.DAOFactory.OperationSlotDAO
          .FindOverlapsRangeDescending (machine, machineStateTemplateSlot.DateTimeRange, TimeSpan.FromDays (3));
        IOperation operation = null;
        Bound<DateTime> since = new LowerBound<DateTime> ();
        bool differentOperation = false;
        foreach (var operationSlot in operationSlots) {
          if (null == operation) {
            if (null != operationSlot.Operation) {
              operation = operationSlot.Operation;
              since = operationSlot.BeginDateTime;
            }
          }
          else if (object.Equals (operation, operationSlot.Operation)) { // Same operation
            Debug.Assert (null != operationSlot.Operation);
            since = operationSlot.BeginDateTime;
          }
          else { // Different operation
            differentOperation = true;
            break;
          }
        }
        
        if ( (null != operation) && differentOperation) {
          since = Bound.GetMaximum<DateTime> (since, machineStateTemplateSlot.DateTimeRange.Lower);
        }
        else {
          since = machineStateTemplateSlot.DateTimeRange.Lower;
        }
        
        return new CurrentMachineStateTemplateOperationResponseDTOAssembler ()
          .Assemble (machineStateTemplateSlot.MachineStateTemplate,
                     operation,
                     since);
      }
    }
    #endregion // Methods
  }
}
