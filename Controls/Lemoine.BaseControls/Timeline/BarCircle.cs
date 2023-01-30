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
  /// Description of BarCircle.
  /// </summary>
  public class BarCircle : BarObject
  {
    #region Members
    double m_verticalPosition = 0.5;
    #endregion // Members
    
    static readonly ILog log = LogManager.GetLogger(typeof (BarCircle).FullName);

    #region Getters / Setters
    /// <summary>
    /// Datetime of the circle
    /// </summary>
    public DateTime HorizontalPosition { get; private set; }
    
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
    /// Radius of the circle in pixels
    /// </summary>
    public int Radius { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="position">date of the object</param>
    /// <param name="obj">identifier to find the legend</param>
    /// <param name="text">textual information regarding the object</param>
    public BarCircle(DateTime position, object obj, string text) : base(obj, text)
    {
      HorizontalPosition = position;
      Radius = 5;
      if (VerticalPosition > 1) {
        VerticalPosition = 1;
      }
      else if (VerticalPosition < 0) {
        VerticalPosition = 0;
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Paint the object in the bar
    /// </summary>
    /// <param name="g">Graphics which is painted</param>
    public override void DrawObject(Graphics g)
    {
      int px = CoordToPixel(HorizontalPosition);
      int py = GetPosY();
      g.FillEllipse(Brush, px - Radius, py - Radius, 2 * Radius, 2 * Radius);
      g.DrawEllipse(Pens.Black, px - Radius, py - Radius, 2 * Radius, 2 * Radius);
    }
    
    /// <summary>
    /// Display a tooltip on an object
    /// </summary>
    /// <param name="toolTip">tooltip which will be displayed</param>
    /// <returns>true if the tooltip is displayed</returns>
    public override bool DisplayToolTip(ToolTip toolTip)
    {
      string text = Description + "\nAt: " + HorizontalPosition.ToString("d");
      int posX = CoordToPixel(HorizontalPosition);
      int posY = GetPosY() + Radius;
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
      int posX = CoordToPixel(dateTime);
      int posXref = CoordToPixel(HorizontalPosition);
      int posYref = GetPosY();
      return ((posXref - posX) * (posXref - posX) + (posYref - posY) * (posYref - posY) <= Radius * Radius);
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
