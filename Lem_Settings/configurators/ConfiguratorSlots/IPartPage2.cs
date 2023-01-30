// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;
using Lemoine.Model;

namespace ConfiguratorSlots
{
  /// <summary>
  /// Description of IPartPage2.
  /// </summary>
  public interface IPartPage2
  {
    /// <summary>
    /// Title
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Some help regarding the page
    /// </summary>
    string Help { get; }

    /// <summary>
    /// Initialization of the page, happening before the first load
    /// This method is called again if the database has been updated
    /// </summary>
    void Initialize();
    
    /// <summary>
    /// Load the page from the data
    /// </summary>
    /// <param name="data">data containing parameters</param>
    void LoadPageFromData(ItemData data);
    
    /// <summary>
    /// Save the page in the data
    /// </summary>
    /// <param name="data">data to be modified</param>
    void SavePageInData(ItemData data);
    
    /// <summary>
    /// If the validation step is enabled, get the list of errors before validating
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data">data to check</param>
    /// <returns>list of errors, can be null</returns>
    IList<string> GetErrorsBeforeValidation(ItemData data);
    
    /// <summary>
    /// If the validation step is enabled, this method will be called after
    /// GetErrorsBeforeValidation()
    /// </summary>
    /// <param name="data">data to validate</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    void Validate(ItemData data, ref IList<string> warnings, ref int revisionId);
  }
}
