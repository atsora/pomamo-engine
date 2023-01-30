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
  /// TextBox with a watermark (text displayed as default - usually in gray, before any input)
  /// http://www.codeproject.com/Articles/319910/Custom-TextBox-with-watermark
  /// </summary>
  public class TextBoxWatermarked : TextBox
  {
    #region Members
    string m_waterMarkText = "Default Watermark...";
    Color m_waterMarkColor;
    Color m_waterMarkActiveColor;
    Panel m_waterMarkContainer;
    Font m_waterMarkFont;
    SolidBrush m_waterMarkBrush;
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Text displayed in the watermark
    /// </summary>
    [Category("Watermark attribtues")]
    [Description("Sets the text of the watermark")]
    public string WaterMark
    {
      get { return m_waterMarkText; }
      set {
        m_waterMarkText = value;
        this.Invalidate();
      }
    }

    /// <summary>
    /// WaterMark active text color
    /// </summary>
    [Category("Watermark attribtues")]
    [Description("When the control gaines focus, " +
                 "this color will be used as the watermark's forecolor")]
    public Color WaterMarkActiveForeColor
    {
      get { return m_waterMarkActiveColor; }
      set {
        m_waterMarkActiveColor = value;
        this.Invalidate();
      }
    }

    /// <summary>
    /// WaterMark inactive text color
    /// </summary>
    [Category("Watermark attribtues")]
    [Description("When the control looses focus, this color " +
                 "will be used as the watermark's forecolor")]
    public Color WaterMarkForeColor
    {
      get { return m_waterMarkColor; }
      set {
        m_waterMarkColor = value;
        this.Invalidate();
      }
    }

    /// <summary>
    /// WaterMark font
    /// </summary>
    [Category("Watermark attribtues")]
    [Description("The font used on the watermark. " +
                 "Default is the same as the control")]
    public Font WaterMarkFont
    {
      get { return m_waterMarkFont; }
      set {
        m_waterMarkFont = value;
        this.Invalidate();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TextBoxWatermarked()
    {
      Initialize();
    }
    #endregion // Constructors

    #region Methods
    void Initialize()
    {
      // Sets some default values to the watermark properties
      m_waterMarkColor = Color.LightGray;
      m_waterMarkActiveColor = Color.Gray;
      m_waterMarkFont = this.Font;
      m_waterMarkBrush = new SolidBrush(m_waterMarkActiveColor);
      m_waterMarkContainer = null;

      // Draw the watermark, so we can see it in design time
      DrawWaterMark();

      // Eventhandlers which contains function calls.
      // Either to draw or to remove the watermark
      this.Enter += ThisHasFocus;
      this.Leave += ThisWasLeaved;
      this.TextChanged += ThisTextChanged;
    }
    
    void RemoveWaterMark()
    {
      if (m_waterMarkContainer != null) {
        this.Controls.Remove(m_waterMarkContainer);
        m_waterMarkContainer = null;
      }
    }
    
    void DrawWaterMark()
    {
      if (this.m_waterMarkContainer == null && this.TextLength <= 0) {
        m_waterMarkContainer = new Panel(); // Creates the new panel instance
        m_waterMarkContainer.Paint += waterMarkContainer_Paint;
        m_waterMarkContainer.Invalidate();
        m_waterMarkContainer.Click += waterMarkContainer_Click;
        this.Controls.Add(m_waterMarkContainer); // adds the control
      }
    }
    #endregion // Methods
    
    #region Event reactions
    void waterMarkContainer_Click(object sender, EventArgs e)
    {
      this.Focus(); // Makes sure you can click wherever you want on the control to gain focus
    }

    void waterMarkContainer_Paint(object sender, PaintEventArgs e)
    {
      // Setting the watermark container up
      m_waterMarkContainer.Location = new Point(2, 0); // sets the location
      m_waterMarkContainer.Height = this.Height; // Height should be the same as its parent
      m_waterMarkContainer.Width = this.Width; // same goes for width and the parent
      m_waterMarkContainer.Anchor = AnchorStyles.Left | AnchorStyles.Right; // makes sure that it resizes with the parent control
      m_waterMarkBrush = new SolidBrush(this.ContainsFocus ? m_waterMarkActiveColor : m_waterMarkColor);

      //Drawing the string into the panel
      Graphics g = e.Graphics;
      g.DrawString(m_waterMarkText, m_waterMarkFont, m_waterMarkBrush, new PointF(-2f, 1f)); // Take a look at that point
      // The reason I'm using the panel at all, is because of this feature, that it has no limits
      // I started out with a label but that looked very very bad because of its paddings
    }
    
    void ThisHasFocus(object sender, EventArgs e)
    {
      // if focused use focus color
      m_waterMarkBrush = new SolidBrush(m_waterMarkActiveColor);

      // The watermark should not be drawn if the user has already written some text
      if (this.TextLength <= 0) {
        RemoveWaterMark();
        DrawWaterMark();
      }
    }

    void ThisWasLeaved(object sender, EventArgs e)
    {
      // if the user has written something and left the control
      if (this.TextLength > 0) {
        // Remove the watermark
        RemoveWaterMark();
      } else {
        // But if the user didn't write anything, Then redraw the control.
        this.Invalidate();
      }
    }

    void ThisTextChanged(object sender, EventArgs e)
    {
      // If the text of the textbox is not empty
      if (this.TextLength > 0) {
        // Remove the watermark
        RemoveWaterMark();
      } else {
        // But if the text is empty, draw the watermark again.
        DrawWaterMark();
      }
    }

    /// <summary>
    /// On paint
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);
      // Draw the watermark even in design time
      DrawWaterMark();
    }

    /// <summary>
    /// On invalidated
    /// </summary>
    /// <param name="e"></param>
    protected override void OnInvalidated(InvalidateEventArgs e)
    {
      base.OnInvalidated(e);
      // Check if there is a watermark
      if (m_waterMarkContainer != null) {
        // if there is a watermark it should also be invalidated();
        m_waterMarkContainer.Invalidate();
      }
    }
    #endregion // Event reactions
  }
}
