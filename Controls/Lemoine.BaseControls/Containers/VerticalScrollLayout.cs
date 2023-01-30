// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of VerticalScrollLayout.
  /// </summary>
  public partial class VerticalScrollLayout : UserControl
  {
    const bool DefaultFlowLayoutMode = false;
    const int DefaultMouseWheelStep = 15;
    const int DefaultScrollBarWidth = 14;
    const int DefaultTitleHeight = 13;
    const bool DefaultScrollCtrlDisable = false;

    #region Events
    /// <summary>
    /// Event emitted when the area of the control (without the scrollbar) is double clicked.
    /// </summary>
    public event Action ContainerDoubleClicked;

    /// <summary>
    /// Event emitted when the visibility of the scrollbar changed
    /// The first argument is the new visibility of the scrollbar
    /// </summary>
    public event Action<bool> ScrollbarVisibilityChanged;
    #endregion // Events

    #region Getters / Setters
    /// <summary>
    /// If activated, the controls will be placed within a flow layout instead of a table
    /// </summary>
    [DefaultValue (DefaultFlowLayoutMode)]
    [Description ("If activated, the controls will be placed within a flow layout instead of a table.")]
    public bool FlowLayoutMode
    {
      get { return m_flowLayoutMode; }
      set {
        Clear ();
        m_flowLayoutMode = value;
        table.Visible = !m_flowLayoutMode;
        flow.Visible = m_flowLayoutMode;
      }
    }
    bool m_flowLayoutMode = DefaultFlowLayoutMode;

    /// <summary>
    /// Number of pixels moved when using the mousewheel
    /// </summary>
    [DefaultValue (DefaultMouseWheelStep)]
    [Description ("Number of pixels moved when using the mousewheel.")]
    public int MouseWheelStep { get; set; }

    /// <summary>
    /// Width of the vertical scrollbar when it's visible
    /// </summary>
    [DefaultValue (DefaultScrollBarWidth)]
    [Description ("Width of the vertical scrollbar when it's visible.")]
    public int ScrollBarWidth { get; set; }

    /// <summary>
    /// Title of the layout. If empty, the title will be hidden.
    /// </summary>
    [DefaultValue ("")]
    [Description ("Title of the layout. If empty, the title will be hidden.")]
    public string Title
    {
      get {
        return labelTitle.Text;
      }
      set {
        labelTitle.Text = value;
        if (labelTitle.Text == "") {
          baseLayout.RowStyles[0].Height = 0;
        }
        else {
          baseLayout.RowStyles[0].Height = TitleHeight;
        }
      }
    }

    /// <summary>
    /// Height of the section comprising the title, if visible
    /// </summary>
    [DefaultValue (DefaultTitleHeight)]
    [Description ("Height of the section comprising the title, if visible.")]
    public int TitleHeight
    {
      get { return m_titleHeight; }
      set {
        m_titleHeight = value;
        if (labelTitle.Text != "") {
          baseLayout.RowStyles[0].Height = TitleHeight;
        }
      }
    }
    int m_titleHeight = DefaultTitleHeight;

    /// <summary>
    /// Font used for the title
    /// </summary>
    [Description ("Font used for the title.")]
    public Font TitleFont
    {
      get { return labelTitle.Font; }
      set {
        if (value == null) {
          labelTitle.Font = base.Font;
        }
        else {
          labelTitle.Font = value;
        }
      }
    }

    /// <summary>
    /// Color used for the title
    /// </summary>
    [Description ("Color used for the title.")]
    public Color TitleForeColor
    {
      get { return labelTitle.ForeColor; }
      set {
        labelTitle.ForeColor = value;
      }
    }

    /// <summary>
    /// Column count (in case where FlowLayout is not used)
    /// </summary>
    [Description ("Column count (in case where FlowLayout is not used).")]
    [DefaultValue (0)]
    public int ColumnCount
    {
      get { return table.ColumnCount; }
      set { table.ColumnCount = value; }
    }

    /// <summary>
    /// Row count (in case where FlowLayout is not used)
    /// </summary>
    [Description ("Row count (in case where FlowLayout is not used).")]
    [DefaultValue (0)]
    public int RowCount
    {
      get { return table.RowCount; }
      set { table.RowCount = value; }
    }

    /// <summary>
    /// Controls contained in the layout
    /// </summary>
    [Browsable (false)]
    public ControlCollection ControlsInLayout
    {
      get {
        return FlowLayoutMode ? flow.Controls : table.Controls;
      }
    }

    /// <summary>
    /// Styles of the columns (in case where FlowLayout is not used)
    /// </summary>
    [Browsable (false), EditorBrowsable (EditorBrowsableState.Never)]
    public TableLayoutColumnStyleCollection ColumnStyles
    {
      get { return table.ColumnStyles; }
    }

    /// <summary>
    /// Styles of the rows (in case where FlowLayout is not used)
    /// </summary>
    [Browsable (false), EditorBrowsable (EditorBrowsableState.Never)]
    public TableLayoutRowStyleCollection RowStyles
    {
      get { return table.RowStyles; }
    }

    /// <summary>
    /// Margin of the area comprising the controls
    /// </summary>
    [Description ("Margin of the area comprising the controls.")]
    public Padding ContainerMargin
    {
      get { return panel.Margin; }
      set { panel.Margin = value; }
    }

    /// <summary>
    /// Disable the scrolling if the ctrl key is down
    /// (in case you want to catch "ctrl + wheel" for something else)
    /// </summary>
    [Description ("Disable the scrolling if the ctrl key is down.")]
    [DefaultValue (false)]
    public bool ScrollCtrlDisabled
    {
      get { return m_scrollCtrlDisabled; }
      set { m_scrollCtrlDisabled = value; }
    }
    bool m_scrollCtrlDisabled = DefaultScrollCtrlDisable;

    /// <summary>
    /// Visibility of the vertical scrollbar
    /// </summary>
    [Browsable (false), EditorBrowsable (EditorBrowsableState.Never)]
    public bool ScrollbarVisible
    {
      get { return baseLayout.ColumnStyles[1].Width > 0; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public VerticalScrollLayout ()
    {
      InitializeComponent ();

      // Default values
      Title = "";
      FlowLayoutMode = DefaultFlowLayoutMode;
      MouseWheelStep = DefaultMouseWheelStep;
      ScrollBarWidth = DefaultScrollBarWidth;

      // Reactions
      panel.MouseWheel += OnPanelMouseWheel;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Clear and dispose all controls from the layout
    /// </summary>
    public void Clear ()
    {
      Utils.ClearControls (flow.Controls);
      Utils.ClearControls (table.Controls);

      table.RowStyles.Clear ();
      table.ColumnStyles.Clear ();
      table.ColumnCount = 0;
      table.RowCount = 0;
      table.Height = 0;
      if (ScrollbarVisible && ScrollbarVisibilityChanged != null) {
        ScrollbarVisibilityChanged (false);
      }

      baseLayout.ColumnStyles[1].Width = 0;
    }

    /// <summary>
    /// Remove all controls from the layout (they are not disposed)
    /// </summary>
    public void RemoveAll ()
    {
      flow.Controls.Clear ();
      table.Controls.Clear ();
      table.RowStyles.Clear ();
      table.ColumnStyles.Clear ();
      table.ColumnCount = 0;
      table.RowCount = 0;
      if (ScrollbarVisible && ScrollbarVisibilityChanged != null) {
        ScrollbarVisibilityChanged (false);
      }

      baseLayout.ColumnStyles[1].Width = 0;
    }

    /// <summary>
    /// Add a control in the layout at the specified location
    /// The location will be ignored if FlowLayoutMode is set
    /// </summary>
    /// <param name="control"></param>
    /// <param name="column"></param>
    /// <param name="row"></param>
    public void AddControl (Control control, int column, int row)
    {
      if (FlowLayoutMode) {
        flow.Controls.Add (control);
      }
      else {
        table.Controls.Add (control, column, row);
      }

      VerticalScrollLayoutSizeChanged (null, null);
    }

    /// <summary>
    /// Add a control in the layout
    /// </summary>
    /// <param name="control"></param>
    public void AddControl (Control control)
    {
      if (FlowLayoutMode) {
        flow.Controls.Add (control);
      }
      else {
        table.Controls.Add (control);
      }

      VerticalScrollLayoutSizeChanged (null, null);
    }

    /// <summary>
    /// Force updating the scrollbar regarding the size of the table content
    /// </summary>
    public void UpdateScroll ()
    {
      VerticalScrollLayoutSizeChanged (null, null);
    }

    /// <summary>
    /// Scroll the vertical layout so that a control is displayed on the top
    /// </summary>
    /// <param name="control"></param>
    public void ScrollTo (Control control)
    {
      vScroll.Value = control.Location.Y;
      VerticalScrollLayoutSizeChanged (null, null);
      UpdateLocation ();
      ControlAnimation.AnimateBackColor (control, Color.LightGreen, BackColor, 0.8);
    }

    void UpdateLocation ()
    {
      if (FlowLayoutMode) {
        flow.Location = new Point (0, -vScroll.Value);
      }
      else {
        table.Location = new Point (0, -vScroll.Value);
      }
    }
    #endregion // Methods

    #region Event reactions
    void VerticalScrollLayoutSizeChanged (object sender, EventArgs e)
    {
      Control container = FlowLayoutMode ? flow as Control : table as Control;
      using (new SuspendDrawing (baseLayout)) {
        // Visibility of the scrollbar
        container.MinimumSize = new Size (baseLayout.Width - panel.Margin.Horizontal, 0);
        container.MaximumSize = new Size (baseLayout.Width - panel.Margin.Horizontal, 8000);
        if (container.Height > panel.Height) {
          if (!ScrollbarVisible && ScrollbarVisibilityChanged != null) {
            ScrollbarVisibilityChanged (true);
          }

          baseLayout.ColumnStyles[1].Width = ScrollBarWidth;
          container.MinimumSize = new Size (baseLayout.Width - panel.Margin.Horizontal - ScrollBarWidth, 0);
          container.MaximumSize = new Size (baseLayout.Width - panel.Margin.Horizontal - ScrollBarWidth, 8000);
        }
        else {
          if (ScrollbarVisible && ScrollbarVisibilityChanged != null) {
            ScrollbarVisibilityChanged (false);
          }

          baseLayout.ColumnStyles[1].Width = 0;
        }

        // Vertical scroll
        vScroll.Maximum = container.Height;
        vScroll.LargeChange = panel.Height;
        if (vScroll.Value > vScroll.Maximum - vScroll.LargeChange) {
          vScroll.Value = Math.Max (vScroll.Minimum, vScroll.Maximum - vScroll.LargeChange);
          UpdateLocation ();
        }
      }
    }

    void VScrollScroll (object sender, ScrollEventArgs e)
    {
      UpdateLocation ();
    }

    void OnPanelMouseWheel (object sender, MouseEventArgs e)
    {
      if (ScrollCtrlDisabled && (ModifierKeys & Keys.Control) == Keys.Control) {
        return;
      }

      int newValue = vScroll.Value - e.Delta / 120 * MouseWheelStep;
      if (newValue < vScroll.Minimum) {
        newValue = vScroll.Minimum;
      }

      if (newValue > vScroll.Maximum - vScroll.LargeChange) {
        newValue = Math.Max (vScroll.Minimum, vScroll.Maximum - vScroll.LargeChange);
      }

      vScroll.Value = newValue;
      UpdateLocation ();
    }

    void LabelTitleMouseDoubleClick (object sender, MouseEventArgs e)
    {
      if (ContainerDoubleClicked != null) {
        ContainerDoubleClicked ();
      }
    }

    void PanelMouseDoubleClick (object sender, MouseEventArgs e)
    {
      if (ContainerDoubleClicked != null) {
        ContainerDoubleClicked ();
      }
    }

    void FlowMouseDoubleClick (object sender, MouseEventArgs e)
    {
      if (ContainerDoubleClicked != null) {
        ContainerDoubleClicked ();
      }
    }

    void TableMouseDoubleClick (object sender, MouseEventArgs e)
    {
      if (ContainerDoubleClicked != null) {
        ContainerDoubleClicked ();
      }
    }

    void TableMouseClick (object sender, MouseEventArgs e)
    {
      this.Focus ();
    }
    #endregion // Event reactions
  }
}
