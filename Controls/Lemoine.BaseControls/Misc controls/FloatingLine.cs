// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of FloatingLine.
  /// </summary>
  public class FloatingLine : UserControl
  {
    #region Getters / Setters
    /// <summary>
    /// Position of the vertical bar
    /// </summary>
    public int Position { get; set; }
    
    /// <summary>
    /// Thickness of the bar
    /// </summary>
    public int Thickness { get; set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FloatingLine()
    {
      this.Resize += new System.EventHandler(this.FloatingLineResize);
      this.Enabled = false;
      this.BackColor = SystemColors.ControlDark;
      Position = 0;
      Thickness = 1;
    }
    #endregion // Constructors

    #region Event reactions
    void FloatingLineResize(object sender, EventArgs e)
    {
      Point[] pts = {
        new Point(Position + Margin.Left, Margin.Top),
        new Point(Position + Margin.Left, Height - Margin.Bottom),
        new Point(Position + Margin.Left + Thickness, Height - Margin.Bottom),
        new Point(Position + Margin.Left + Thickness, Margin.Top)
      };
      
      // Start point (0), then line (1)
      byte[] types = {0, 1, 1, 1};

      var path = new GraphicsPath(pts, types);
      this.Region = new Region(path);
    }
    #endregion // Event reactions
  }
}
