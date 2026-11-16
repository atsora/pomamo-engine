// Copyright (C) 2026 Atsora Solutions

using System;
using System.Collections.Generic;
using Lemoine.Business.Config;
using Lemoine.Core.Log;
using Lemoine.Model;

namespace Pulse.Web.Operation
{
  /// <summary>
  /// Work informations (work order or job, part or component, operation) of an operation slot,
  /// with the id of each item, for example to show the documents of the operation and of the part
  ///
  /// Only the items that are set are returned
  /// </summary>
  public static class OperationSlotWorkInformations
  {
    static readonly ILog log = LogManager.GetLogger (typeof (OperationSlotWorkInformations).FullName);

    /// <summary>
    /// Build the work informations of an operation slot
    ///
    /// To call in an open session: the displays of the items are loaded
    /// </summary>
    /// <param name="operationSlot">not null</param>
    /// <returns></returns>
    public static List<WorkInformationDTO> Build (IOperationSlot operationSlot)
    {
      if (operationSlot is null) {
        log.Fatal ("Build: null operation slot");
        throw new ArgumentNullException (nameof (operationSlot));
      }

      var result = new List<WorkInformationDTO> ();

      var workOrder = operationSlot.WorkOrder;
      if (workOrder is not null) {
        var job = WorkOrderProjectIsJob ? workOrder.Job : null;
        if (job is not null) {
          result.Add (new WorkInformationDTO (WorkInformationKind.Job, job.Display, job.Id));
        }
        else {
          result.Add (new WorkInformationDTO (WorkInformationKind.WorkOrder, workOrder.Display, workOrder.Id));
        }
      }

      var component = operationSlot.Component;
      if (component is not null) {
        var part = ProjectComponentIsPart ? component.Part : null;
        if (part is not null) {
          result.Add (new WorkInformationDTO (WorkInformationKind.Part, part.Display, part.ComponentId));
        }
        else {
          result.Add (new WorkInformationDTO (WorkInformationKind.Component, component.Display, component.Id));
        }
      }

      var operation = operationSlot.Operation;
      if (operation is not null) {
        result.Add (new WorkInformationDTO (WorkInformationKind.Operation, operation.Display, operation.Id));
      }

      return result;
    }

    static bool WorkOrderProjectIsJob => Lemoine.Info.ConfigSet
      .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.WorkOrderProjectIsJob));

    static bool ProjectComponentIsPart => Lemoine.Info.ConfigSet
      .Get<bool> (ConfigKeys.GetDataStructureConfigKey (DataStructureConfigKey.ProjectComponentIsPart));
  }
}
