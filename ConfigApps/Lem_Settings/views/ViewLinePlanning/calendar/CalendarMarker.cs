// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Model;

namespace ViewLinePlanning
{
  /// <summary>
  /// Description of CalendarMarker.
  /// </summary>
  public partial class CalendarMarker : UserControl
  {
    #region Members
    Brush m_brush;
    readonly ILine m_line;
    ToolTip m_toolTip = null;
    #endregion // Members

    #region Events
    public event Action<ILine> MarkerDoubleClicked;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="brush"></param>
    /// <param name="toolTip"></param>
    /// <param name="line"></param>
    public CalendarMarker(Brush brush, string toolTip, ILine line)
    {
      m_brush = brush;
      m_line = line;
      
      InitializeComponent();
      m_toolTip = new ToolTip();
      m_toolTip.AutomaticDelay = 100;
      m_toolTip.InitialDelay = 100;
      m_toolTip.AutoPopDelay = 32000;
      m_toolTip.SetToolTip(this, toolTip);
    }
    
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="brush"></param>
    /// <param name="line"></param>
    public CalendarMarker(Brush brush, ILine line)
    {
      m_brush = brush;
      m_line = line;
      
      InitializeComponent();
    }
    #endregion // Constructors

    #region Methods
    protected override void OnPaint(PaintEventArgs e)
    {
      base.OnPaint(e);
      e.Graphics.FillRectangle(m_brush, e.ClipRectangle);
    }
    #endregion // Methods
    
    #region Event reactions
    void CalendarMarkerMouseEnter(object sender, EventArgs e)
    {
      this.Focus();
    }
    
    void CalendarMarkerMouseDoubleClick(object sender, MouseEventArgs e)
    {
      MarkerDoubleClicked(m_line);
    }
    #endregion // Event reactions
  }
}
