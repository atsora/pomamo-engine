// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;

namespace Lemoine.Settings
{
  /// <summary>
  /// Interface identifying all items
  /// </summary>
  public interface IItem: IComparable
  {
    /// <summary>
    /// ID related to the dll which created the item
    /// </summary>
    string ID { get; }
    
    /// <summary>
    /// Sub ID (a dll possibly creates several items, this subid identifies them)
    /// </summary>
    string SubID { get; }
    
    /// <summary>
    /// Title of the item (appears in the main page, under the icon)
    /// </summary>
    string Title { get; }
    
    /// <summary>
    /// Description of the item (appears as a help text)
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// List of keywords, allowing an easy search of the item
    /// Plural form is better
    /// </summary>
    ICollection<string> Keywords { get; }
    
    /// <summary>
    /// Full list of keywords associated with their score, based on the title,
    /// the description, the category, the subcategory and the additional keywords
    /// Set by GenericItem
    /// </summary>
    IDictionary<string, int> FullKeywords { get; }
    
    /// <summary>
    /// Image displayed as an icon
    /// </summary>
    Image Image { get; }
    
    /// <summary>
    /// Default category
    /// </summary>
    string Category { get; }
    
    /// <summary>
    /// Default subcategory
    /// </summary>
    string Subcategory { get; }
    
    /// <summary>
    /// Flags that characterize the item (extended)
    /// </summary>
    LemSettingsGlobal.ItemFlag Flags { get; set; }
    
    /// <summary>
    /// Context of the application
    /// Set by the dll loader
    /// </summary>
    ItemContext Context { get; set; }
    
    /// <summary>
    /// Path of the dll having generated the item
    /// Set by the dll loader
    /// </summary>
    string DllPath { get; set; }
    
    /// <summary>
    /// Path of the ini file having configured the item
    /// Set by the dll loader
    /// </summary>
    string IniPath { get; set; }
    
    /// <summary>
    /// List of classes being read / written by the item
    /// </summary>
    IDictionary<Type, LemSettingsGlobal.InteractionType> Types { get; }
    
    /// <summary>
    /// Score (in % based on an empirical formula)
    /// on how much the item matches with a series of keyword
    /// Set by ItemManager when we search for items
    /// </summary>
    double Score { get; set; }
    
    /// <summary>
    /// Last use of the item
    /// Set by MainWindow
    /// </summary>
    DateTime LastUsed { get; set; }
    
    /// <summary>
    /// Preparation of the item
    /// </summary>
    void Initialize();
  }
}
