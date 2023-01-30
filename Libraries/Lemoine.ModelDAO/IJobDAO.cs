// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IJob.
  /// </summary>
  public interface IJobDAO: IGenericUpdateDAO<IJob, int>
    , IMergeDAO<IJob>
  {
    /// <summary>
    /// Find a job by its WorkOrderId
    /// </summary>
    /// <param name="workOrderId"></param>
    /// <returns></returns>
    IJob FindByWorkOrderId (int workOrderId);

    /// <summary>
    /// Find a job by its ProjectId
    /// </summary>
    /// <param name="projectId"></param>
    /// <returns></returns>
    IJob FindByProjectId (int projectId);
  }
}
