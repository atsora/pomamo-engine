// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Settings;
using Lemoine.Core.Log;

namespace Lem_Settings
{
  /// <summary>
  /// Description of GuiCenter1.
  /// </summary>
  public partial class SelectionGuiCenter : UserControl
  {
    /// <summary>
    /// Different display for the items
    /// </summary>
    public enum DisplayMode
    {
      /// <summary>
      /// Icons with text below, in a simple alphabetical order
      /// </summary>
      TILES_SIMPLE,

      /// <summary>
      /// Icons with text below, classified by category and subcategory
      /// </summary>
      TILES_BY_CATEGORY,

      /// <summary>
      /// Icons with text to the right, classified by last access date (closest first)
      /// </summary>
      LIST_BY_DATE,

      /// <summary>
      /// Icons with text to the right, classified by scored (best first)
      /// </summary>
      LIST_BY_SCORE
    }

    GuiLeft1 m_guiLeft1;
    IItem m_currentItem = null;
    readonly IList<ClickableCell> m_cells = new List<ClickableCell> ();

    static readonly ILog log = LogManager.GetLogger (typeof (SelectionGuiCenter).FullName);

    /// <summary>
    /// Event emitted when an item is double clicked
    /// First argument is the item to launch
    /// Second argument is true if the item has to be displayed as a view
    /// Third argument is the current data
    /// Fourth argument is for the next item
    /// </summary>
    public event Action<IItem, bool, ItemData, bool> ItemLaunched;

    /// <summary>
    /// Emitted when an item is clicked
    /// First argument is the item
    /// </summary>
    public event Action<IItem> ItemClicked;

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public SelectionGuiCenter ()
    {
      InitializeComponent ();
    }

    #region Methods
    /// <summary>
    /// Initialize references to access foreign objects
    /// </summary>
    /// <param name="guiLeft1"></param>
    public void InitReferences (GuiLeft1 guiLeft1)
    {
      m_guiLeft1 = guiLeft1;
    }

    /// <summary>
    /// Fill the list with items
    /// Several display modes are possible
    /// </summary>
    /// <param name="items"></param>
    /// <param name="displayMode"></param>
    public void DisplayItems (IList<IItem> items, DisplayMode displayMode)
    {
      verticalScrollLayout.Show ();
      buttonOk.Enabled = false;

      // Clear controls
      verticalScrollLayout.Clear ();
      m_cells.Clear ();

      // Display controls
      switch (displayMode) {
      case DisplayMode.TILES_SIMPLE:
        DisplayTilesSimple (items);
        break;
      case DisplayMode.TILES_BY_CATEGORY:
        DisplayTilesByCategory (items);
        break;
      case DisplayMode.LIST_BY_DATE:
        DisplayListByDate (items);
        break;
      case DisplayMode.LIST_BY_SCORE:
        DisplayListByScore (items);
        break;
      }

      // Update scroll
      verticalScrollLayout.UpdateScroll ();
    }

    void DisplayTilesSimple (IList<IItem> items)
    {
      var sortedItems = new List<IItem> (items);
      sortedItems.Sort (); // Alphabetical order

      FlowLayoutPanel flow = GetFlowLayoutItems (items);
      verticalScrollLayout.AddControl (flow);
      flow.Margin = new Padding (3, 3, 0, 0);
    }

