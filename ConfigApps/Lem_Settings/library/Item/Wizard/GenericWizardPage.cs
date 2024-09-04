// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Settings
{
  /// <summary>
  /// Description of GenericWizardPage.
  /// </summary>
  public class GenericWizardPage : GenericItemPage
  {
    #region Getters / Setters
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    public virtual IList<Type> EditableTypes { get { return null; } }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public GenericWizardPage() : base() {}
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Called after SavePage and before going to the previous page
    /// </summary>
    /// <param name="data"></param>
    public virtual void DoSomethingBeforePrevious(ItemData data) {}
    
    /// <summary>
    /// Get the list of failures that have to be fixed before we can access the next page.
    /// Use short sentences, no uppercase at the beginning and no punctuation.
    /// </summary>
    /// <returns>List of errors, can be null</returns>
    public virtual IList<string> GetErrorsToGoNext(ItemData data) { return null; }
    
    /// <summary>
    /// If no errors are found to go to the next page, non-blocking warnings are checked.
    /// Data may be slightly modified here before continuing to the next page.
    /// Full sentences are desired here.
    /// </summary>
    /// <returns>List of warnings, can be null</returns>
    public virtual IList<string> GetWarnings(ItemData data) { return null; }
    #endregion // Methods
  }
}
