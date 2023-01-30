// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;
using System.Collections.Generic;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IWorkOrder.
  /// </summary>
  public interface IWorkOrderDAO: IGenericUpdateDAO<IWorkOrder, int>
    , IMergeDAO<IWorkOrder>
  {
    /// <summary>
    /// Try to get the WorkOrder entity matching an external code
    /// <param name="workOrderExternalCode"></param>
    /// </summary>
    IWorkOrder FindByExternalCode(string workOrderExternalCode);
    
    /// <summary>
    /// Try to get the WorkOrder entity matching a code
    /// <param name="workOrderCode"></param>
    /// </summary>
    IWorkOrder FindByCode(string workOrderCode);
    
    /// <summary>
    /// Try to get the WorkOrder entity matching a name
    /// <param name="workOrderName"></param>
    /// </summary>
    IWorkOrder FindByName(string workOrderName);
    
    /// <summary>
    /// Tests if others WorkOrder have same name  like WorkOrder with given id
    /// </summary>
    /// <param name="name"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Boolean IfExistsOtherWithSameName(String name, int id);

    /// <summary>
    /// Tests if others WorkOrder have same code like WorkOrder with given id
    /// </summary>
    /// <param name="code"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    Boolean IfExistsOtherWithSameCode(String code, int id);

    /// <summary>
    /// Tests if exists WorkOrder have same name
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    Boolean IfExistsWithSameName(String name);
    
    /// <summary>
    /// Tests if exists WorkOrder have same code
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    Boolean IfExistsWithSameCode(String code);

    /// <summary>
    /// return all WorkOrder with loading associated Project
    /// </summary>
    /// <returns></returns>
    IList<IWorkOrder> FindAllEager();

    /// <summary>
    /// Initialize the associated projects
    /// </summary>
    /// <param name="workOrder"></param>
    void InitializeProjects (IWorkOrder workOrder);
  }
}
