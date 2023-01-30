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
  /// Description of BarSegment.
  /// </summary>
  public class BarSegment : BarObject
  {
    static readonly ILog log = LogManager.GetLogger(typeof (BarSegment).FullName);

    #region Getters / Setters
    /// <summary>
    /// Start datetime of the segment
    /// </summary>
    public DateTime? Start { get; private set; }
    
    /// <summary>
    /// End datetime of the segment
    /// </summary>
    public DateTime? End { get; private set; }
    #endregion // Getters / Setters
    
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="start">left limit of the object</param>
    /// <param name="end">right limit of the object</param>
    /// <param name="obj">identifier to find the legend</param>
    /// <param name="description">textual information regarding the object</param>
    public BarSegment(DateTime? start, DateTime? end, object obj, string description) : base(obj, description)
    {
      Start = start;
      End = end;
    }
    
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="start">left limit of the object</param>
    /// <param name="end">right limit of the object</param>
    /// <param name="obj">identifier to find the legend</param>
    /// <param name="description">textual information regarding the object</param>
    /// <param name="legend">legend of the object (if different from the description)</param>
    public BarSegment(DateTime? start, DateTime? end, object obj, string description, string legend) : base(obj, description, legend)
    {
      Start = start;
      End = end;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Get the intersection with a segment
    /// Return true if the intersection exists
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public bool GetIntersection(ref DateTime start, ref DateTime end)
    {
      if (Start.HasValue && Start.Value > start) {
        start = Start.Value;
      }

      if (End.HasValue && End.Value < end) {
        end = End.Value;
      }

      return start < end;
    }
    
    /// <summary>
    /// Paint the object in the bar
    /// </summary>
    /// <param name="g">Graphics which is painted</param>
    public override void DrawObject(Graphics g)
    {
      DateTime start = Parent.StartDateTime;
      DateTime end = Parent.EndDateTime;
      if (GetIntersection(ref start, ref end)) {
        int pxStart = CoordToPixel(start);
        int pxEnd = CoordToPixel(end);
        g.FillRectangle(Brush, pxStart, Parent.MarginTop + 1,
                        pxEnd - pxStart, Parent.Height - Parent.MarginTop - Parent.MarginBottom - 3);
      }
    }
    
    /// <summary>
    /// Display a tooltip on an object
    /// </summary>
    /// <param name="toolTip">tooltip which will be displayed</param>
    /// <returns>true if the tooltip is displayed</returns>
    public override bool DisplayToolTip(ToolTip toolTip)
    {
      DateTime start = Parent.StartDateTime;
      DateTime end = Parent.EndDateTime;
      if (GetIntersection(ref start, ref end)) {
        string text = Description + "\nFrom: ";
        if (Start.HasValue) {
          text += Start.Value.ToString();
        }
        else {
          text += "no beginning";
        }

        text += "\nTo: ";
        if (End.HasValue) {
          text += End.Value.ToString();
        }
        else {
          text += "no end";
        }

        int pos = (CoordToPixel(start) + CoordToPixel(end)) / 2;
        
        // The second line is necessary, tooltip bug
        toolTip.Show(text, Parent, new Point(pos, Parent.Height - Parent.MarginBottom - 1));
        toolTip.Show(text, Parent, new Point(pos, Parent.Height - Parent.MarginBottom - 1));
        return true;
      }
      
      return false;
    }
    
    /// <summary>
    /// Return true if a position is contained in the object
    /// </summary>
    /// <param name="dateTime">horizontal position of the mouse</param>
    /// <param name="posY">vertical position of the mouse</param>
    /// <returns></returns>
    public override bool Comprises(DateTime dateTime, int posY)
    {
      return ((!Start.HasValue || Start.Value <= dateTime) &&
              (!End.HasValue || End.Value >= dateTime));
    }
    
    /// <summary>
    /// Try a merge with another barObject
    /// Return true if success (in that case, the merged barObject might be modified)
    /// </summary>
    /// <param name="otherBarObject"></param>
    /// <returns></returns>
    public override bool TryMerge(BarObject otherBarObject)
    {
      BarSegment segment = otherBarObject as BarSegment;
      if (segment != null) {
        if (this.Obj.Equals(segment.Obj) && this.End.HasValue &&
            segment.Start.HasValue && this.End.Value == segment.Start.Value) {
          this.End = segment.End;
          return true;
        }
      }
      
      return false;
    }
    #endregion // Methods
  }
}
