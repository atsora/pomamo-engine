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
  /// Control made of a text and an image, whose behaviour shows to the client that it can be clicked.
  /// This control could typically be integrated within a list.
  /// The background color changes when hovered by the mouse.
  /// The border is shown when the user clicks on the cell. The border of all other cells sharing
  /// the same parent is removed.
  /// Note: the image can be associated with a brush to set a custom background
  ///       a border can be added to the image => this control can be used as a legend
  /// </summary>
  public partial class ClickableCell : UserControl
  {
    /// <summary>
    /// Display mode
    /// </summary>
    public enum Mode
    {
      /// <summary>
      /// One label is displayed below the image
      /// </summary>
      SingleTextBelow,
      
      /// <summary>
      /// One label is displayed to the right
      /// </summary>
      SingleTextRight,
      
      /// <summary>
      /// Two labels are displayed to the right
      /// </summary>
      DoubleTextRight
    }
    
    // Label allowing us to choose the color, even if it's disabled
    class SpecialLabel : Label
    {
      readonly ClickableCell m_parentCell;
      
      public SpecialLabel(ClickableCell parentCell)
      {
        m_parentCell = parentCell;
      }
      
      protected override void OnPaint(PaintEventArgs e)
      {
        TextFormatFlags flags = TextFormatFlags.TextBoxControl;
        if (m_parentCell.AutoLineBreak) {
          flags |= TextFormatFlags.WordBreak;
        }

        if (TextAlign == ContentAlignment.MiddleLeft) {
          flags |= TextFormatFlags.Left | TextFormatFlags.VerticalCenter;
        }
        else if (TextAlign == ContentAlignment.MiddleCenter) {
          flags |= TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
        }
        else if (TextAlign == ContentAlignment.TopLeft) {
          flags |= TextFormatFlags.Top | TextFormatFlags.Left;
        }
        else if (TextAlign == ContentAlignment.BottomLeft) {
          flags |= TextFormatFlags.Bottom | TextFormatFlags.Left;
        }

        TextRenderer.DrawText(e.Graphics, this.Text, this.Font,
                              base.ClientRectangle, ForeColor, flags);
      }
    }
    
    // PictureBox allowing us to change the background thanks to a brush
    class SpecialPictureBox : PictureBox
    {
      /// <summary>
      /// Brush used to draw the background of the image
      /// </summary>
      public Brush BackgroundBrush { get; set; }
      
      protected override void OnPaint(PaintEventArgs e)
      {
        if (BackgroundBrush != null) {
          e.Graphics.FillRectangle(BackgroundBrush, base.ClientRectangle);
        }

        base.OnPaint(e);
      }
    }
    
    #region Events
    /// <summary>
    /// Event emitted when an element has been clicked in the menu
    /// The argument is the number of the element
    /// </summary>
    public event Action<int> MenuClicked;
    #endregion // Events
    
    #region Members
    readonly SpecialLabel m_specialLabel = null;
    readonly SpecialLabel m_secondSpecialLabel = null;
    readonly SpecialPictureBox m_specialPictureBox = new SpecialPictureBox();
    ToolTip m_toolTip = null;
    Color m_previousBackgroundColor;
    object m_parent = null;
    static IDictionary<object, IList<ClickableCell>> s_mapCells = new Dictionary<object, IList<ClickableCell>>();
    Mode m_displayMode = Mode.SingleTextBelow;
    ContextMenuStrip m_menu = null;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Background color when the cell is hovered by the mouse
    /// </summary>
    [Description("Background color when the cell is hovered by the mouse.")]
    public Color HoverColor { get; set; }
    
    /// <summary>
    /// Text displayed as tooltip when the cell is hovered
    /// </summary>
    [DefaultValue("")]
    [Description("Text displayed as tooltip when the cell is hovered.")]
    public string Tooltip { get; set; }
    
    /// <summary>
    /// Text displayed by the cell next to the image
    /// </summary>
    [DefaultValue("")]
    [Description("Text displayed by the first label.")]
    public override string Text {
      get { return m_specialLabel.Text; }
      set { m_specialLabel.Text = value; }
    }
    
    /// <summary>
    /// Text displayed by the cell next to the image
    /// </summary>
    [DefaultValue("")]
    [Description("Text displayed by the second label (in DoubleTextRight mode).")]
    public string SecondText {
      get { return m_secondSpecialLabel.Text; }
      set { m_secondSpecialLabel.Text = value; }
    }
    
    /// <summary>
    /// Image displayed by the cell next to the text
    /// Set the ImageSize first
    /// </summary>
    [Description("Image displayed by the cell. Set the ImageSize first!")]
    public Image Image {
      get { return m_specialPictureBox.Image; }
      set { m_specialPictureBox.Image = ScaleImage(value, m_specialPictureBox.Width, m_specialPictureBox.Height); }
    }
    
    /// <summary>
    /// Size of the image (margins included)
    /// </summary>
    [Description("Size of the image (margins included).")]
    public Size ImageSize {
      get { return m_specialPictureBox.Size; }
      set {
        baseLayout.ColumnStyles[1].Width = value.Width;
        if (DisplayMode == Mode.DoubleTextRight) {
          baseLayout.RowStyles[1].Height = (float)Math.Floor((double)value.Height / 2.0);
          baseLayout.RowStyles[2].Height = (float)Math.Ceiling((double)value.Height / 2.0);
        } else {
          baseLayout.RowStyles[1].Height = value.Height;
        }
      }
    }
    
    /// <summary>
    /// Margins of the image
    /// </summary>
    [Description("Margins of the image.")]
    public Padding ImageMargin {
      get { return m_specialPictureBox.Margin; }
      set { m_specialPictureBox.Margin = value; }
    }
    
    /// <summary>
    /// Image border style
    /// </summary>
    [DefaultValue(BorderStyle.None)]
    [Description("Image border style.")]
    public BorderStyle ImageBorderStyle {
      get { return m_specialPictureBox.BorderStyle; }
      set { m_specialPictureBox.BorderStyle = value; }
    }
    
    /// <summary>
    /// Brush used to display the background of the image
    /// Can be null
    /// </summary>
    public Brush ImageBackgroundBrush {
      get { return m_specialPictureBox.BackgroundBrush; }
      set { m_specialPictureBox.BackgroundBrush = value; }
    }
    
    /// <summary>
    /// Display mode: position and number of the labels
    /// </summary>
    [DefaultValue(Mode.SingleTextBelow)]
    [Description("Display mode: position and number of the labels.")]
    public Mode DisplayMode {
      get { return m_displayMode; }
      set {
        m_displayMode = value;
        switch (value)
        {
          case Mode.SingleTextBelow:
            // One label below the image
            baseLayout.ColumnStyles[0].Width = 50;
            baseLayout.ColumnStyles[2].Width = 50;
            baseLayout.ColumnStyles[3].Width = 0;
            baseLayout.RowStyles[0].Height = 0;
            baseLayout.RowStyles[2].Height = 0;
            baseLayout.RowStyles[3].Height = 100;
            
            // Picture box
            baseLayout.SetRowSpan(m_specialPictureBox, 1);
            
            // First label
            baseLayout.Controls.Add(m_specialLabel, 0, 3);
            baseLayout.SetColumnSpan(m_specialLabel, 3);
            baseLayout.SetRowSpan(m_specialLabel, 1);
            m_specialLabel.TextAlign = ContentAlignment.MiddleCenter;
            m_specialLabel.Margin = new Padding(1, 0, 1, 0);
            
            // Second label
            m_secondSpecialLabel.Hide();
            break;
          case Mode.SingleTextRight:
            // One label to the right
            baseLayout.ColumnStyles[0].Width = 0;
            baseLayout.ColumnStyles[2].Width = 0;
            baseLayout.ColumnStyles[3].Width = 100;
            baseLayout.RowStyles[0].Height = 50;
            baseLayout.RowStyles[2].SizeType = SizeType.Percent;
            baseLayout.RowStyles[2].Height = 50;
            baseLayout.RowStyles[3].Height = 0;
            
            // Picture box
            baseLayout.SetRowSpan(m_specialPictureBox, 1);
            
            // First label
            baseLayout.Controls.Add(m_specialLabel, 3, 0);
            baseLayout.SetColumnSpan(m_specialLabel, 1);
            baseLayout.SetRowSpan(m_specialLabel, 3);
            m_specialLabel.TextAlign = ContentAlignment.MiddleLeft;
            m_specialLabel.Margin = new Padding(1, 0, 1, 0);
            
            // Second label
            m_secondSpecialLabel.Hide();
            break;
          case Mode.DoubleTextRight:
            // Two labels to the right
            baseLayout.ColumnStyles[0].Width = 0;
            baseLayout.ColumnStyles[2].Width = 0;
            baseLayout.ColumnStyles[3].Width = 100;
            baseLayout.RowStyles[0].Height = 50;
            baseLayout.RowStyles[2].SizeType = SizeType.Absolute;
            baseLayout.RowStyles[3].Height = 50;
            
            // Picture box
            baseLayout.SetRowSpan(m_specialPictureBox, 2);
            
            // First label
            baseLayout.Controls.Add(m_specialLabel, 3, 0);
            baseLayout.SetColumnSpan(m_specialLabel, 1);
            baseLayout.SetRowSpan(m_specialLabel, 2);
            m_specialLabel.TextAlign = ContentAlignment.BottomLeft;
            m_specialLabel.Margin = new Padding(1, 0, 1, 2);
            
            // Second label
            baseLayout.Controls.Add(m_secondSpecialLabel, 3, 2);
            baseLayout.SetColumnSpan(m_secondSpecialLabel, 1);
            baseLayout.SetRowSpan(m_secondSpecialLabel, 2);
            m_secondSpecialLabel.TextAlign = ContentAlignment.TopLeft;
            m_secondSpecialLabel.Show();
            break;
        }
      }
    }
    
    /// <summary>
    /// If true, a long text will be displayed on several lines automatically
    /// </summary>
    [DefaultValue(true)]
    [Description("If true, a long text will be displayed on several lines automatically.")]
    public bool AutoLineBreak { get; set; }
    
    /// <summary>
    /// List of the different elements in the menu
    /// An empty list or a null value will remove the menu
    /// </summary>
    public IList<string> MenuElements {
      get {
        IList<string> items = new List<string>();
        if (m_menu != null) {
          foreach (ToolStripMenuItem element in m_menu.Items) {
            items.Add(element.Text);
          }
        }

        return items;
      }
      
      set {
        m_menu.Items.Clear();
        int count = 0;
        if (value != null && value.Count > 0) {
          foreach (string str in value) {
            ToolStripItem item = m_menu.Items.Add(str, null, new EventHandler (OnMenuClicked));
            item.Tag = count++;
          }
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public ClickableCell(object parent)
    {
      InitializeComponent();
      AutoLineBreak = true;
      HoverColor = SystemColors.Highlight;
      
      // First label
      m_specialLabel = new SpecialLabel(this);
      m_specialLabel.Dock = DockStyle.Fill;
      
      // Second label
      m_secondSpecialLabel = new SpecialLabel(this);
      m_secondSpecialLabel.Margin = new Padding(1, 2, 1, 0);
      m_secondSpecialLabel.Dock = DockStyle.Fill;
      m_secondSpecialLabel.ForeColor = SystemColors.GrayText;
      m_secondSpecialLabel.Font = new Font(m_secondSpecialLabel.Font, FontStyle.Italic);
      
      // Image
      m_specialPictureBox.Margin = new Padding(0);
      m_specialPictureBox.Dock = DockStyle.Fill;
      baseLayout.Controls.Add(m_specialPictureBox, 1, 1);
      DisplayMode = Mode.SingleTextBelow;
      
      m_toolTip = new ToolTip();
      m_toolTip.SetToolTip(m_specialLabel, "");
      m_toolTip.SetToolTip(m_secondSpecialLabel, "");
      
      m_parent = parent;
      if (!s_mapCells.ContainsKey(parent)) {
        s_mapCells[m_parent] = new List<ClickableCell>();
      }

      s_mapCells[m_parent].Add(this);
      
      // Menu
      m_menu = new ContextMenuStrip();
    }
    #endregion // Constructors

    #region Methods
    static Image ScaleImage(Image image, int maxWidth, int maxHeight)
    {
      var ratioX = (double)maxWidth / image.Width;
      var ratioY = (double)maxHeight / image.Height;
      var ratio = Math.Min(ratioX, ratioY);

      var newWidth = (int)(image.Width * ratio);
      var newHeight = (int)(image.Height * ratio);

      var newImage = new Bitmap(maxWidth, maxHeight);
      Graphics.FromImage(newImage).DrawImage(image,
                                             (maxWidth - newWidth) / 2,
                                             (maxHeight - newHeight) / 2,
                                             newWidth, newHeight);
      return newImage;
    }
    #endregion // Methods
    
    #region Event reactions
    void OnMenuClicked(object sender, EventArgs e) {
      var item = (ToolStripMenuItem)sender;
      if (MenuClicked != null) {
        MenuClicked ((int)item.Tag);
      }
    }
    
    void ClickableCellMouseClick(object sender, MouseEventArgs e)
    {
      // Remove selection for all other cells having the same parent
      IList<ClickableCell> sisterCells = s_mapCells[m_parent];
      foreach (ClickableCell cell in sisterCells) {
        if (cell != this) {
          if (DisplayMode == Mode.DoubleTextRight) {
            cell.m_specialLabel.Margin = new Padding(1, 0, 1, 2);
          }
          else {
            cell.m_specialLabel.Margin = new Padding(1, 0, 1, 0);
          }

          cell.m_secondSpecialLabel.Margin = new Padding(1, 2, 1, 0);
          cell.BorderStyle = BorderStyle.None;
        }
      }
      
      // Select this cell
      this.BorderStyle = BorderStyle.FixedSingle;
      if (DisplayMode == Mode.DoubleTextRight) {
        m_specialLabel.Margin = new Padding(0, 0, 0, 2);
      }
      else {
        m_specialLabel.Margin = new Padding(0, 0, 0, 0);
      }

      m_secondSpecialLabel.Margin = new Padding(0, 2, 0, 0);
      
      // Display menu
      if (m_menu != null && m_menu.Items.Count > 0 && e.Button == MouseButtons.Right) {
        m_menu.Show(this, e.Location);
      }
    }
    
    void ClickableCellMouseEnter(object sender, EventArgs e)
    {
      m_previousBackgroundColor = this.BackColor;
      this.BackColor = HoverColor;
    }
    
    void ClickableCellMouseLeave(object sender, EventArgs e)
    {
      this.BackColor = m_previousBackgroundColor;
      m_toolTip.Hide(m_specialLabel);
      m_toolTip.Hide(m_secondSpecialLabel);
    }
    
    void ClickableCellMouseHover(object sender, EventArgs e)
    {
      Point point = m_specialLabel.PointToClient(Cursor.Position);
      if (Tooltip != "") {
        m_toolTip.Show(Tooltip, m_specialLabel, point.X + 11, point.Y + 14);
      }
    }
    
    void ClickableCellEnabledChanged(object sender, EventArgs e)
    {
      if (this.Enabled) {
        m_specialLabel.ForeColor = SystemColors.ControlText;
        m_secondSpecialLabel.ForeColor = SystemColors.GrayText;
      } else {
        m_specialLabel.ForeColor = SystemColors.GrayText;
        m_secondSpecialLabel.ForeColor = SystemColors.ControlDark;
        this.BorderStyle = BorderStyle.None;
      }
      m_specialLabel.Refresh();
      m_secondSpecialLabel.Refresh();
    }
    #endregion // Event reactions
  }
}
