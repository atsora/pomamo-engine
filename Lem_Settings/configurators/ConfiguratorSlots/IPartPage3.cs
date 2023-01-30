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
  /// Description of AbstractPartPage3.
  /// </summary>
  public interface IPartPage3
  {
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
    /// Prepare a modification to save
    /// This method is within a transaction
    /// </summary>
    /// <param name="data"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    IModification PrepareModification(ItemData data, object item);
    
    /// <summary>
    /// Get the name of the elements that are going to be modified
    /// </summary>
    /// <param name="items"></param>
    /// <returns>list of name, may be null</returns>
    IList<string> GetElementName(IList<object> items);
    
    /// <summary>
    /// Name of the category of element, plural with uppercase for the first letter
    /// Ex: "Machines"
    /// </summary>
    string ElementName { get; }
    
    /// <summary>
    /// List of classes which are allowed to be edited externally, may be null
    /// An update will trigger a call to Initialize()
    /// </summary>
    IList<Type> EditableTypes { get; }
    
    /// <summary>
    /// Description of the page
    /// </summary>
    string Description { get; }
  }
}
