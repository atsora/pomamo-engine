// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of NullableColorDialog.
  /// </summary>
  public partial class NullableColorDialog : Form
  {
    #region Members
    bool m_isNull = false;
    double m_currentH = 180;
    double m_currentS = 1.0;
    double m_currentL = 1.0;
    readonly Pen m_circlePen = new Pen(Color.White, 2);
    
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (NullableColorDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Set or get the color chosen
    /// </summary>
    public Color? SelectedColor {
      get {
        return m_isNull ? (Color?)null :
          FromHSLA(m_currentH / 360.0, m_currentS, m_currentL);
      }
      set {
        if (value == null) {
          m_isNull = true;
        }
        else {
          m_currentH = value.Value.GetHue();
          m_currentS = value.Value.GetSaturation();
          m_currentL = value.Value.GetBrightness();
        }
        
        UpdatePanel2D();
        UpdateHex();
      }
    }
    
    /// <summary>
    /// True if the user can choose a null color
    /// </summary>
    public bool NullColorPossible {
      get { return buttonClear.Visible; }
      set {
        buttonClear.Visible = value;
        baseLayout.ColumnStyles[2].Width = value ? 60 : 0;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public NullableColorDialog()
    {
      this.StartPosition = FormStartPosition.CenterParent;
      if (null != Form.ActiveForm) {
        this.Icon = Form.ActiveForm.Icon;
      }
      InitializeComponent();
      
      // Marker colors
      marker1 .BackColor = Color.FromArgb(255,   0,   0);
      marker2 .BackColor = Color.FromArgb(239, 152,  15);
      marker3 .BackColor = Color.FromArgb(229, 229,   0);
      marker4 .BackColor = Color.FromArgb(140, 255, 140);
      marker5 .BackColor = Color.FromArgb(191, 191, 255);
      marker6 .BackColor = Color.FromArgb(255,  41, 255);
      marker7 .BackColor = Color.FromArgb(140, 255, 255);
      marker8 .BackColor = Color.FromArgb(255,  82,  82);
      marker9 .BackColor = Color.FromArgb(  0, 187,   0);
      marker10.BackColor = Color.FromArgb(187, 187,  86);
      marker11.BackColor = Color.FromArgb(133, 133, 255);
      marker12.BackColor = Color.FromArgb(173,   0, 151);
      marker13.BackColor = Color.FromArgb(  0, 197, 197);
      marker14.BackColor = Color.FromArgb( 99,   0,   0);
      marker15.BackColor = Color.FromArgb(  0, 135,   0);
      marker16.BackColor = Color.FromArgb( 52, 103,  30);
      marker17.BackColor = Color.FromArgb( 61,  61, 219);
      marker18.BackColor = Color.FromArgb(107,   0,  94);
      marker19.BackColor = Color.FromArgb(  0, 152, 141);
      marker20.BackColor = Color.FromArgb(  0,  69,  33);
      marker21.BackColor = Color.FromArgb( 29,  43,  14);
      marker22.BackColor = Color.FromArgb(  0,   0, 167);
      marker23.BackColor = Color.FromArgb(143,  78, 126);
      marker24.BackColor = Color.FromArgb(  0,   0,   0);
      marker25.BackColor = Color.FromArgb( 77,  77,  77);
      marker26.BackColor = Color.FromArgb(255, 255, 255);
      
      // Hue
      panelHue.BackgroundImage = CreateGradient(panelHue.ClientRectangle);
      panelHue.Cursor = Cursors.Cross;
      
      // Saturation / Lightness
      Utils.SetDoubleBuffered(panelSL);
      panelSL.Cursor = Cursors.Cross;
      UpdatePanel2D();
    }
    #endregion // Constructors

    #region Methods
    void UpdateHex()
    {
      using (new SuspendDrawing(labelHex)) {
        if (m_isNull) {
          labelHex.BackColor = SystemColors.Control;
          labelHex.ForeColor = SystemColors.ControlText;
          labelHex.Text = "none";
        } else {
          // Hexadecimal representation of the color
          var color = SelectedColor.Value;
          labelHex.Text = String.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
          labelHex.BackColor = color;
          labelHex.ForeColor = (0.3 * color.R + 0.6 * color.G + 0.1 * color.B) > 127 ? Color.Black : Color.White;
        }
      }
      panelSL.Refresh();
    }
    
    void UpdatePanel2D()
    {
      using (new SuspendDrawing(panelSL)) {
        panelSL.BackgroundImage = CreateGradient2D(panelSL.ClientRectangle, m_currentH);
      }
    }
    
    static Bitmap CreateGradient(Rectangle r)
    {
      // Create a bitmap
      var bmp = new Bitmap(r.Width, r.Height);

      // Color each pixel
      for (int x = 0; x < bmp.Width; x++) {
        var color = Rainbow((float)x / bmp.Width);
        for (int y = 0; y < bmp.Height; y++) {
          bmp.SetPixel(x, y, color);
        }
      }
      
      return bmp;
    }
    
    static Bitmap CreateGradient2D(Rectangle r, double hue)
    {
      // Create a bitmap
      var bmp = new Bitmap(r.Width, r.Height);
      
      // Hue between 0 and 1
      hue /= 360.0;
      
      // Color each pixel
      for (int x = 0; x < bmp.Width; x++) {
        for (int y = 0; y < bmp.Height; y++) {
          var s = (float)x / bmp.Width;
          var l = 1.0 - (float)y / bmp.Height;
          bmp.SetPixel(x, y, FromHSLA(hue, s, l));
        }
      }
      
      return bmp;
    }
    
    /// <summary>
    /// Get all possible colors
    /// </summary>
    /// <param name="progress">Between 0 and 1</param>
    /// <returns></returns>
    static Color Rainbow(float progress)
    {
      float div = (Math.Abs(progress % 1) * 6);
      int ascending = (int) ((div % 1) * 255);
      int descending = 255 - ascending;

      switch ((int) div)
      {
        case 0:
          return Color.FromArgb(255, ascending, 0);
        case 1:
          return Color.FromArgb(descending, 255, 0);
        case 2:
          return Color.FromArgb(0, 255, ascending);
        case 3:
          return Color.FromArgb(0, descending, 255);
        case 4:
          return Color.FromArgb(ascending, 0, 255);
        default: // case 5:
          return Color.FromArgb(255, 0, descending);
      }
    }
    
    /// <summary>
    /// http://www.geekymonkey.com/Programming/CSharp/RGB2HSL_HSL2RGB.htm
    /// </summary>
    /// <param name="h">between 0 and 1</param>
    /// <param name="s">between 0 and 1</param>
    /// <param name="l">between 0 and 1</param>
    /// <returns></returns>
    public static Color FromHSLA(double h, double s, double l)
    {
      double v;
      double r,g,b;

      r = l;   // default to gray
      g = l;
      b = l;
      v = (l <= 0.5) ? (l * (1.0 + s)) : (l + s - l * s);

      if (v > 0) {
        double m;
        double sv;
        int sextant;
        double fract, vsf, mid1, mid2;

        m = l + l - v;
        sv = (v - m ) / v;
        h *= 6.0;
        sextant = (int)h;
        fract = h - sextant;
        vsf = v * sv * fract;
        mid1 = m + vsf;
        mid2 = v - vsf;

        switch (sextant) {
          case 0:
            r = v;
            g = mid1;
            b = m;
            break;
          case 1:
            r = mid2;
            g = v;
            b = m;
            break;
          case 2:
            r = m;
            g = v;
            b = mid1;
            break;
          case 3:
            r = m;
            g = mid2;
            b = v;
            break;
          case 4:
            r = mid1;
            g = m;
            b = v;
            break;
          case 5:
            r = v;
            g = m;
            b = mid2;
            break;
        }
      }

      return System.Drawing.Color.FromArgb(
        (int)(r * 255), (int)(g * 255), (int)(b * 255));
    }
    #endregion // Methods
    
    #region Event reactions
    void ButtonOkClick(object sender, EventArgs e)
    {
      m_isNull = false;
      DialogResult = DialogResult.OK;
      Close();
    }
    
    void ButtonClearClick(object sender, EventArgs e)
    {
      m_isNull = true;
      DialogResult = DialogResult.OK;
      Close();
    }
    
    void ButtonCancelClick(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }
    
    void MarkerClick(object sender, EventArgs e)
    {
      var marker = sender as Marker;
      if (marker != null) {
        SelectedColor = marker.BackColor;
      }
    }
    
    void PanelHueMouseDown(object sender, MouseEventArgs e)
    {
      PanelHueMouseMove(sender, e);
    }
    
    void PanelHueMouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left) {
        m_isNull = false;
        var pos = (float)e.X / panelHue.ClientRectangle.Width;
        if (pos < 0) {
          pos = 0;
        }
        else if (pos > 1) {
          pos = 1;
        }

        m_currentH = Rainbow(pos).GetHue();
        UpdatePanel2D();
        UpdateHex();
      }
    }
    
    void PanelSVMouseDown(object sender, MouseEventArgs e)
    {
      PanelSVMouseMove(sender, e);
    }
    
    void PanelSVMouseMove(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left) {
        m_isNull = false;
        m_currentS = (float)e.X / panelSL.ClientRectangle.Width;
        if (m_currentS < 0) {
          m_currentS = 0;
        }
        else if (m_currentS > 1) {
          m_currentS = 1;
        }

        m_currentL = 1.0 - (float)e.Y / panelSL.ClientRectangle.Height;
        if (m_currentL < 0) {
          m_currentL = 0;
        }
        else if (m_currentL > 1) {
          m_currentL = 1;
        }

        UpdateHex ();
      }
    }
    
    void NullableColorDialogShown(object sender, EventArgs e)
    {
      UpdatePanel2D();
    }
    
    void PanelSLPaint(object sender, PaintEventArgs e)
    {
      // Center of the circle
      float x = (float)(m_currentS * panelSL.ClientRectangle.Width);
      float y = (float)((1.0 - m_currentL) * panelSL.ClientRectangle.Height);

      // Display a circle around the right value
      Graphics g = e.Graphics;
      g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
      const float radius = 8.0f;
      g.DrawEllipse(m_circlePen, x - radius, y - radius, 2.0f * radius, 2.0f * radius);
    }
    #endregion // Event reactions
  }
}
