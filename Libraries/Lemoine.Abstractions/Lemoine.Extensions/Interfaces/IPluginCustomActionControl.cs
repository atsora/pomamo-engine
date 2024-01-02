// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;

using System.Collections.Generic;
using System.Linq;

namespace Lemoine.Extensions.Interfaces
{
  /// <summary>
  /// Interface for a plugin custom action control
  /// </summary>
  public interface IPluginCustomActionControl
  {
    #region Getters / Setters
    /// <summary>
    /// Title of the action
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Help text for the configuration
    /// </summary>
    string Help { get; }
    #endregion // Getters / Setters

    #region Methods
    /// <summary>
    /// Initialize the interface
    /// </summary>
    void InitializeInterface ();

    /// <summary>
    /// Action done when validated
    /// This method is NOT called within a transaction
    /// </summary>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision Id that is going to be applied when the function returns</param>
    void DoAction (ref IList<string> warnings, ref int revisionId);

    /// <summary>
    /// Get the configuration errors
    /// </summary>
    /// <returns></returns>
    IList<string> GetErrors ();
    #endregion // Methods
  }
}
