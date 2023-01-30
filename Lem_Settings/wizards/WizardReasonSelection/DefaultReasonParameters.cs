// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace WizardReasonSelection
{
  /// <summary>
  /// Description of DefaultReasonParameters.
  /// </summary>
  public class DefaultReasonParameters
  {
    #region Getters / Setters
    /// <summary>
    /// True if the user has to overwrite a default reason
    /// </summary>
    public bool OverwriteRequired { get; private set; }
    
    /// <summary>
    /// Possible machine filter that include some machines
    /// </summary>
    public IMachineFilter MachineFilterInclude { get; private set; }
    
    /// <summary>
    /// Possible machine filter that exclude some machines
    /// </summary>
    public IMachineFilter MachineFilterExclude { get; private set; }
    
    /// <summary>
    /// Maximum time a default can be applied
    /// </summary>
    public TimeSpan? MaxTime { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="cell"></param>
    public DefaultReasonParameters (DefaultReasonCell cell)
    {
      // Default parameters
      OverwriteRequired = cell.OverwriteRequired;
      MachineFilterInclude = cell.MachineFilterInclude;
      MachineFilterExclude = cell.MachineFilterExclude;
      MaxTime = cell.MaxTime;
    }
    #endregion // Constructors
  }
}
