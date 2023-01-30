// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lemoine.BaseControls.Inputs
{
  /// <summary>
  /// Description of ColorPicker.
  /// </summary>
  public partial class ColorPicker : UserControl
  {
    #region Events
    /// <summary>
    /// Event emitted when the current color changed
    /// The first argument is the color
    /// </summary>
    public event Action<Color?> ColorChanged;
    #endregion // Events
    
    #region Members
    Color? m_color;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Get / set the color
    /// </summary>
    [Description("Color chosen.")]
    public Color? SelectedColor {
      get {
        return m_color;
      }
      set {
        m_color = value;
        if (m_color.HasValue) {
          marker.Brush = new SolidBrush(m_color.Value);
          textBox.Text = ColorTranslator.ToHtml(m_color.Value);
        } else {
          marker.Brush = new HatchBrush(HatchStyle.DarkUpwardDiagonal,
                                        SystemColors.ControlDark, SystemColors.Control);
          textBox.Text = "none";
        }
        
        if (ColorChanged != null) {
          ColorChanged (value);
        }
      }
    }
    
    /// <summary>
    /// Compact view
    /// </summary>
    [DefaultValue(false)]
    [Description("Compact view, if true only the marker is displayed.")]
    public bool Compact {
      get {
        return !textBox.Visible;
      }
      set {
        textBox.Visible = !value;
        buttonChangeColor.Visible = !value;
        if (value) {
          baseLayout.ColumnStyles[0].Width = 0;
          baseLayout.ColumnStyles[1].SizeType = SizeType.Percent;
          baseLayout.ColumnStyles[2].Width = 0;
          this.MinimumSize = new Size(0, 0);
        } else {
          baseLayout.ColumnStyles[0].Width = 100;
          baseLayout.ColumnStyles[1].SizeType = SizeType.Absolute;
          baseLayout.ColumnStyles[2].Width = 25;
          this.MinimumSize = new Size(110, 26);
        }
      }
    }
    
    /// <summary>
    /// True if the color can be null
    /// </summary>
    [Description("True if the color can be null")]
    public bool NullColorPossible { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ColorPicker()
    {
      InitializeComponent();
      SelectedColor = Color.Aqua;
    }
    #endregion // Constructors

    #region Event reactions
    void ButtonChangeColorClick(object sender, EventArgs e)
    {
      var dialog = new NullableColorDialog();
      dialog.NullColorPossible = NullColorPossible;
      dialog.SelectedColor = SelectedColor;
      if (dialog.ShowDialog() == DialogResult.OK) {
        SelectedColor = dialog.SelectedColor;
      }
    }
    
    void TextBoxValidated(object sender, EventArgs e)
    {
      if (NullColorPossible && (
        string.IsNullOrEmpty(textBox.Text) || string.Equals(textBox.Text, "none", StringComparison.InvariantCultureIgnoreCase))) {
        SelectedColor = null;
      } else {
        try {
          SelectedColor = ColorTranslator.FromHtml(textBox.Text);
        } catch {
          SelectedColor = m_color;
        }
      }
    }
    
    void MarkerMouseClick(object sender, MouseEventArgs e)
    {
      if (Compact) {
        ButtonChangeColorClick (null, null);
      }
    }
    #endregion // Event reactions
  }
}
