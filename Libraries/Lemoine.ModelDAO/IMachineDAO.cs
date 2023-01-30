// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// DAO interface for IMachine.
  /// </summary>
  public interface IMachineDAO: IGenericUpdateDAO<IMachine, int>
  {
    /// <summary>
    /// Find all machines trying to use the cache first
    /// 
    /// Note that the machines may be detached
    /// </summary>
    /// <param name="useCache"></param>
    /// <returns></returns>
    IList<IMachine> FindAll (bool useCache);

    /// <summary>
    /// Find all IMachine for XML serialization
    /// </summary>
    /// <returns></returns>
    IList<IMachine> FindAllForXmlSerialization ();

    /// <summary>
    /// Find all IMachine, but get also in the same all its children
    /// </summary>
    /// <returns></returns>
    IList<IMachine> FindAllWithChildren ();
    
    /// <summary>
    /// Find an IMachine using its name <paramref name="machineName"/>
    /// </summary>
    /// <param name="machineName"></param>
    /// <returns></returns>
    IMachine FindByName(string machineName);
    
    /// <summary>
    /// Find all IMachine ordered by name
    /// </summary>
    /// <returns></returns>
    IList<IMachine> FindAllOrderByName ();
    
    /// <summary>
    /// Find all IMachine that are not obsolete
    /// </summary>
    /// <returns></returns>
    IList<IMachine> FindAllNotObsolete();

    /// <summary>
    /// Find all IMachine with given company id
    /// </summary>
    /// <param name="companyId"></param>
    /// <returns></returns>
    IList<IMachine> FindAllInCompany (int companyId);

    /// <summary>
    /// Find all IMachine with given department id
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns></returns>
    IList<IMachine> FindAllInDepartment (int departmentId);

    /// <summary>
    /// Find all IMachine with given machine category id
    /// </summary>
    /// <param name="machineCategoryId"></param>
    /// <returns></returns>
    IList<IMachine> FindAllInMachineCategory (int machineCategoryId);

    /// <summary>
    /// Find all IMachine with given machine sub-category id
    /// </summary>
    /// <param name="machineSubCategoryId"></param>
    /// <returns></returns>
    IList<IMachine> FindAllInMachineSubCategory (int machineSubCategoryId);

    /// <summary>
    /// Find all IMachine with given cell id
    /// </summary>
    /// <param name="cellId"></param>
    /// <returns></returns>
    IList<IMachine> FindAllInCell (int cellId);
  }
}
