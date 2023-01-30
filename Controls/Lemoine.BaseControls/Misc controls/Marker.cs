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
  /// Simple marker to integrate in an interface (legend for instance),
  /// representing a square with a 3D border filled with a brush
  /// </summary>
  public class Marker: UserControl
  {
    #region Members
    Brush m_brush;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Brush that fills the marker
    /// If null, no color will be applied
    /// </summary>
    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Brush Brush {
      get {
        return m_brush;
      }
      set {
        m_brush = value;
        this.Invalidate();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public Marker() : base()
    {
      Brush = null;
      BorderStyle = BorderStyle.Fixed3D;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Redefine the paint method
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      if (Brush != null) {
        e.Graphics.FillRectangle(Brush, e.ClipRectangle);
      }
    }
    #endregion // Methods
  }
}
