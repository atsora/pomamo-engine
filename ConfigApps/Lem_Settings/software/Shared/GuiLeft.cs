// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of GuiLeft
  /// </summary>
  public partial class GuiLeft : UserControl
  {
    #region Members
    // For each item, view mode or not
    readonly IDictionary<IItem, bool> m_baseItems = new Dictionary<IItem, bool>();
    IItem m_currentItem;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Editors are enabled in the list?
    /// (default: true)
    /// </summary>
    public bool EditorsEnabled { get; set; }
    #endregion
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public GuiLeft()
    {
      EditorsEnabled = true;
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the current item
    /// </summary>
    /// <param name="item">current item</param>
    public void SetCurrentItem(IItem item)
    {
      m_currentItem = item;
      
      // Items always shown
      m_baseItems.Clear();
      IList<IItem> items;
      if (IsView()) {
        items = ItemManager.GetRelatedItems(item);
        foreach (IItem itemTmp in items) {
          m_baseItems[itemTmp] = false;
        }
      } else {
        items = ItemManager.GetRelatedReadOnlyItems(item);
        foreach (IItem itemTmp in items) {
          m_baseItems[itemTmp] = true;
        }
      }
      
      if (!EditorsEnabled) {
        // Immediately displayed
        treeItems.SetItems(m_baseItems);
      }
    }
    
    /// <summary>
    /// Set the current item data
    /// </summary>
    /// <param name="itemData"></param>
    public void SetCurrentItemData(ItemData itemData)
    {
      treeItems.CurrentItemData = itemData;
    }
    
    /// <summary>
    /// The wizard page changed, editors can be added in the tree
    /// </summary>
    /// <param name="page"></param>
    public void SetCurrentItemPage(IItemPage page)
    {
      if (EditorsEnabled && !IsView()) {
        // Additional items
        IDictionary<IItem, bool> relatedItems = new Dictionary<IItem, bool>(m_baseItems);
        IList<IItem> items = ItemManager.GetRelatedWriterItems(m_currentItem, page);
        foreach (IItem itemTmp in items) {
          relatedItems[itemTmp] = false;
        }

        treeItems.SetItems(relatedItems);
      }
    }
    
    bool IsView()
    {
      return m_currentItem is IView ||
        (m_currentItem is IConfigurator && m_currentItem.Context.ViewMode &&
         (m_currentItem.Flags & LemSettingsGlobal.ItemFlag.VIEW_MODE_ALLOWED) != 0);
    }
    #endregion // Methods
  }
}
