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
  /// Description of BarEllipse.
  /// </summary>
  public class BarEllipse : BarObject
  {
    #region Members
    double m_verticalPosition = 0.5;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (BarEllipse).FullName);

    #region Getters / Setters
    /// <summary>
    /// Left limit of the ellipse
    /// </summary>
    public DateTime LeftPosition { get; private set; }
    
    /// <summary>
    /// Right limit of the ellipse
    /// </summary>
    public DateTime RightPosition { get; private set; }
    
    /// <summary>
    /// Vertical position of the circle
    /// 0 is bottom, 1 is top
    /// </summary>
    public double VerticalPosition {
      get { return m_verticalPosition; }
      set {
        if (value > 1) {
          m_verticalPosition = 1;
        }
        else if (value < 0) {
          m_verticalPosition = 0;
        }
        else {
          m_verticalPosition = value;
        }
      }
    }
    
    /// <summary>
    /// Height of the ellipse in pixels
    /// </summary>
    public int Height { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public BarEllipse(DateTime left, DateTime right, object obj, string text) : base(obj, text)
    {
      LeftPosition = left;
      RightPosition = right;
      Height = 5;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Paint the object in the bar
    /// </summary>
    /// <param name="g">Graphics which is painted</param>
    public override void DrawObject(Graphics g)
    {
      int px1 = CoordToPixel(LeftPosition);
      int px2 = CoordToPixel(RightPosition);
      int py = GetPosY();
      g.FillEllipse(Brush, px1, py - Height, px2 - px1, 2 * Height);
      g.DrawEllipse(Pens.Black, px1, py - Height, px2 - px1, 2 * Height);
    }
    
    /// <summary>
    /// Display a tooltip on an object
    /// </summary>
    /// <param name="toolTip">tooltip which will be displayed</param>
    /// <returns>true if the tooltip is displayed</returns>
    public override bool DisplayToolTip(ToolTip toolTip)
    {
      string text = Description;
      int posX = (CoordToPixel(LeftPosition) + CoordToPixel(RightPosition)) / 2;
      int posY = GetPosY() + Height;
      toolTip.Show(text, Parent, new Point(posX, posY));
      toolTip.Show(text, Parent, new Point(posX, posY));
      return true;
    }
    
    /// <summary>
    /// Return true if a position is contained in the object
    /// </summary>
    /// <param name="dateTime">horizontal position of the mouse</param>
    /// <param name="posY">vertical position of the mouse</param>
    /// <returns></returns>
    public override bool Comprises(DateTime dateTime, int posY)
    {
      double posX = CoordToPixel(dateTime);
      double posX1ref = CoordToPixel(LeftPosition);
      double posX2ref = CoordToPixel(RightPosition);
      double posXcenter = (posX1ref + posX2ref) / 2;
      double width = posX2ref - posX1ref;
      double posYcenter = GetPosY();
      return ((posXcenter - posX) * (posXcenter - posX) / (width * width / 4) +
              (posYcenter - posY) * (posYcenter - posY) / (Height * Height) <= 1);
    }
    
    /// <summary>
    /// Try a merge with another barObject
    /// Return true if success (in that case, the merged barObject might be modified)
    /// </summary>
    /// <param name="otherBarObject"></param>
    /// <returns></returns>
    public override bool TryMerge(BarObject otherBarObject)
    {
      return false;
    }
    
    int GetPosY()
    {
      int pyBottom = Parent.Height - Parent.MarginBottom - 1;
      int pyTop = Parent.MarginTop;
      return (int)(VerticalPosition * pyTop + (1.0 - VerticalPosition) * pyBottom);
    }
    #endregion // Methods
  }
}
