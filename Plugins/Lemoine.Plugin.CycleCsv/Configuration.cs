// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.CycleCsv
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration: Lemoine.Extensions.Configuration.IConfiguration
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// Directory path where the CSV files are stored
    /// </summary>
    public string Path { get; set;  }

    /// <summary>
    /// CSV separator
    /// </summary>
    public string Separator { get; set; }

    /// <summary>
    /// Header of the Operation Cycle UTC start date/time
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string OperationCycleUtcStartHeader { get; set; }

    /// <summary>
    /// Header of the Operation Cycle UTC stop date/time
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string OperationCycleUtcStopHeader { get; set; }

    /// <summary>
    /// Header of the Operation Cycle local start date/time
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string OperationCycleLocalStartHeader { get; set; }

    /// <summary>
    /// Header of the Operation Cycle local stop date/time
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string OperationCycleLocalStopHeader { get; set; }

    /// <summary>
    /// Header of the Machine name
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string MachineNameHeader { get; set; }

    /// <summary>
    /// Header of the Machine code
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string MachineCodeHeader { get; set; }

    /// <summary>
    /// Header of the Machine external code
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string MachineExternalCodeHeader { get; set; }

    /// <summary>
    /// Header of the Machine display
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string MachineDisplayHeader { get; set; }

    /// <summary>
    /// Header of the component name
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string ComponentNameHeader { get; set; }

    /// <summary>
    /// Header of the component code
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string ComponentCodeHeader { get; set; }

    /// <summary>
    /// Header of the operation name
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string OperationNameHeader { get; set; }

    /// <summary>
    /// Header of the operation code
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string OperationCodeHeader { get; set; }

    /// <summary>
    /// Header of the running time (in s)
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string RunningDurationHeader { get; set; }

    /// <summary>
    /// Header of the not running time (in s)
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string NotRunningDurationHeader { get; set; }

    /// <summary>
    /// Header of the task id
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string TaskIdHeader { get; set; }

    /// <summary>
    /// Header of the task external code
    /// 
    /// If empty, dismiss it
    /// </summary>
    public string TaskExternalCodeHeader { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Configuration ()
    {
    }
    #endregion // Constructors

    #region Methods
    public bool IsValid (out IEnumerable<string> errors)
    {
      errors = new List<string> ();

      // TODO: check 

      return true;
    }
    #endregion // Methods
  }
}
