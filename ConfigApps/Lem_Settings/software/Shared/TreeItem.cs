// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Settings;

namespace Lem_Settings
{
  /// <summary>
  /// Description of TreeItems.
  /// </summary>
  public partial class TreeItem : UserControl
  {
    #region Members
    IDictionary<IItem, bool> m_viewModes = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Current item data, used to initialize an item triggered in the tree
    /// </summary>
    public ItemData CurrentItemData { get; set; }
    #endregion // Getters / Setters
    
    #region Events
    /// <summary>
    /// Event emitted when an item has to be called
    /// First argument is the item to launch
    /// Second argument is true if the item has to be displayed as a view
    /// Third argument is the current data
    /// Fourth argument is for the next item
    /// </summary>
    public static event Action<IItem, bool, ItemData, bool> ItemCalledEvent;
    #endregion // Events
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public TreeItem()
    {
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Fill items in the tree
    /// </summary>
    /// <param name="relatedItems"></param>
    public void SetItems(IDictionary<IItem, bool> relatedItems)
    {
      m_viewModes = relatedItems;
      ICollection<IItem> items = relatedItems.Keys;
      
      // Sort items by category and subcategory
      var sortedItems = new Dictionary<string, Dictionary<string, IList<IItem>>>();
      foreach (IItem item in items) {
        string category = item.Category;
        if (string.IsNullOrEmpty(category)) {
          category = LemSettingsGlobal.DEFAULT_CATEGORY;
        }

        string subcategory = item.Subcategory;
        if (string.IsNullOrEmpty(subcategory)) {
          subcategory = LemSettingsGlobal.DEFAULT_SUBCATEGORY;
        }

        // Create category if necessary
        if (!sortedItems.ContainsKey(category)) {
          sortedItems[category] = new Dictionary<string, IList<IItem>>();
        }
        
        // Create subcategory if necessary
        if (!sortedItems[category].ContainsKey(subcategory)) {
          sortedItems[category][subcategory] = new List<IItem>();
        }
        
        // Add item
        if (!sortedItems[category][subcategory].Contains(item)) {
          sortedItems[category][subcategory].Add(item);
        }
      }
      
      // Sort categories
      var sortedCategories = new List<string>();
      foreach (string category in sortedItems.Keys) {
        sortedCategories.Add(category);
      }

      sortedCategories.Sort();
      
      if (relatedItems.Count == 0) {
        verticalScrollLayout.Hide();
      }
      else {
        using (new SuspendDrawing(verticalScrollLayout))
        {
          verticalScrollLayout.Clear();
          
          verticalScrollLayout.Show();
          int row = 0;
          foreach (string category in sortedCategories)
          {
            // Category
            verticalScrollLayout.AddControl(GetCategoryControl(category), 0, row++);
            
            // Sort subcategories
            var sortedSubCategories = new List<string>();
            foreach (string subCategory in sortedItems[category].Keys) {
              sortedSubCategories.Add(subCategory);
            }

            sortedSubCategories.Sort();
            foreach (string subcategory in sortedSubCategories)
            {
              // Subcategory
              verticalScrollLayout.AddControl(GetSubCategoryControl(subcategory), 0, row++);
              
              // Items
              foreach (IItem item in sortedItems[category][subcategory]) {
                var cell = new ClickableCell(this);
                cell.DisplayMode = ClickableCell.Mode.SingleTextRight;
                cell.Height = 34;
                cell.ImageMargin = new Padding(2, 1, 2, 1);
                cell.ImageSize = new Size(26, 26);
                cell.Tag = item;
                item.Context.ViewMode = m_viewModes[item];
                cell.Text = item.Title;
                cell.Image = GetImage(item);
                cell.Tooltip = item.Description;
                cell.HoverColor = LemSettingsGlobal.COLOR_ITEM_HOVER;
                cell.MouseDoubleClick += CellDoubleClicked;
                cell.Dock = DockStyle.Fill;
                cell.Margin = new Padding(3, 0, 3, 3);
                verticalScrollLayout.AddControl(cell, 0, row++);
              }
            }
          }
        }
        verticalScrollLayout.UpdateScroll();
      }
    }
    
    Image GetImage(IItem item)
    {
      Image image;
      
      if (item is IConfigurator && !m_viewModes[item]) {
        image = imageList.Images[0];
      }
      else if (item is ILink) {
        image = imageList.Images[1];
      }
      else if (item is ILauncher) {
        image = imageList.Images[2];
      }
      else if (item is IWizard) {
        image = imageList.Images[4];
      }
      else // View
{
        image = imageList.Images[3];
      }

      return image;
    }
    
    Control GetCategoryControl(string category)
    {
      var control = new Label();
      control.Font = new Font(control.Font.FontFamily, 12, FontStyle.Bold);
      control.Text = category;
      control.Height = 22;
      control.BackColor = LemSettingsGlobal.COLOR_CATEGORY;
      control.ForeColor = Color.White;
      control.TextAlign = ContentAlignment.MiddleLeft;
      control.Margin = new Padding(0, 0, 0, 4);
      control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      return control;
    }
    
    Control GetSubCategoryControl(string subcategory)
    {
      var control = new Label();
      control.Font = new Font(control.Font.FontFamily, 9, FontStyle.Bold);
      control.Text = subcategory;
      control.Height = 18;
      control.ForeColor = LemSettingsGlobal.COLOR_SUBCATEGORY;
      control.TextAlign = ContentAlignment.MiddleLeft;
      control.Margin = new Padding(0, 0, 0, 3);
      control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
      return control;
    }
    #endregion // Methods
    
    #region Event reactions
    void CellDoubleClicked(object sender, EventArgs e)
    {
      var selectedItem = (sender as ClickableCell).Tag as IItem;
      ItemCalledEvent(selectedItem, m_viewModes[selectedItem], CurrentItemData, false);
    }
    #endregion // Event reactions
  }
}
