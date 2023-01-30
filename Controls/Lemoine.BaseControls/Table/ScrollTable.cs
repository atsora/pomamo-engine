// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of ScrollTable.
  /// </summary>
  public partial class ScrollTable : UserControl
  {
    #region Events
    /// <summary>
    /// Event emitted when a cell changed.
    /// The first argument is the row, the second one is the column.
    /// </summary>
    [Description("Fires when a cell value is changed (first arg is row, second is column).")]
    public event Action<int, int> CellChanged;
    
    /// <summary>
    /// Event emitted when an action in a menu from the vertical header is triggered.
    /// The first argument is the row, the second one is the action number.
    /// </summary>
    [Description("Fires when an action in a menu from the vertical header is triggered (first arg is row, second is action).")]
    public event Action<int, int> VerticalMenuClicked;
    
    /// <summary>
    /// Event emitted when an action in a menu from the horizontal header is triggered.
    /// The first argument is the column, the second one is the action number.
    /// </summary>
    [Description("Fires when an action in a menu from the horizontal header is triggered (first arg is column, second is action).")]
    public event Action<int, int> HorizontalMenuClicked;
    
    /// <summary>
    /// Event emitted when the menu from the vertical header is open
    /// The first argument is the row
    /// </summary>
    [Description("Fires when the menu from the vertical header is open (first arg is row).")]
    public event Action<int> VerticalMenuOpen;
    
    /// <summary>
    /// Event emitted when the menu from the horizontal header is open
    /// The first argument is the column
    /// </summary>
    [Description("Fires when the menu from the horizontal header is open (first arg is column).")]
    public event Action<int> HorizontalMenuOpen;
    
    /// <summary>
    /// Event emitted when the menu from the vertical header is closed
    /// The first argument is the row
    /// </summary>
    [Description("Fires when the menu from the vertical header is closed (first arg is row).")]
    public event Action<int> VerticalMenuClosed;
    
    /// <summary>
    /// Event emitted when the menu from the horizontal header is closed
    /// The first argument is the column
    /// </summary>
    [Description("Fires when the menu from the horizontal header is closed (first arg is column).")]
    public event Action<int> HorizontalMenuClosed;
    #endregion // Events
    
    #region Members
    const int COLUMN_WIDTH = 100;
    const int ROW_HEIGHT = 18;
    Pen LIGHT_PEN = new Pen(SystemColors.ControlLight);
    Pen DARK_PEN = new Pen(SystemColors.ControlDarkDark);
    bool m_updateLocationsAllowed = true;
    IList<string> m_verticalMenuItems;
    IList<string> m_horizontalMenuItems;
    ContextMenuStrip m_verticalMenu = null;
    ContextMenuStrip m_horizontalMenu = null;
    
    private const int BUTTON_WIDTH = 19;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Set the height of the horizontal header
    /// </summary>
    [DefaultValue(20)]
    [Description("Set the height of the horizontal header.")]
    public int HorizontalHeaderHeight {
      set {
        hHeader.RowStyles[0].Height = value;
        tableBase.RowStyles[0].Height = value;
      }
      get { return (int)hHeader.RowStyles[0].Height; }
    }
    
    /// <summary>
    /// Set the width of the first vertical header
    /// </summary>
    [DefaultValue(70)]
    [Description("Set the width of the first vertical header.")]
    public int FirstVerticalHeaderWidth {
      set {
        vHeader.ColumnStyles[0].Width = value;
        tableBase.ColumnStyles[0].Width = value + vHeader.ColumnStyles[1].Width +
          vHeader.ColumnStyles[2].Width;
      }
      get { return (int)vHeader.ColumnStyles[0].Width; }
    }
    
    /// <summary>
    /// Set the width of the second vertical header
    /// </summary>
    [DefaultValue(80)]
    [Description("Set the width of the second vertical header.")]
    public int SecondVerticalHeaderWidth {
      set {
        vHeader.ColumnStyles[1].Width = value;
        tableBase.ColumnStyles[0].Width = vHeader.ColumnStyles[0].Width + value +
          vHeader.ColumnStyles[2].Width;
      }
      get { return (int)vHeader.ColumnStyles[1].Width; }
    }
    
    /// <summary>
    /// Show or hide the horizontal footer
    /// </summary>
    [DefaultValue(true)]
    [Description("Show or hide the horizontal footer.")]
    public bool HorizontalFooterVisible {
      get { return tableBase.RowStyles[2].Height != 0; }
      set {
        if (value) {
          tableBase.RowStyles[2].Height = 20;
        }
        else {
          tableBase.RowStyles[2].Height = 0;
        }
      }
    }
    
    /// <summary>
    /// Show or hide the horizontal scrollbar
    /// </summary>
    [DefaultValue(true)]
    [Description("Show or hide the horizontal scrollbar.")]
    public bool HorizontalScrollbarVisible {
      get { return tableBase.RowStyles[3].Height != 0; }
      set {
        if (value) {
          tableBase.RowStyles[3].Height = 14;
          hScroll.Height = 14;
        }
        else {
          tableBase.RowStyles[3].Height = 0;
          hScroll.Height = 0;
        }
      }
    }
    
    /// <summary>
    /// Fill the table with checkboxes
    /// It has to be defined before using the table
    /// </summary>
    [DefaultValue(false)]
    [Description("Fill the table with checkboxes.")]
    public bool CheckBoxMode { get; set; }
    
    /// <summary>
    /// Number of columns
    /// </summary>
    public int ColumnCount { get { return table.ColumnCount - 1; } }
    
    /// <summary>
    /// Number of rows
    /// </summary>
    public int RowCount { get { return table.RowCount - 1; } }
    
    /// <summary>
    /// Menu displayed for each row
    /// If the list is null or empty, the menu is hidden
    /// </summary>
    [Description("Menu displayed for each row.")]
    public IList<string> VerticalMenu {
      get {
        return m_verticalMenuItems;
      }
      set {
        m_verticalMenuItems = value;
        if (value == null || value.Count == 0) {
          m_verticalMenu = null;
          vHeader.ColumnStyles[2].Width = 0;
        }
        else {
          m_verticalMenu = new ContextMenuStrip();
          m_verticalMenu.Closed += OnVerticalMenuClosed;
          int i = 0;
          foreach (string text in value) {
            ToolStripItem item = m_verticalMenu.Items.Add(text);
            item.Tag = i++;
            item.Click += OnVerticalMenuItemClicked;
          }
          vHeader.ColumnStyles[2].Width = BUTTON_WIDTH;
        }
        
        tableBase.ColumnStyles[0].Width = vHeader.ColumnStyles[0].Width +
          vHeader.ColumnStyles[1].Width + vHeader.ColumnStyles[2].Width;
      }
    }
    
    /// <summary>
    /// Menu displayed for each column
    /// If the list is null or empty, the menu is hidden
    /// </summary>
    [Description("Menu displayed for each column.")]
    public IList<string> HorizontalMenu {
      get {
        return m_horizontalMenuItems;
      }
      set {
        m_horizontalMenuItems = value;
        if (value == null || value.Count == 0) {
          m_horizontalMenu = null;
          vHeader.ColumnStyles[2].Width = 0;
          int nbColumn = table.ColumnCount;
          for (int i = 0; i < nbColumn; i++) {
            hHeader.ColumnStyles[2 * i].Width = 0;
          }
        }
        else {
          m_horizontalMenu = new ContextMenuStrip();
          m_horizontalMenu.Closed += OnHorizontalMenuClosed;
          int i = 0;
          foreach (string text in value) {
            ToolStripItem item = m_horizontalMenu.Items.Add(text);
            item.Tag = i++;
            item.Click += OnHorizontalMenuItemClicked;
          }
          int nbColumn = table.ColumnCount;
          for (i = 0; i < nbColumn; i++) {
            hHeader.ColumnStyles[2 * i].Width = BUTTON_WIDTH;
          }
        }
        
        tableBase.ColumnStyles[0].Width = vHeader.ColumnStyles[0].Width +
          vHeader.ColumnStyles[1].Width + vHeader.ColumnStyles[2].Width;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ScrollTable()
    {
      // Default values
      CheckBoxMode = false;
      
      InitializeComponent();
      Lemoine.BaseControls.Utils.SetDoubleBuffered(table);
      Lemoine.BaseControls.Utils.SetDoubleBuffered(hHeader);
      Lemoine.BaseControls.Utils.SetDoubleBuffered(vHeader);
      Lemoine.BaseControls.Utils.SetDoubleBuffered(hFooter);
      VerticalMenu = new List<string>();
      
      // Border style
      tableBase.CellPaint += PaintMainBorder;
      table.CellPaint += PaintInnerBorder;
      hHeader.CellPaint += PaintHHeaderBorder;
      vHeader.CellPaint += PaintVHeaderBorder;
      hFooter.CellPaint += PaintHFooterBorder;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Initialize the table with a double vertical header
    /// </summary>
    /// <param name="verticalHeaders">labels corresponding to the double vertical header</param>
    /// <param name="horizontalHeaders">labels corresponding to the horizontal header</param>
    public void InitTable(IDictionary<string, IList<string>> verticalHeaders, IList<string> horizontalHeaders)
    {
      InitHHeader(horizontalHeaders);
      InitHFooter(horizontalHeaders.Count);
      InitVHeader(verticalHeaders);
      
      int rowCount = 0;
      foreach (string key in verticalHeaders.Keys) {
        if (verticalHeaders[key] != null) {
          rowCount += verticalHeaders[key].Count;
        }
      }
      
      InitCenter(rowCount, horizontalHeaders.Count);
    }
    
    /// <summary>
    /// Initialize the table with a single vertical header
    /// </summary>
    /// <param name="verticalHeaders">labels corresponding to the double vertical header</param>
    /// <param name="horizontalHeaders">labels corresponding to the horizontal header</param>
    public void InitTable(IList<string> verticalHeaders, IList<string> horizontalHeaders)
    {
      InitHHeader(horizontalHeaders);
      InitHFooter(horizontalHeaders.Count);
      InitVHeader(verticalHeaders);
      InitCenter(verticalHeaders.Count, horizontalHeaders.Count);
    }
    
    /// <summary>
    /// Store a value in the table
    /// If CheckBoxMode is enabled, the corresponding checkbox is checked if value > 0
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="value"></param>
    public void SetValue(int row, int column, int value)
    {
      if (column >= ColumnCount || row >= RowCount) {
        return;
      }

      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      Control control = cell.Control;
      
      if (CheckBoxMode) {
        (control as CheckBox).Checked = (value > 0);
      } else {
        NumericUpDown numeric = control as NumericUpDown;
        numeric.Value = value;
        numeric.Text = value.ToString();
      }
    }
    
    /// <summary>
    /// Reset a value, resulting in an empty cell
    /// If CheckBoxMode is enabled, the corresponding checkbox is unchecked
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void ResetValue(int row, int column)
    {
      if (column >= ColumnCount || row >= RowCount) {
        return;
      }

      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      Control control = cell.Control;
      
      if (CheckBoxMode) {
        (control as CheckBox).Checked = false;
      } else {
        NumericUpDown numeric = control as NumericUpDown;
        numeric.Value = 0;
        numeric.Text = "";
      }
    }
    
    /// <summary>
    /// Retrieve a value from the table
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public int GetValue(int row, int column)
    {
      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      Control control = cell.Control;
      
      int iRet = 0;
      if (CheckBoxMode) {
        if ((control as CheckBox).Checked) {
          iRet = 1;
        }
      } else {
        iRet = (int)(control as NumericUpDown).Value;
      }
      
      return iRet;
    }
    
    /// <summary>
    /// Return true if the value is defined (if the cell is not empty)
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public bool IsValueDefined(int row, int column)
    {
      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      Control control = cell.Control;
      
      bool bRet = false;
      if (CheckBoxMode) {
        bRet = true;
      }
      else {
        bRet = ((control as NumericUpDown).Text != "");
      }

      return bRet;
    }
    
    /// <summary>
    /// Set the text of a cell in the footer
    /// </summary>
    /// <param name="column"></param>
    /// <param name="text"></param>
    public void SetFooterText(int column, string text)
    {
      ((Label)hFooter.GetControlFromPosition(column, 0)).Text = text;
    }
    
    /// <summary>
    /// Add a tooltip associated with an image
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="image">size max of the image: 15 * 15 px</param>
    /// <param name="text"></param>
    public void SetTooltip(int row, int column, Image image, string text)
    {
      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      cell.SetTooltip(image, text);
    }
    
    /// <summary>
    /// Remove a tooltip
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    public void RemoveToolTip(int row, int column)
    {
      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      cell.RemoveTooltip();
    }
    
    /// <summary>
    /// Return true if a tooltip is set in a specific cell
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public bool HasToolTip(int row, int column)
    {
      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      return cell.HasToolTip;
    }
    
    /// <summary>
    /// Return the content of the tooltip for a specific cell
    /// or an empty string if the tooltip is not set
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    public string ToolTipText(int row, int column)
    {
      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      return cell.ToolTipText;
    }
    
    /// <summary>
    /// Set the font to bold or not for a specific cell
    /// </summary>
    /// <param name="row"></param>
    /// <param name="column"></param>
    /// <param name="isBold"></param>
    public void SetBold(int row, int column, bool isBold)
    {
      ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
      if (isBold) {
        cell.Font = new Font(cell.Font, FontStyle.Bold);
      }
      else {
        cell.Font = new Font(cell.Font, FontStyle.Regular);
      }
    }
    
    /// <summary>
    /// Set the color of a row
    /// </summary>
    /// <param name="row"></param>
    /// <param name="color"></param>
    public void SetRowColor(int row, Color color)
    {
      if (row >= RowCount) {
        return;
      }

      for (int column = 0; column < ColumnCount; column++) {
        ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
        Control control = cell.Control;
        control.BackColor = color;
      }
    }
    
    /// <summary>
    /// Reset the color of a row
    /// </summary>
    /// <param name="row"></param>
    public void ResetRowColor(int row)
    {
      if (row >= RowCount) {
        return;
      }

      for (int column = 0; column < ColumnCount; column++) {
        ScrollTableCell cell = table.GetControlFromPosition(column, row) as ScrollTableCell;
        Control control = cell.Control;
        control.BackColor = SystemColors.Window;
      }
    }
    
    void InitHHeader(IList<string> horizontalHeaders)
    {
      int columnCount = horizontalHeaders.Count;
      
      // Initialize
      hHeader.RowStyles.Clear();
      hHeader.ColumnStyles.Clear();
      Utils.ClearControls(hHeader.Controls);
      hHeader.RowCount = 1;
      hHeader.ColumnCount = 2 * columnCount + 1;
      
      // Fill
      for (int column = 0; column < columnCount; column++) {
        // Label
        hHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, COLUMN_WIDTH));
        Label label = new Label();
        label.Height = ROW_HEIGHT;
        label.Dock = DockStyle.Fill;
        label.Text = horizontalHeaders[column];
        label.TextAlign = CheckBoxMode ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
        label.Margin = new Padding(3, 1, 3, 1);
        hHeader.Controls.Add(label, 2 * column, 0);
        
        // Button
        if (HorizontalMenu != null && HorizontalMenu.Count != 0) {
          hHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, BUTTON_WIDTH));
          hHeader.Controls.Add(GetButton(column, true), 2 * column + 1, 0);
        } else {
          hHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0));
        }
      }
      
      hHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1));
      hHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
    }
    
    void InitVHeader(IDictionary<string, IList<string>> verticalHeaders)
    {
      int rowCount = 0;
      foreach (string key in verticalHeaders.Keys) {
        if (verticalHeaders[key] != null) {
          rowCount += verticalHeaders[key].Count;
        }
      }

      // Initialize
      vHeader.RowStyles.Clear();
      Utils.ClearControls(vHeader.Controls);
      vHeader.RowCount = rowCount + 1;
      
      // Fill
      int numRow = 0;
      foreach (string key in verticalHeaders.Keys)
      {
        // Subheaders
        Label label;
        IList<string> subHeaders = verticalHeaders[key];
        if (subHeaders != null) {
          foreach (string subHeader in subHeaders) {
            vHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, ROW_HEIGHT));
            
            // Text
            label = new Label();
            label.Dock = DockStyle.Fill;
            label.Text = subHeader;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Margin = new Padding(3, 2, 3, 2);
            vHeader.Controls.Add(label, 1, numRow);
            
            // Button
            if (VerticalMenu != null && VerticalMenu.Count != 0) {
              vHeader.Controls.Add(GetButton(numRow, false), 2, numRow);
            }

            numRow++;
          }
          
          if (subHeaders.Count > 0) {
            // Header
            label = new Label();
            label.Dock = DockStyle.Fill;
            label.Text = key;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Margin = new Padding(0, 2, 0, 2);
            vHeader.Controls.Add(label, 0, numRow - subHeaders.Count);
            vHeader.SetRowSpan(label, subHeaders.Count);
          }
        }
      }
      vHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
    }
    
    void InitVHeader(IList<string> verticalHeaders)
    {
      // Initialize
      vHeader.RowStyles.Clear();
      Utils.ClearControls(vHeader.Controls);
      vHeader.RowCount = verticalHeaders.Count + 1;
      
      // Fill
      int numRow = 0;
      Label label;
      foreach (string header in verticalHeaders) {
        vHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, ROW_HEIGHT));
        label = new Label();
        label.Dock = DockStyle.Fill;
        label.Text = header;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Margin = new Padding(3, 2, 3, 2);
        vHeader.Controls.Add(label, 0, numRow);
        
        // Button
        if (VerticalMenu != null && VerticalMenu.Count != 0) {
          vHeader.Controls.Add(GetButton(numRow, false), 2, numRow);
        }

        numRow++;
      }
      vHeader.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
    }
    
    Button GetButton(int numCell, bool isHorizontal)
    {
      Button button = new Button();
      
      button.MaximumSize = new Size(BUTTON_WIDTH - 1, 16);
      button.Dock = DockStyle.Fill;
      button.TextAlign = ContentAlignment.MiddleCenter;
      button.Text = "≡";
      button.Margin = new Padding(0, 1, 0, 0);
      button.Tag = numCell;
      if (isHorizontal) {
        button.Click += OnHorizontalMenuButtonClicked;
      }
      else {
        button.Click += OnVerticalMenuButtonClicked;
      }

      return button;
    }
    
    void InitCenter(int rowCount, int columnCount)
    {
      // Clear the table
      table.RowStyles.Clear();
      table.ColumnStyles.Clear();
      Utils.ClearControls(table.Controls);
      
      table.RowCount = rowCount + 1;
      table.ColumnCount = columnCount + 1;
      int buttonWidth = 0;
      if (HorizontalMenu != null && HorizontalMenu.Count > 0) {
        buttonWidth = BUTTON_WIDTH;
      }

      for (int column = 0; column < columnCount; column++) {
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, COLUMN_WIDTH + buttonWidth));
      }

      table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1));
      for (int row = 0; row < rowCount; row++) {
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, ROW_HEIGHT));
      }

      table.RowStyles.Add(new RowStyle(SizeType.Absolute, 1));
      for (int column = 0; column < columnCount; column++) {
        for (int row = 0; row < rowCount; row++) {
          ScrollTableCell cell = new ScrollTableCell(row, column, CheckBoxMode);
          cell.ControlChanged += OnControlChanged;
          table.Controls.Add(cell, column, row);
        }
      }
      
      hScroll.Maximum = table.Width;
      vScroll.Maximum = table.Height;
    }
    
    void InitHFooter(int columnCount)
    {
      // Initialize
      hFooter.RowStyles.Clear();
      hFooter.ColumnStyles.Clear();
      Utils.ClearControls(hFooter.Controls);
      hFooter.RowCount = 1;
      hFooter.ColumnCount = columnCount + 1;
      
      // Fill
      int buttonWidth = 0;
      if (HorizontalMenu != null && HorizontalMenu.Count > 0) {
        buttonWidth = BUTTON_WIDTH;
      }

      for (int column = 0; column < columnCount; column++) {
        hFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, COLUMN_WIDTH + buttonWidth));
        Label label = new Label();
        label.Height = ROW_HEIGHT;
        label.Dock = DockStyle.Fill;
        label.Text = "0";
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Margin = new Padding(3, 1, 3, 1);
        hFooter.Controls.Add(label, column, 0);
      }
      hFooter.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 1));
      hFooter.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
    }
    
    void UpdateLocations()
    {
      table.Location = new Point(-hScroll.Value, -vScroll.Value);
      hHeader.Location = new Point(-hScroll.Value, 0);
      vHeader.Location = new Point(0, -vScroll.Value);
      hFooter.Location = new Point(-hScroll.Value, hFooter.Location.Y);
    }
    #endregion // Methods
    
    #region Event reactions
    private void OnControlChanged(int row, int column)
    {
      if (CellChanged != null) {
        CellChanged (row, column);
      }
    }
    
    private void PaintHHeaderBorder(object sender, TableLayoutCellPaintEventArgs e)
    {
      if (e.Column > 0 && e.Column % 2 == 0) {
        e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Left, e.CellBounds.Bottom);
      }
    }
    
    private void PaintHFooterBorder(object sender, TableLayoutCellPaintEventArgs e)
    {
      if (e.Column > 0) {
        e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Left, e.CellBounds.Bottom);
      }
    }
    
    private void PaintVHeaderBorder(object sender, TableLayoutCellPaintEventArgs e)
    {
      // Top
      if (e.Row > 0 && vHeader.GetControlFromPosition(0, e.Row) !=
          vHeader.GetControlFromPosition(0, e.Row - 1)) {
        e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Right, e.CellBounds.Top);
      }
      
      // Left
      if (e.Column > 0 && (e.Column != 2 && (e.Column != 1 || SecondVerticalHeaderWidth > 0))) {
        e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Left, e.CellBounds.Bottom);
      }
    }
    
    private void PaintInnerBorder(object sender, TableLayoutCellPaintEventArgs e)
    {
      // Top
      if (e.Row > 0) {
        if (vHeader.GetControlFromPosition(0, e.Row) != vHeader.GetControlFromPosition(0, e.Row - 1)) {
          e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                              e.CellBounds.Right, e.CellBounds.Top);
        }
        else {
          e.Graphics.DrawLine(LIGHT_PEN, e.CellBounds.Left, e.CellBounds.Top,
                              e.CellBounds.Right, e.CellBounds.Top);
        }
      }
      
      // Left
      if (e.Column > 0) {
        e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Left, e.CellBounds.Bottom);
      }
    }
    
    private void PaintMainBorder(object sender, TableLayoutCellPaintEventArgs e)
    {
      // Top
      if (e.Row > 0) {
        e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Right, e.CellBounds.Top);
      }

      // Left
      if (e.Column > 0) {
        e.Graphics.DrawLine(DARK_PEN, e.CellBounds.Left, e.CellBounds.Top,
                            e.CellBounds.Left, e.CellBounds.Bottom);
      }
    }
    
    void PanelTableSizeChanged(object sender, EventArgs e)
    {
      bool updateLocation = false;
      m_updateLocationsAllowed = false;
      
      // Horizontal scroll
      hScroll.LargeChange = panelTable.Width;
      if (hScroll.Value > hScroll.Maximum - hScroll.LargeChange) {
        hScroll.Value = Math.Max(hScroll.Minimum, hScroll.Maximum - hScroll.LargeChange);
        updateLocation = true;
      }
      
      // Vertical scroll
      vScroll.LargeChange = panelTable.Height;
      if (vScroll.Value > vScroll.Maximum - vScroll.LargeChange) {
        vScroll.Value = Math.Max(vScroll.Minimum, vScroll.Maximum - vScroll.LargeChange);
        updateLocation = true;
      }
      
      m_updateLocationsAllowed = true;
      if (updateLocation) {
        UpdateLocations ();
      }
    }
    
    void HScrollScroll(object sender, ScrollEventArgs e)
    {
      if (m_updateLocationsAllowed) {
        UpdateLocations ();
      }
    }
    
    void VScrollScroll(object sender, ScrollEventArgs e)
    {
      if (m_updateLocationsAllowed) {
        UpdateLocations ();
      }
    }
    
    void OnVerticalMenuButtonClicked(object sender, EventArgs e)
    {
      Button button = (Button)sender;
      
      Point ptLowerLeft = new Point(0, button.Height);
      ptLowerLeft = button.PointToScreen(ptLowerLeft);
      m_verticalMenu.Tag = button.Tag;
      if (VerticalMenuOpen != null) {
        VerticalMenuOpen ((int)button.Tag);
      }

      m_verticalMenu.Show(ptLowerLeft);
    }
    
    void OnVerticalMenuItemClicked(object sender, EventArgs e)
    {
      ToolStripItem item = (ToolStripItem)sender;
      if (VerticalMenuClicked != null) {
        VerticalMenuClicked ((int)m_verticalMenu.Tag, (int)item.Tag);
      }
    }
    
    void OnHorizontalMenuButtonClicked(object sender, EventArgs e)
    {
      Button button = (Button)sender;
      
      Point ptLowerLeft = new Point(0, button.Height);
      ptLowerLeft = button.PointToScreen(ptLowerLeft);
      m_horizontalMenu.Tag = button.Tag;
      if (HorizontalMenuOpen != null) {
        HorizontalMenuOpen ((int)button.Tag);
      }

      m_horizontalMenu.Show(ptLowerLeft);
    }
    
    void OnHorizontalMenuItemClicked(object sender, EventArgs e)
    {
      ToolStripItem item = (ToolStripItem)sender;
      if (HorizontalMenuClicked != null) {
        HorizontalMenuClicked ((int)m_horizontalMenu.Tag, (int)item.Tag);
      }
    }
    
    void OnVerticalMenuClosed(object sender, EventArgs e)
    {
      if (VerticalMenuClosed != null) {
        VerticalMenuClosed ((int)m_verticalMenu.Tag);
      }
    }
    
    void OnHorizontalMenuClosed(object sender, EventArgs e)
    {
      if (HorizontalMenuClosed != null) {
        HorizontalMenuClosed ((int)m_horizontalMenu.Tag);
      }
    }
    #endregion // Event reactions
  }
}
