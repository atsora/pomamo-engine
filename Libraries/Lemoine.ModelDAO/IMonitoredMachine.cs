// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Set of possible data to display in the operation bar
  /// </summary>
  public enum OperationBar
  {
    /// <summary>
    /// Nothing in the operation bar
    /// </summary>
    None = 0,
    /// <summary>
    /// Operation slots in the operation bar
    /// </summary>
    Operation = 1
  }
  
  /// <summary>
  /// Model of table MonitoredMachine
  /// </summary>
  public interface IMonitoredMachine: IMachine
  {
    /// <summary>
    /// Set of machine modules this monitored machine is made of
    /// </summary>
    ICollection<IMachineModule> MachineModules { get; }
    
    /// <summary>
    /// Reference to the main machine module
    /// </summary>
    IMachineModule MainMachineModule { get; set; }
    
    /// <summary>
    /// Reference to the CncAcquisition of the main machine module
    /// </summary>
    ICncAcquisition MainCncAcquisition { get; set; }

    /// <summary>
    /// Reference to the performance field that is associated to this monitored machine
    /// </summary>
    IField PerformanceField { get; set; }

    /// <summary>
    /// Get/Set what kind of data is displayed in the operation bar
    /// </summary>
    OperationBar OperationBar { get; set; }
    
    /// <summary>
    /// Pallet changing duration, null if not applicable
    /// </summary>
    Nullable<TimeSpan> PalletChangingDuration { get;set; }
    
    /// <summary>
    /// Is operation information given by CNC (i.e. stamped files) or not for this machine ?
    /// </summary>
    bool OperationFromCnc { get; set; }
    
    /// <summary>
    /// Add a machine module
    /// </summary>
    /// <param name="machineModule"></param>
    void AddMachineModule (IMachineModule machineModule);

    /// <summary>
    /// Associated stamping config by name
    /// 
    /// nullable
    /// </summary>
    IStampingConfigByName StampingConfigByName { get; set; }
  }
  
  /// <summary>
  /// Monitored machine interface for the data grid views (to bypass a limitation of the data grid views)
  /// </summary>
  public interface IDataGridViewMonitoredMachine: IDataWithIdentifiers, IDisplayable, ISelectionable
  {
    // Note: IMachine does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Version
    /// </summary>
    int Version { get; }

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
    /// Associated category
    /// </summary>
    IMachineCategory Category { get; set; }
    
    /// <summary>
    /// Associated sub-category
    /// </summary>
    IMachineSubCategory SubCategory { get; set; }

    /// <summary>
    /// Set of machine modules this monitored machine is made of
    /// </summary>
    ICollection<IMachineModule> MachineModules { get; }
    
    /// <summary>
    /// Reference to the main machine module
    /// </summary>
    IMachineModule MainMachineModule { get; set; }
    
    /// <summary>
    /// Reference to the CncAcquisition of the main machine module
    /// </summary>
    ICncAcquisition MainCncAcquisition { get; set; }
    
    /// <summary>
    /// Reference to the performance field that is associated to this monitored machine
    /// </summary>
    IField PerformanceField { get; set; }
    
    /// <summary>
    /// Get/Set what kind of data is displayed in the operation bar
    /// </summary>
    OperationBar OperationBar { get; set; }

    /// <summary>
    /// Reference to the stamping config by name
    /// </summary>
    IStampingConfigByName StampingConfigByName { get; set; }


    /// <summary>
    /// Pallet changing duration, null if not applicable
    /// </summary>
    Nullable<TimeSpan> PalletChangingDuration { get;set; }
    
    /// <summary>
    /// OperationFromCnc
    /// </summary>
    bool OperationFromCnc { get; set; }
  }
}