    void DisplayTilesByCategory (IList<IItem> items)
    {
      // Sort items by categories and subcategories
      var currentItems = new Dictionary<string, Dictionary<string, List<IItem>>> ();
      foreach (IItem item in items) {
        string category = item.Category;
        if (category == "") {
          category = LemSettingsGlobal.DEFAULT_CATEGORY;
        }

        string subcategory = item.Subcategory;
        if (subcategory == "") {
          subcategory = LemSettingsGlobal.DEFAULT_SUBCATEGORY;
        }

        // Add category if not present
        if (!currentItems.ContainsKey (category)) {
          currentItems[category] = new Dictionary<string, List<IItem>> ();
        }

        // Add subcategory if not present
        if (!currentItems[category].ContainsKey (subcategory)) {
          currentItems[category][subcategory] = new List<IItem> ();
        }

        // Add item
        currentItems[category][subcategory].Add (item);
      }

      // Sort categories
      var sortedCategories = new List<string> ();
      foreach (string category in currentItems.Keys) {
        sortedCategories.Add (category);
      }

      sortedCategories.Sort ();

      // Display controls
      FlowLayoutPanel flow = null;
      foreach (string category in sortedCategories) {
        // Category
        verticalScrollLayout.AddControl (GetCategoryControl (category));

        // Sort subcategories
        var sortedSubcategories = new List<string> ();
        foreach (string subCategory in currentItems[category].Keys) {
          sortedSubcategories.Add (subCategory);
        }

        sortedSubcategories.Sort ();

        foreach (string subcategory in sortedSubcategories) {
          // Subcategory
          verticalScrollLayout.AddControl (GetSubcategoryControl (subcategory));

          // Items
          currentItems[category][subcategory].Sort ();
          flow = GetFlowLayoutItems (currentItems[category][subcategory]);
          verticalScrollLayout.AddControl (flow);
        }
      }

      // Last flow layout margin
      if (flow != null) {
        flow.Margin = new Padding (3, 3, 0, 0);
      }
    }

    void DisplayListByDate (IList<IItem> items)
    {
      // Sort items by date (closest first)
      var sortedItems = new Dictionary<DateTime, List<IItem>> ();
      foreach (var item in items) {
        if (!sortedItems.ContainsKey (item.LastUsed)) {
          sortedItems[item.LastUsed] = new List<IItem> ();
        }

        sortedItems[item.LastUsed].Add (item);
      }
      foreach (var value in sortedItems.Values) {
        value.Sort ();
      }

      // Sort dates
      var sortedDates = new List<DateTime> ();
      foreach (DateTime date in sortedItems.Keys) {
        sortedDates.Add (date);
      }

      sortedDates.Sort ((a, b) => -1 * a.CompareTo (b));

      // Display controls
      IList<IItem> sortedList = new List<IItem> ();
      IList<string> messages = new List<string> ();
      foreach (DateTime date in sortedDates) {
        foreach (IItem item in sortedItems[date]) {
          sortedList.Add (item);
          messages.Add (String.Format ("Last accessed: {0}",
                                     date.Year < 1980 ? "never" :
                                     date.ToLongDateString () + ", " + date.ToShortTimeString ()));
        }
      }
      DisplayList (sortedList, messages);
    }

    void DisplayListByScore (IList<IItem> items)
    {
      // Sort items by score (highest first)
      var sortedItems = new Dictionary<double, List<IItem>> ();
      foreach (var item in items) {
        if (!sortedItems.ContainsKey (item.Score)) {
          sortedItems[item.Score] = new List<IItem> ();
        }

        sortedItems[item.Score].Add (item);
      }
      foreach (var value in sortedItems.Values) {
        value.Sort ();
      }

      // Sort scores
      var sortedScores = new List<double> ();
      foreach (double score in sortedItems.Keys) {
        sortedScores.Add (score);
      }

      sortedScores.Sort ((a, b) => -1 * a.CompareTo (b));

      // Display controls
      IList<IItem> sortedList = new List<IItem> ();
      IList<string> messages = new List<string> ();
      foreach (double score in sortedScores) {
        foreach (IItem item in sortedItems[score]) {
          sortedList.Add (item);
          messages.Add (String.Format ("Score: {0} %", (int)Math.Round (score)));
        }
      }
      DisplayList (sortedList, messages);
    }

    void DisplayList (IList<IItem> sortedItems, IList<string> messages)
    {
      bool withMessage = (messages != null && sortedItems.Count == messages.Count);
      ClickableCell cell = null;
      for (int i = 0; i < sortedItems.Count; i++) {
        IItem item = sortedItems[i];
        cell = GetCell (item, withMessage ? ClickableCell.Mode.DoubleTextRight :
                       ClickableCell.Mode.SingleTextRight);
        cell.Margin = new Padding (3, 3, 3, 0);
        cell.Dock = DockStyle.Fill;
        cell.Size = new Size (0, 58);
        if (withMessage) {
          cell.SecondText = messages[i];
        }

        verticalScrollLayout.AddControl (cell);
      }

      // Last cell has an additional margin
      if (cell != null) {
        cell.Margin = new Padding (3, 3, 3, 3);
      }
    }

