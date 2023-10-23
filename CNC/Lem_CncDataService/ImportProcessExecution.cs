// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Threading;
using Lemoine.Core.Log;

namespace Lem_CncDataService
{
  /// <summary>
  /// Override of ProcessClassExecution to run ImportCncDataFromQueue
  /// </summary>
  public class ImportProcessExecution: ProcessClassExecution
  {
    static readonly string CNC_DATA_CONSOLE = "Lem_CncDataConsole";
    static readonly ILog log = LogManager.GetLogger(typeof (ImportProcessExecution).FullName);

    #region Getters / Setters
    /// <summary>
    /// Associated ImportCncDataFromQueue
    /// </summary>
    public Lemoine.CncDataImport.ImportCncDataFromQueue ImportCncDataFromQueue { get; set; }
    #endregion // Getters / Setters
    
    /// <summary>
    /// Constructor
    /// </summary>
    public ImportProcessExecution (Lemoine.CncDataImport.ImportCncDataFromQueue importCncDataFromQueue)
      : base (CNC_DATA_CONSOLE, importCncDataFromQueue)
    {
      ImportCncDataFromQueue = importCncDataFromQueue;
    }
    
    /// <summary>
    /// Implements <see cref="ProcessClassExecution">ProcessClassExecution</see>
    /// </summary>
    /// <returns></returns>
    public override string GetSpecificArguments()
    {
      string arguments = string.Format ("-i {0} " +
                                        "-z {1} " +
                                        "-b {2} " +
                                        "-t {3} " +
                                        "-f {4} " +
                                        "-m {5} " +
                                        "-a {6} " +
                                        "-v {7}",
                                        this.ImportCncDataFromQueue.MachineModule.Id,
                                        this.ImportCncDataFromQueue.SleepTime.TotalMilliseconds,
                                        this.ImportCncDataFromQueue.BreakFrequency.TotalMilliseconds,
                                        this.ImportCncDataFromQueue.BreakTime.TotalMilliseconds,
                                        this.ImportCncDataFromQueue.FetchDataNumber,
                                        this.ImportCncDataFromQueue.MinNbOfDataToProcess,
                                        this.ImportCncDataFromQueue.WhicheverNbOfDataProcessAfter,
                                        this.ImportCncDataFromQueue.VisitMachineModesEvery);
      log.DebugFormat ("GetSpecificArguments: " +
                       "arguments are: {0}",
                       arguments);
      return arguments;
    }
    
    /// <summary>
    /// Implements <see cref="ProcessClassExecution">ProcessClassExecution</see>
    /// </summary>
    /// <returns></returns>
    public override int GetId()
    {
      return this.ImportCncDataFromQueue.MachineModule.Id;
    }
  }
}
