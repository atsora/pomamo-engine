// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Model;

namespace Lemoine.Settings
{
  /// <summary>
  /// Special functions provided by Wizards
  /// </summary>
  public interface IWizard: IItem
  {
    /// <summary>
    /// All pages provided by the wizard
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    IList<IWizardPage> Pages { get; }
    
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    ItemData Initialize(ItemData otherData);
    
    /// <summary>
    /// All settings are done, changes will take effect
    /// This method is already within a try / catch
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <param name="warnings">List of warnings when the function returns (full sentences desired)</param>
    /// <param name="revision">Revision that is going to be applied when the function returns</param>
    void Finalize(ItemData data, ref IList<string> warnings, ref IRevision revision);
    
    /// <summary>
    /// Get a list of items for proposing the next possible steps to the user
    /// once the current wizard is successfully finished
    /// The key of the dictionary to return has the pattern {0}-{1} or {0}-{1}-{2}, where:
    /// - {0} is the item id,
    /// - {1} is the item subid,
    /// - {2} is the ViewMode (True or False)
    /// The value of the dictionary to return is an infinive text such as "monitor the machine"
    /// </summary>
    /// <param name="data">Data filled through the wizard</param>
    /// <returns>See the description above, can be null</returns>
    IDictionary<string, string> GetPossibleNextItems(ItemData data);
  }
}
