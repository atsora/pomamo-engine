// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Model for table Machine
  /// 
  /// This table replaces with the Monitored Machine
  /// and Machine Path tables the old sfkmach table.
  /// 
  /// This new table extends the concept of machine
  /// to include also the outsource resources and the not monitored machines.
  /// </summary>
  public interface IMachine
    : IDataWithIdentifiers, IDisplayable, ISelectionable, IDataWithVersion
    , ISerializableModel
    , IMachineFilterItemSet
    , IComparable, IComparable<IMachine>
  {
    // Note: IMachine does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name of the machining resource.
    /// 
    /// In case it is a monitored machine,
    /// it is the same than the name of the corresponding Monitored Machine
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code used in some companies to identify a machining resource
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// Machine external code
    /// 
    /// It may help synchronizing our data with an external database
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Reference to the Machine Monitoring Type table
    /// </summary>
    IMachineMonitoringType MonitoringType { get; set; }
    
    /// <summary>
    /// Priority to use to display it in the reports or in the applications
    /// </summary>
    int? DisplayPriority { get; set; }
    
    /// <summary>
    /// Associated company
    /// </summary>
    ICompany Company { get; set; }
    
    /// <summary>
    /// Associated department
    /// </summary>
    IDepartment Department { get; set; }
    
    /// <summary>
    /// Associated cell
    /// </summary>
    ICell Cell { get; set; }
    
    /// <summary>
    /// Associated category
    /// </summary>
    IMachineCategory Category { get; set; }
    
    /// <summary>
    /// Associated sub-category
    /// </summary>
    IMachineSubCategory SubCategory { get; set; }
    
    /// <summary>
    /// Is the machine monitored ?
    /// </summary>
    /// <returns></returns>
    bool IsMonitored ();
    
    /// <summary>
    /// Is the machine obsolete?
    /// </summary>
    /// <returns></returns>
    bool IsObsolete();
  }


  /// <summary>
  /// IMachineModuleExtensions
  /// </summary>
  public static class IMachineExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IMachine data)
    {
      if (ModelDAO.ModelDAOHelper.DAOFactory.IsInitialized (data)) {
        return data.ToString ();
      }
      else {
        return $"[{data.GetType ().Name} {data.Id}]";
      }
    }
  }
}
