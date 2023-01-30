// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using System.ComponentModel;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of TransparentLabel.
  /// https://stackoverflow.com/questions/34335157/show-a-label-with-semi-transparent-backcolor-above-other-controls
  /// </summary>
  public class TransparentLabel : LinkLabel
  {
    #region Members
    int m_opacity;
    Color m_transparentBackColor;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Opacity of the background
    /// </summary>
    public int Opacity {
      get { return m_opacity; }
      set {
        if (value >= 0 && value <= 255) {
          m_opacity = value;
        }

        this.Invalidate();
      }
    }

    /// <summary>
    /// Color of the background
    /// </summary>
    public Color TransparentBackColor {
      get { return m_transparentBackColor; }
      set {
        m_transparentBackColor = value;
        this.Invalidate();
      }
    }

    /// <summary>
    /// Override back color: this is always transparent
    /// (no need to use it)
    /// </summary>
    [Browsable(false)]
    public override Color BackColor {
      get { return Color.Transparent; }
      set { base.BackColor = Color.Transparent; }
    }
    #endregion // Getters / Setters
    
    #region Constructors / Destructors
    /// <summary>
    /// Default constructor
    /// </summary>
    public TransparentLabel()
    {
      this.m_transparentBackColor = Color.Blue;
      this.m_opacity = 50;
      this.BackColor = Color.Transparent;
    }
    #endregion // Constructors / Destructors
    
    #region Event reactions
    /// <summary>
    /// Display what is behind the control and the control itself
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
      if (Parent != null) {
        using (var bmp = new Bitmap(Parent.Width, Parent.Height)) {
          Parent.Controls.Cast<Control>()
            .Where(c => Parent.Controls.GetChildIndex(c) > Parent.Controls.GetChildIndex(this))
            .Where(c => c.Bounds.IntersectsWith(this.Bounds))
            .OrderByDescending(c => Parent.Controls.GetChildIndex(c))
            .ToList()
            .ForEach(c => c.DrawToBitmap(bmp, c.Bounds));

          e.Graphics.DrawImage(bmp, -Left, -Top);
          using (var b = new SolidBrush(Color.FromArgb(this.Opacity, this.TransparentBackColor))) {
            e.Graphics.FillRectangle(b, this.ClientRectangle);
          }
          e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
          TextRenderer.DrawText(e.Graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor, Color.Transparent);
        }
      }
    }
    #endregion // Event reactions
  }
}
