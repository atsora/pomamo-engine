// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;

namespace Lemoine.Settings
{
  /// <summary>
  /// Interface that should implement every dll to be loaded
  /// </summary>
  public interface IItemDll
  {
    /// <summary>
    /// Create items unrelated to configurations
    /// </summary>
    /// <param name="context">Context in which items are created</param>
    /// <returns>List of items, may be null</returns>
    IList<IItem> CreateItems(ItemContext context);
    
    /// <summary>
    /// Create items based on configurations
    /// </summary>
    /// <param name="context">Context in which items are created</param>
    /// <param name="config">Configuration used for the creation</param>
    /// <returns>List of items, may be null</returns>
    IList<IItem> CreateItems(ItemContext context, ItemConfig config);
  }
}
