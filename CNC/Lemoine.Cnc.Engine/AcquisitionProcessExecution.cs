// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Lemoine.Core.Log;
using Lemoine.Threading;

namespace Lemoine.CncEngine
{
  /// <summary>
  /// Description of ProcessCncAcquisition.
  /// </summary>
  public class AcquisitionProcessExecution : ProcessClassExecution
  {
    static readonly string CNC_CONSOLE = "Lem_CncConsole";

    #region Getters / Setters
    /// <summary>
    /// Associated Lemoine.Cnc.Acquisition
    /// </summary>
    public Acquisition Acquisition { get; set; }
    #endregion // Getters / Setters

    /// <summary>
    /// Constructor
    /// </summary>
    public AcquisitionProcessExecution (Acquisition acquisition)
      : base (CNC_CONSOLE, acquisition)
    {
      this.Acquisition = acquisition;
    }

    /// <summary>
    /// Implements <see cref="ProcessClassExecution">ProcessClassExecution</see>
    /// </summary>
    /// <returns></returns>
    public override string GetSpecificArguments ()
    {

      return string.Format ("-i {0} " +
                            "-e {1}",
                            this.Acquisition.CncAcquisitionId,
                            this.Acquisition.Every.TotalMilliseconds);
    }

    /// <summary>
    /// Implements <see cref="ProcessClassExecution">ProcessClassExecution</see>
    /// </summary>
    /// <returns></returns>
    public override int GetId ()
    {
      return this.Acquisition.CncAcquisitionId;
    }
  }
}
