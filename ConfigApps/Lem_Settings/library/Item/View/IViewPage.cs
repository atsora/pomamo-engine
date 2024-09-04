// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Settings
{
  /// <summary>
  /// Special functions of a view page
  /// </summary>
  public interface IViewPage: IItemPage
  {
    /// <summary>
    /// Event emitted when another page has to be displayed
    /// First argument is the name of the page to call
    /// Second argument is the list of errors that could cancel the call (may be null)
    /// Third argument is true if SuperAdmin can click on "Ignore"
    /// </summary>
    event Action<string, IList<string>, bool> DisplayPageEvent;

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
    /// <param name="revisionId">Revision that is going to be applied when the function returns (not used for views)</param>
    void Validate(ItemData data, ref IList<string> warnings, ref int revisionId);

    /// <summary>
    /// If the validation step is enabled, method called after the validation and after the possible progress
    /// bar linked to a revision (the user or the timeout could have canceled the progress bar but in that
    /// case a warning is displayed).
    /// Don't forget to emit "DataChangedEvent" if data changed
    /// </summary>
    /// <param name="data">data that can be processed before the page changes</param>
    void ProcessAfterValidation(ItemData data);
    
    /// <summary>
    /// If the validation step is enabled, class name of the page to go after having validated
    /// The name must be a previous page otherwise this will have no effects
    /// A null or an empty value keeps the normal behavious (the previous page is displayed after validating)
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    string GetPageAfterValidation(ItemData data);
  }
}
