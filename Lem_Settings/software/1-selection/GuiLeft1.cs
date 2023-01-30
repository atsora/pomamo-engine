// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using Lemoine.BaseControls;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of GuiLeft1.
  /// </summary>
  public partial class GuiLeft1 : UserControl
  {
    #region Members
    SelectionGuiCenter m_guiCenter1;
    GuiRight1 m_guiRight1;
    bool m_dontUpdate = false;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Return true if "Recent" is selected
    /// </summary>
    public bool IsRecent { get { return listBoxDisplay.SelectedIndex == 1; } }
    
    /// <summary>
    /// Return true if "Favorites" is selected
    /// </summary>
    public bool IsFavorite { get { return listBoxDisplay.SelectedIndex == 2; } }
    #endregion

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public GuiLeft1()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize references to access foreign objects
    /// </summary>
    /// <param name="guiCenter1"></param>
    /// <param name="guiRight1"></param>
    public void InitReferences(SelectionGuiCenter guiCenter1, GuiRight1 guiRight1)
    {
      m_guiCenter1 = guiCenter1;
      m_guiRight1 = guiRight1;
    }
    
    /// <summary>
    /// Update displayed categories
    /// </summary>
    public void UpdateCategories()
    {
      int indexListDisplay = listBoxDisplay.SelectedIndex;
      string currentCategory = listBoxCategories.SelectedIndex == -1 ? "" :
        (string)listBoxCategories.SelectedItem;
      
      // Fill the categories
      using (var suspend = new SuspendDrawing(listBoxCategories))
      {
        listBoxCategories.Items.Clear();
        IList<string> categories = ItemManager.GetCategories();
        foreach (string category in categories) {
          listBoxCategories.Items.Add(category);
        }
      }
      
      // Selection
      m_dontUpdate = true;
      if (indexListDisplay == -1) {
        // A category - or all - is selected
        if (String.IsNullOrEmpty(currentCategory) || !listBoxCategories.Items.Contains(currentCategory)) {
          listBoxDisplay.SelectedIndex = 0;
        }
        else {
          listBoxCategories.SelectedItem = currentCategory;
        }
      } else {
        listBoxDisplay.SelectedIndex = indexListDisplay;
      }

      m_dontUpdate = false;
      
      UpdateItems(true);
    }
    
    /// <summary>
    /// Focus on the search textbox
    /// </summary>
    public void FocusOnSearch()
    {
      textSearch.Focus();
      ControlAnimation.AnimateBackColor(textSearch, Color.LightGreen, SystemColors.Window, 0.4);
    }
    #endregion // Methods
    
    #region Event reactions
    void ListBoxCategoriesSelectedIndexChanged(object sender, EventArgs e)
    {
      if (m_dontUpdate) {
        return;
      }

      m_dontUpdate = true;
      listBoxDisplay.ClearSelected();
      m_dontUpdate = false;
      
      UpdateItems(true);
    }
    
    void ListBoxDisplaySelectedIndexChanged(object sender, EventArgs e)
    {
      if (m_dontUpdate) {
        return;
      }

      m_dontUpdate = true;
      listBoxCategories.ClearSelected();
      m_dontUpdate = false;
      
      UpdateItems(true);
    }
    
    void ButtonClearSearchClick(object sender, EventArgs e)
    {
      textSearch.Clear();
    }
    
    void TextSearchTextChanged(object sender, EventArgs e)
    {
      UpdateItems(false);
    }
    
    void UpdateItems(bool focusOnCenter)
    {
      // List of words
      ICollection<string> words = KeywordSearch.FormatKeywords(KeywordSearch.SplitIntoWords(textSearch.Text));
      
      // List of items
      IList<IItem> items;
      switch (listBoxDisplay.SelectedIndex) {
        case 0: case 1:
          // Display all or recent
          items = ItemManager.GetItems("", false, words);
          break;
        case 2:
          // Display favorites
          items = ItemManager.GetItems("", true, words);
          break;
        default:
          // Display a category
          items = ItemManager.GetItems((string)listBoxCategories.SelectedItem, false, words);
          break;
      }
      
      // Display the items
      using (var suspend = new SuspendDrawing(m_guiCenter1))
      {
        switch (listBoxDisplay.SelectedIndex) {
          case 1:
            // Recent items: always by date
            if (items.Count > 0) {
            m_guiCenter1.DisplayItems(items, SelectionGuiCenter.DisplayMode.LIST_BY_DATE);
          }
          else {
              if (words.Count > 0) {
              m_guiCenter1.DisplayMessage("no results");
            }
            else {
              m_guiCenter1.DisplayMessage("no recent items");
            }
          }
            break;
          case 2:
            // Favorite items: simple display or by score
            if (words != null && words.Count > 0) {
              if (items.Count > 0) {
              m_guiCenter1.DisplayItems(items, SelectionGuiCenter.DisplayMode.LIST_BY_SCORE);
            }
            else {
              m_guiCenter1.DisplayMessage("no results");
            }
          } else {
              if (items.Count > 0) {
              m_guiCenter1.DisplayItems(items, SelectionGuiCenter.DisplayMode.TILES_SIMPLE);
            }
            else {
              m_guiCenter1.DisplayMessage("no favorite items");
            }
          }
            break;
          default:
            // Display all or a category: display by category or by score
            if (words != null && words.Count > 0) {
              if (items.Count > 0) {
              m_guiCenter1.DisplayItems(items, SelectionGuiCenter.DisplayMode.LIST_BY_SCORE);
            }
            else {
              m_guiCenter1.DisplayMessage("no results");
            }
          } else {
              if (items.Count > 0) {
              m_guiCenter1.DisplayItems(items, SelectionGuiCenter.DisplayMode.TILES_BY_CATEGORY);
            }
            else {
              m_guiCenter1.DisplayMessage("no items");
            }
          }
            break;
        }
      }
      m_guiRight1.SetCurrentItem(null);
      
      if (focusOnCenter) {
        m_guiCenter1.Focus();
      }
    }
    #endregion // Event reactions
  }
}