    /// <summary>
    /// Display a message instead of the items
    /// </summary>
    /// <param name="message"></param>
    public void DisplayMessage (string message)
    {
      labelMessage.Text = message;
      verticalScrollLayout.Hide ();
      buttonOk.Enabled = false;

      // Clear controls
      verticalScrollLayout.Clear ();
      m_cells.Clear ();
    }

    Control GetCategoryControl (string category)
    {
      var control = new Label ();
      control.Font = new Font (control.Font.FontFamily, 12, FontStyle.Bold);
      control.Text = category;
      control.Height = 22;
      control.BackColor = LemSettingsGlobal.COLOR_CATEGORY;
      control.ForeColor = Color.White;
      control.TextAlign = ContentAlignment.MiddleLeft;
      control.Margin = new Padding (0, 0, 0, 4);
      control.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
      return control;
    }

    Control GetSubcategoryControl (string subcategory)
    {
      var control = new Label ();
      control.Font = new Font (control.Font.FontFamily, 9, FontStyle.Bold);
      control.Text = subcategory;
      control.Height = 18;
      control.ForeColor = LemSettingsGlobal.COLOR_SUBCATEGORY;
      control.TextAlign = ContentAlignment.MiddleLeft;
      control.Margin = new Padding (0, 0, 0, 0);
      control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
      return control;
    }

    FlowLayoutPanel GetFlowLayoutItems (IList<IItem> items)
    {
      // Layout preparation
      var layout = new FlowLayoutPanel ();
      layout.AutoSize = true;
      layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
      layout.Margin = new Padding (3, 3, 0, 15);

      int heightOffset = 0;
      {
        Graphics graphics = this.CreateGraphics ();
        if (graphics.DpiY >= 144) {
          heightOffset = 20;
        }
        else if (graphics.DpiY >= 120) {
          heightOffset = 10;
        }
      }

      foreach (IItem item in items) {
        ClickableCell cell = GetCell (item, ClickableCell.Mode.SingleTextBelow);
        cell.Margin = new Padding (0, 0, 3, 3);
        cell.Size = new Size (106, 116 + heightOffset);
        layout.Controls.Add (cell);
      }

      return layout;
    }

    ClickableCell GetCell (IItem item, ClickableCell.Mode mode)
    {
      item.Context.ViewMode = false;
      var cell = new ClickableCell (this);
      cell.MouseClick += CellClicked;
      cell.MouseDoubleClick += CellDoubleClicked;
      cell.MenuClicked += MenuClicked;
      cell.DisplayMode = mode;
      cell.Tag = item;
      cell.Text = item.Title;
      cell.HoverColor = LemSettingsGlobal.COLOR_ITEM_HOVER;
      cell.ImageMargin = new Padding (3);
      if (mode == ClickableCell.Mode.SingleTextBelow) {
        cell.ImageSize = new Size (86, 86);
        PrepareImage (cell, false);
      }
      else {
        cell.ImageSize = new Size (57, 57);
        PrepareImage (cell, true);
      }
      PrepareMenu (cell);

      m_cells.Add (cell);
      return cell;
    }

