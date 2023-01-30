// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.ModelDAO
{
  /// <summary>
  /// Description of IMonitoredMachineDAO.
  /// </summary>
  public interface IMonitoredMachineDAO: IGenericUpdateDAO<IMonitoredMachine, int>
  {    
    /// <summary>
    /// Find by Id with an eager fetch of the machine modules
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IMonitoredMachine FindByIdWithMachineModules (int id);

    /// <summary>
    /// Find by Id with an eager fetch of the monitoring type
    /// </summary>
    /// <returns></returns>
    IMonitoredMachine FindByIdForXmlSerialization (int id);

    /// <summary>
    /// Find by Id with an eager fetch of:
    /// <item>the main machine module</item>
    /// <item>the performance field</item>
    /// <item>the unit of the performance field</item>
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    IMonitoredMachine FindByIdWithMainMachineModulePerformanceFieldUnit (int id);
    
    /// <summary>
    /// Find all IMonitoredMachine for XML serialization
    /// </summary>
    /// <returns></returns>
    IList<IMonitoredMachine> FindAllForXmlSerialization ();

      /// <summary>
    /// Find all IMonitoredMachine, but get also the MachineModule child elements
    /// </summary>
    /// <returns></returns>
    IList<IMonitoredMachine> FindAllWithMachineModule ();
    
    /// <summary>
    /// Find all IMonitoredMachine, but get also the Cnc Acquisition child elements
    /// </summary>
    /// <returns></returns>
    IList<IMonitoredMachine> FindAllWithCncAcquisition ();
    
    /// <summary>
    /// Find all IMonitoredMachine to configure them.
    /// 
    /// That means get also:
    /// <item>all its Cnc Acquisition child elements</item>
    /// <item>its performance field</item>
    /// </summary>
    /// <returns></returns>
    IList<IMonitoredMachine> FindAllForConfig ();
    
    /// <summary>
    /// Find the IMonitoredMachine corresponding to a machine name
    /// 
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machineName"></param>
    /// <returns></returns>
    IMonitoredMachine FindByName(string machineName);
    
    /// <summary>
    /// Find the IMonitoredMachine corresponding to a machine
    /// Note: this is registered to be cacheable
    /// </summary>
    /// <param name="machine"></param>
    /// <returns></returns>
    IMonitoredMachine FindByMachine(IMachine machine);
    
    /// <summary>
    /// Find all IMonitoredMachine ordered by name
    /// </summary>
    /// <returns></returns>
    IList<IMonitoredMachine> FindAllOrderByName ();
    
    /// <summary>
    /// Find all IMonitoredMachine in a given department with a not-null company
    /// </summary>
    /// <param name="departmentId"></param>
    /// <returns></returns>
    IList<IMonitoredMachine> FindAllByDepartment (int departmentId);
    
    /// <summary>
    /// Find all IMonitoredMachine in a given category with a not-null company
    /// </summary>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    IList<IMonitoredMachine> FindAllByCategory (int categoryId);

    /// <summary>
    /// Find all the IMonitoredMachine that matches the specified stampingConfigByName
    /// </summary>
    /// <param name="stampingConfigByName">not null</param>
    /// <returns></returns>
    IList<IMonitoredMachine> FindByStampingConfig (IStampingConfigByName stampingConfigByName);
  }
}
