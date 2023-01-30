// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Lemoine.Settings;

namespace WizardReasonSelection
{
  /// <summary>
  /// The only class exported by the dll, which implements IItemDll
  /// Instances of IItem can be created:
  /// - based on an itemConfig, in the method AddConfig
  /// - without the use of an itemConfig
  /// </summary>
  public class ItemDll: IItemDll
  {
    /// <summary>
    /// Create items unrelated to configurations
    /// </summary>
    /// <param name="context">Context in which items are created</param>
    /// <returns>List of items, may be null</returns>
    public IList<IItem> CreateItems(ItemContext context)
    {
      var items = new List<IItem>();
      items.Add(new Item() as IItem);
      return items;
    }
    
    /// <summary>
    /// Create items based on configurations
    /// </summary>
    /// <param name="context">Context in which items are created</param>
    /// <param name="config">Configuration used for the creation</param>
    /// <returns>List of items, may be null</returns>
    public IList<IItem> CreateItems(ItemContext context, ItemConfig config)
    {
      return null;
    }
  }
}