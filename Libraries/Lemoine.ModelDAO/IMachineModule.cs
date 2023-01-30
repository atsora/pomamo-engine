// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

namespace Lemoine.Model
{
  /// <summary>
  /// Activity data to take into account to record the 'auto-only' sequences
  /// </summary>
  public enum MachineModuleAutoSequenceActivity
  {
    /// <summary>
    /// No auto-sequence activity
    /// </summary>
    None = 0,
    /// <summary>
    /// The global Machine activity and the fact table
    /// </summary>
    Machine = 1,
    /// <summary>
    /// The machine module activity and the machinemoduleactivity table
    /// </summary>
    MachineModule = 2
  }

  /// <summary>
  /// Model for the table MachineModule
  /// 
  /// This table replaces with the Machine and Monitored Machine tables the old sfkmach table
  /// 
  /// A monitored machine may be made of several independent modules.
  /// Each module reports its own set of data.
  /// A module can represent for example a Machine Path.
  /// </summary>
  public interface IMachineModule: ISelectionable, IDataWithVersion, ISerializableModel, IDisplayable
  {
    // Note: IMachineModule does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }
    
    /// <summary>
    /// Name of the module
    /// 
    /// Unique for a given associated monitored machine
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// Code of the module
    /// 
    /// Unique for a given associated monitored machine
    /// </summary>
    string Code { get; set; }
    
    /// <summary>
    /// External code
    /// 
    /// It may help synchronizing our data with an external database
    /// 
    /// Unique
    /// </summary>
    string ExternalCode { get; set; }
    
    /// <summary>
    /// Cnc acquisition module that drives this machine module
    /// </summary>
    ICncAcquisition CncAcquisition { get; set; }
    
    /// <summary>
    /// Configuration prefix found in the XML configuration file
    /// </summary>
    string ConfigPrefix { get; set; }
    
    /// <summary>
    /// Parameters to use in the CNC service
    /// 
    /// The first character is the separators that is used to separate the different parameters.
    /// 
    /// The parameters are made in the XML configuration file
    /// by a config prefix and a parameter number
    /// </summary>
    string ConfigParameters { get; set; }
    
    /// <summary>
    /// Reference to the associated monitored machine
    /// </summary>
    IMonitoredMachine MonitoredMachine { get; set; }
    
    /// <summary>
    /// Sequence variable/macro for this machine module
    /// </summary>
    string SequenceVariable { get; set; }

    /// <summary>
    /// Milestone variable/macro for this machine module
    /// </summary>
    string MilestoneVariable { get; set; }

    /// <summary>
    /// Cycle variable/macro
    /// </summary>
    string CycleVariable { get; set; }
    
    /// <summary>
    /// Start Cycle variable/macro
    /// </summary>
    string StartCycleVariable { get; set; }
    
    /// <summary>
    /// Sequence detection method
    /// 
    /// Default: stamp (1024)
    /// </summary>
    SequenceDetectionMethod SequenceDetectionMethod { get; set; }
    
    /// <summary>
    /// End cycle detection method
    /// 
    /// Default: stamp (8)
    /// </summary>
    CycleDetectionMethod CycleDetectionMethod { get; set; }
    
    /// <summary>
    /// Start cycle detection method
    /// 
    /// Default: stamp (1)
    /// </summary>
    StartCycleDetectionMethod StartCycleDetectionMethod { get; set; }
    
    /// <summary>
    /// Default detection method
    /// 
    /// This is combination of Sequence/Cycle/StartCycle detection methods
    /// </summary>
    DetectionMethod DefaultDetectionMethod { get; }

      /// <summary>
    /// Detection method variable/macro
    /// </summary>
    string DetectionMethodVariable { get; set; }
    
    /// <summary>
    /// Auto-sequence activity reference data
    /// </summary>
    MachineModuleAutoSequenceActivity AutoSequenceActivity { get; set; }
  }

  /// <summary>
  /// IMachineModuleExtensions
  /// </summary>
  public static class IMachineModuleExtensions
  {
    /// <summary>
    /// Call the ToString () method only if initialized
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string ToStringIfInitialized (this IMachineModule data)
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
