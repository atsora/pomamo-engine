// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Model
{
  /// <summary>
  /// Cnc module license
  /// </summary>
  [Flags]
  public enum CncModuleLicense
  {
    /// <summary>
    /// No specific license
    /// </summary>
    None = 0,
    /// <summary>
    /// Gpl
    /// </summary>
    Gpl = 1,
    /// <summary>
    /// Proprietary
    /// </summary>
    Proprietary = 2,
  }

  /// <summary>
  /// Cnc acquisition module
  /// 
  /// Such a cnc acquisition module may drive the data acquisition
  /// of one or several machine modules
  /// </summary>
  public interface ICncAcquisition: ISelectionable, IDataWithVersion, ISerializableModel
  {
    // Note: ICncAcquisition does not inherit from IVersionable
    //       else the corresponding properties can't be used in a DataGridView binding
    
    /// <summary>
    /// ID
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Name
    /// 
    /// Note an empty string is converted to null.
    /// </summary>
    string Name { get; set; }
    
    /// <summary>
    /// XML configuration file
    /// </summary>
    string ConfigFile { get; set; }
    
    /// <summary>
    /// Prefix used in the XML configuration file to reference some parameters
    /// 
    /// Default is Param
    /// </summary>
    string ConfigPrefix { get; set; }
    
    /// <summary>
    /// Parameters used in XML configuration file.
    /// 
    /// The first character in this string is the separator to use to separate the different parameters
    /// </summary>
    string ConfigParameters { get; set; }
    
    /// <summary>
    /// Use a process instead of a thread
    /// 
    /// This may useful when the acquisition modules uses some unmanaged code
    /// </summary>
    bool UseProcess { get; set; }

    /// <summary>
    /// True if the thread must be run in a single thread apartment
    /// </summary>
    bool StaThread { get; set; }

    /// <summary>
    /// Use the service in .Net Core Lem_CncCoreService
    /// </summary>
    bool UseCoreService { get; set; }

    /// <summary>
    /// Frequency when the data is acquired
    /// 
    /// Default is every 2 seconds
    /// </summary>
    TimeSpan Every { get; set; }

    /// <summary>
    /// Time after which the acquisition module is considered as not responding any more
    /// 
    /// Default is 2 minutes
    /// </summary>
    TimeSpan NotRespondingTimeout { get; set; }
    
    /// <summary>
    /// In case the machine module acquisition was stopped (because it was considered as not responding any more),
    /// time after which the acquisition is restarted
    /// 
    /// Default is 10 seconds
    /// </summary>
    TimeSpan SleepBeforeRestart { get; set; }
    
    /// <summary>
    /// List of machine modules this Cnc Acquisition drives
    /// </summary>
    ICollection<IMachineModule> MachineModules { get; }
    
    /// <summary>
    /// Computer on which the acquisition is made
    /// </summary>
    IComputer Computer { get; set; }

    /// <summary>
    /// License that is associated to a cnc acquisition
    /// </summary>
    CncModuleLicense License { get; set; }
  }
}
