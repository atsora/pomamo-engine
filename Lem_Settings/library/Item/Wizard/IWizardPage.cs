// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Settings
{
  /// <summary>
  /// Special functions of a wizard page
  /// </summary>
  public interface IWizardPage: IItemPage
  {
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    IList<Type> EditableTypes { get; }
    
    /// <summary>
    /// Called after SavePage and before going to the previous page
    /// </summary>
    /// <param name="data"></param>
    void DoSomethingBeforePrevious(ItemData data);
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of errors, can be null</returns>
    IList<string> GetErrorsToGoNext(ItemData data);
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>List of warnings, can be null</returns>
    IList<string> GetWarnings(ItemData data);
    
    /// <summary>
    /// Get the name of the next page (skipping one or several pages is possible)
    /// An empty string or a null value will show the summary page
    /// </summary>
    /// <param name="data"></param>
    /// <returns>class name of the next page</returns>
    string GetNextPageName(ItemData data);
    
    /// <summary>
    /// Get a summary of the user inputs, which will be displayed in a tree
    /// Each string in the list will be a rootnode. If line breaks are present, all lines
    /// after the first one will be shown as child nodes.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    IList<string> GetSummary(ItemData data);
  }
}