    /// <summary>
    /// Remove a local item, currently not used
    /// </summary>
    /// <param name="itemToRemove"></param>
    void RemoveWizard (IItem itemToRemove)
    {
      IList<IItem> brothers = ItemManager.GetBrothers (itemToRemove);
      var names = new List<string> ();
      foreach (IItem item in brothers) {
        names.Add ("\"" + item.Title + "\"");
      }

      string plural = "";
      if (brothers.Count > 1) {
        plural = "s";
      }

      DialogResult confirmResult = MessageBox.Show ("Are you sure to delete the item" + plural + " " +
                                                   string.Join (", ", names.ToArray ()) + " ?",
                                                   "Confirmation",
                                                   MessageBoxButtons.YesNo,
                                                   MessageBoxIcon.Warning,
                                                   MessageBoxDefaultButton.Button2);
      if (confirmResult == DialogResult.Yes) {
        if (!ItemManager.RemoveItem (itemToRemove)) {
          MessageBoxCentered.Show (this.Parent, "Cannot remove the item.", "Warning",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
      }
    }

    void PrepareImage (ClickableCell cell, bool small)
    {
      var item = cell.Tag as IItem;
      int width = small ? 85 : 128;
      Image mainImage = ScaleImage (item.Image, width, width);

      if ((item.Flags & LemSettingsGlobal.ItemFlag.FAVORITE) != 0) {
        Image star = imageList.Images[5];
        using (Graphics g = Graphics.FromImage (mainImage)) {
          g.DrawImage (star, new Point (mainImage.Width - star.Width - 18, mainImage.Height - star.Height));
        }
      }

      Image imposeImage;
      if (item is IConfigurator) {
        imposeImage = imageList.Images[0];
      }
      else if (item is ILink) {
        imposeImage = imageList.Images[1];
      }
      else if (item is ILauncher) {
        imposeImage = imageList.Images[2];
      }
      else if (item is IView) {
        imposeImage = imageList.Images[3];
      }
      else if (item is IWizard) {
        imposeImage = imageList.Images[4];
      }
      else {
        imposeImage = new Bitmap (0, 0);
      }

      using (Graphics g = Graphics.FromImage (mainImage)) {
        g.DrawImage (imposeImage, new Point (mainImage.Width - imposeImage.Width, mainImage.Height - imposeImage.Height));
      }

      cell.Image = mainImage;
    }

    void PrepareMenu (ClickableCell cell)
    {
      var item = cell.Tag as IItem;

      IList<string> actions = new List<string> ();
      actions.Add ((item.Flags & LemSettingsGlobal.ItemFlag.FAVORITE) == 0 ?
                  "Mark as favorite" : "Remove from favorite");
      actions.Add ("Remove last access date");
      actions.Add ("Send to desktop");
      actions.Add ("Information");
      cell.MenuElements = actions;
    }

    static Image ScaleImage (Image image, int maxWidth, int maxHeight)
    {
      var ratioX = (double)maxWidth / image.Width;
      var ratioY = (double)maxHeight / image.Height;
      var ratio = Math.Min (ratioX, ratioY);

      var newWidth = (int)(image.Width * ratio);
      var newHeight = (int)(image.Height * ratio);

      var newImage = new Bitmap (maxWidth, maxHeight);
      Graphics.FromImage (newImage).DrawImage (image,
                                             (maxWidth - newWidth) / 2,
                                             (maxHeight - newHeight) / 2,
                                             newWidth, newHeight);
      return newImage;
    }
    #endregion // Methods

    #region Event reactions
    void CellClicked (object sender, EventArgs e)
    {
      m_currentItem = (sender as ClickableCell).Tag as IItem;
      if (ItemClicked != null) {
        ItemClicked (m_currentItem);
      }

      buttonOk.Enabled = true;
    }

    void CellDoubleClicked (object sender, EventArgs e)
    {
      ItemLaunched ((sender as ClickableCell).Tag as IItem, false, null, false);
    }

    void ButtonOkClick (object sender, EventArgs e)
    {
      if (m_currentItem != null) {
        ItemLaunched (m_currentItem, false, null, false);
      }
    }

    void MenuClicked (int numAction)
    {
      if (m_currentItem != null) {
        switch (numAction) {
        case 0:
          // Change favorite status
          m_currentItem.Flags ^= LemSettingsGlobal.ItemFlag.FAVORITE;
          if (m_guiLeft1.IsFavorite) {
            m_guiLeft1.UpdateCategories ();
          }
          else {
            foreach (var cell in m_cells) {
              if (cell.Tag == m_currentItem) {
                PrepareImage (cell, cell.ImageSize.Width < 80);
                PrepareMenu (cell);
                break;
              }
            }
          }
          break;
        case 1:
          // Remove last access date
          m_currentItem.LastUsed = new DateTime (1970, 1, 1);
          if (m_guiLeft1.IsRecent) {
            m_guiLeft1.UpdateCategories ();
          }

          break;
        case 2:
          // Address of the shortcut
          CreateShortcut ();
          break;
        case 3:
          // Information
          var dialog = new DialogItemInformation (m_currentItem);
          dialog.ShowDialog ();
          break;
        }
      }
    }
    #endregion // Event reactions

    void CreateShortcut ()
    {
      throw new NotImplementedException ("Create shortcut not implemented in .NET Core");
    }
  }
}
