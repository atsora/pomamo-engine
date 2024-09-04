// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Settings
{
  /// <summary>
  /// Special functions provided by Configurators
  /// </summary>
  public interface IConfigurator : IItem
  {
    /// <summary>
    /// All pages provided by the configurator
    /// The first page in the list will be displayed first
    /// The order is then not important
    /// </summary>
    IList<IConfiguratorPage> Pages { get; }
    
    /// <summary>
    /// Initialization of the data that is going to pass through all pages
    /// All values - except for GUI parameter - must be defined in common data
    /// </summary>
    /// <param name="otherItemData">configuration from another item, can be null</param>
    /// <returns>data initialized</returns>
    ItemData Initialize(ItemData otherItemData);
  }
}
