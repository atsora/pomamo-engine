// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using Lemoine.Model;

namespace WizardReasonSelection
{
  /// <summary>
  /// Description of ReasonParameters.
  /// </summary>
  public class ReasonParameters
  {
    #region Getters / Setters
    /// <summary>
    /// True if the user has to add details for a reason
    /// </summary>
    public bool DetailsRequired { get; private set; }
    
    /// <summary>
    /// Possible machine filter if the reason cannot be applied to all machines
    /// </summary>
    public IMachineFilter MachineFilter { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="cell"></param>
    public ReasonParameters (ReasonCell cell)
    {
      // Default parameters
      DetailsRequired = cell.DetailsRequired;
      MachineFilter = cell.MachineFilter;
    }
    #endregion // Constructors
  }
}
