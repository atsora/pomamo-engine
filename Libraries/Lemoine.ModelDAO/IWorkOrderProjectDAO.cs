// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// 
  /// </summary>
  public interface IWorkOrderProjectDAO: IGenericUpdateDAO<IWorkOrderProject, int>
  {
    /// <summary>
    /// Get the unique <see cref="IWorkOrderProject"/>
    /// given the associated work order and project
    /// </summary>
    /// <param name="workOrder">not null</param>
    /// <param name="project">not null</param>
    /// <returns></returns>
    IWorkOrderProject Get (IWorkOrder workOrder, IProject project);
  }
}
