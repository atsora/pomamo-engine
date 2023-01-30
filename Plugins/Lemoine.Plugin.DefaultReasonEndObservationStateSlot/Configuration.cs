// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.Plugin.DefaultReasonEndObservationStateSlot
{
  /// <summary>
  /// Description of Configuration.
  /// </summary>
  public class Configuration: Lemoine.Extensions.Configuration.IConfiguration
  {
    #region Members
    readonly IList<string> m_errors = new List<string> ();
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (Configuration).FullName);

    #region Getters / Setters
    /// <summary>
    /// List of machine mode IDs when this is applicable
    /// </summary>
    public IList<int> MachineModeIds { get; set; } = new List<int> ();
    
    /// <summary>
    /// ID of the current machine observation state when this is applicable
    /// </summary>
    public int CurrentMachineObservationStateId { get; set; }

    /// <summary>
    /// ID of the new machine observation state when this is applicable
    /// 
    /// null: any
    /// </summary>
    public int? NextMachineObservationStateId { get; set; }
    
    /// <summary>
    /// Check if the shift of the next observation state slot should be the same or not
    /// 
    /// If null, not check is done
    /// </summary>
    public bool? IsNextSameShift { get; set; }
    
    /// <summary>
    /// ID of the reason to apply if all the other criteria match
    /// </summary>
    public int ReasonId { get; set; }
    
    /// <summary>
    /// Reason score
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Overwrite Required value to apply if all the other criteria match
    /// </summary>
    public bool OverwriteRequired { get; set; }
    
    /// <summary>
    /// Details to use
    /// </summary>
    public string Details { get; set; }
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

      // TODO: check ths IDs

      return true;
    }
    #endregion // Methods
  }
}
