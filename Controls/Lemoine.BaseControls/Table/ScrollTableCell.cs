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
  /// Description of ScrollTableCell.
  /// </summary>
  public partial class ScrollTableCell : UserControl
  {
    #region Members
    ToolTip m_tooltip = null;
    #endregion // Members
    
    #region Events
    /// <summary>
    /// Emitted when control changed (checked, value modified...)
    /// First argument is the row, second one is the column
    /// </summary>
    public event Action<int, int> ControlChanged;
    #endregion // Events
    
    #region Getters / Setters
    /// <summary>
    /// Control contained in the cell
    /// </summary>
    public Control Control {
      get {
        return baseLayout.GetControlFromPosition(1, 0);
      }
      private set {
        baseLayout.Controls.Add(value, 1, 0);
      }
    }
    
    /// <summary>
    /// Return true if a tooltip is set
    /// </summary>
    public bool HasToolTip {
      get {
        return pictureBox.Visible;
      }
    }
    
    /// <summary>
    /// Return the text contained in the tooltip or an empty string if the tooltip is not set
    /// </summary>
    public string ToolTipText {
      get {
        if (HasToolTip) {
          return m_tooltip.GetToolTip(pictureBox);
        }

        return "";
      }
    }
    
    private int Row { get; set; }
    private int Column { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="row">row of the cell</param>
    /// <param name="column">column of the cell</param>
    /// <param name="checkBoxMode">CheckBoxMode</param>
    public ScrollTableCell(int row, int column, bool checkBoxMode)
    {
      InitializeComponent();
      Row = row;
      Column = column;
      this.Dock = DockStyle.Fill;
      this.Margin = new Padding(3, 1, 0, 0);
      RemoveTooltip();
      
      // Preparation of the control
      Control control;
      if (checkBoxMode) {
        CheckBox check = new CheckBox();
        check.CheckAlign = ContentAlignment.MiddleCenter;
        check.CheckedChanged += OnControlChanged;
        control = check as Control;
      } else {
        NumericUpDown numeric = new NumericUpDown();
        numeric.Minimum = 0;
        numeric.Maximum = 1000000;
        numeric.BorderStyle = BorderStyle.None;
        numeric.ValueChanged += OnControlChanged;
        control = numeric as Control;
      }
      control.Margin = new Padding(0, 1, 0, 0);
      control.Dock = DockStyle.Fill;
      Control = control;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set a tooltip associated with an image
    /// </summary>
    /// <param name="image">size max of the image;: 15 * 15 px</param>
    /// <param name="text"></param>
    public void SetTooltip(Image image, string text)
    {
      if (m_tooltip != null) {
        m_tooltip.Dispose();
      }

      pictureBox.Image = image;
      m_tooltip = new ToolTip();
      m_tooltip.SetToolTip(pictureBox, text);
      baseLayout.ColumnStyles[0].Width = 18;
      if (Control is CheckBox) {
        Control.Margin = new Padding(0, 1, 18, 0);
      }

      pictureBox.Visible = true;
    }
    
    /// <summary>
    /// Remove the image and the tooltip associated
    /// </summary>
    public void RemoveTooltip()
    {
      if (m_tooltip != null) {
        m_tooltip.Dispose();
        m_tooltip = null;
      }
      baseLayout.ColumnStyles[0].Width = 0;
      if (Control is CheckBox) {
        Control.Margin = new Padding(0, 1, 0, 0);
      }

      pictureBox.Visible = false;
    }
    #endregion // Methods
    
    #region Event reactions
    private void OnControlChanged(object sender, EventArgs e)
    {
      ControlChanged(this.Row, this.Column);
    }
    #endregion // Event reactions
  }
}
