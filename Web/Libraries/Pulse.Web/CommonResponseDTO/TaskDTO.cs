// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
#if NSERVICEKIT
using NServiceKit.ServiceHost;
#else // !NSERVICEKIT
using Lemoine.Extensions.Web.Attributes;
#endif // NSERVICEKIT

namespace Pulse.Web.CommonResponseDTO
{
  /// <summary>
  /// Response DTO for Task
  /// </summary>
  [Api ("Task Response DTO")]
  public class TaskDTO
  {
    /// <summary>
    /// Id of machine
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display of machine
    /// </summary>
    public string Display { get; set; }
  }

  /// <summary>
  /// Assembler for TaskDTO
  /// </summary>
  public class TaskDTOAssembler : IGenericDTOAssembler<TaskDTO, Lemoine.Model.ITask>
  {
    readonly ILog log = LogManager.GetLogger<TaskDTOAssembler> ();

    /// <summary>
    /// TaskDTO assembler
    /// </summary>
    /// <param name="task">nullable</param>
    /// <returns></returns>
    public TaskDTO Assemble (Lemoine.Model.ITask task)
    {
      if (null == task) {
        return null;
      }
      if (!Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (task)) {
        using (var session = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.OpenSession ()) {
          using (var transaction = session.BeginReadOnlyTransaction ("Web.TaskDTO.Assemble")) {
            var initialized = Lemoine.ModelDAO.ModelDAOHelper.DAOFactory.TaskDAO
              .FindById (((Lemoine.Collections.IDataWithId<int>)task).Id);
            if (null == initialized) {
              log.Error ($"Assemble: task with id {((Lemoine.Collections.IDataWithId<int>)task).Id} does not exist");
              return null;
            }
            else {
              return Assemble (initialized);
            }
          }
        }
      }
      TaskDTO taskDTO = new TaskDTO ();
      taskDTO.Id = ((Lemoine.Collections.IDataWithId<int>)task).Id;
      taskDTO.Display = task.Display;
      return taskDTO;
    }

    /// <summary>
    /// TaskDTO list assembler (default display)
    /// </summary>
    /// <param name="tasks"></param>
    /// <returns></returns>
    public IEnumerable<TaskDTO> Assemble (IEnumerable<Lemoine.Model.ITask> tasks)
    {
      Debug.Assert (null != tasks);

      IList<TaskDTO> result = new List<TaskDTO> ();
      foreach (var task in tasks) {
        result.Add (Assemble (task));
      }
      return result;
    }
  }
}
