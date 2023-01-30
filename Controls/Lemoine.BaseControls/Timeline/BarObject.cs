// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of BarObject.
  /// </summary>
  public abstract class BarObject
  {
    #region Members
    static DistinctBrushes s_distinctBrushes = new DistinctBrushes();
    static IDictionary<Brush, string> s_legendBrushes = new Dictionary<Brush, string>();
    #endregion // Members

    #region Getters / Setters
    /// <summary>
    /// Brush
    /// </summary>
    public Brush Brush { get; private set; }
    
    /// <summary>
    /// Priority of the tooltip
    /// If several elements are under the mouse, the tooltip of the element having
    /// the highest priority is displayed
    /// Default is 0
    /// </summary>
    public int ToolTipPriority { get; set; }
    
    /// <summary>
    /// Access to the parent bar
    /// </summary>
    internal Bar Parent { get; set; }
    
    /// <summary>
    /// Object represented
    /// </summary>
    protected object Obj { get; private set; }
    
    /// <summary>
    /// Text used to describe the object
    /// </summary>
    protected string Description { get; private set; }
    
    /// <summary>
    /// Text used in the legend
    /// </summary>
    protected string Legend { get; private set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Create a new object to be displayed on the timeline
    /// datetimes must be in local time
    /// </summary>
    /// <param name="obj">identifier to find the legend</param>
    /// <param name="description">textual information regarding the object</param>
    protected BarObject(object obj, string description)
    {
      // Default values
      ToolTipPriority = 0;
      
      Obj = obj;
      Description = Legend = description;
      
      Brush = s_distinctBrushes.GetBrush(obj);
      if (!s_legendBrushes.ContainsKey(Brush)) {
        s_legendBrushes[Brush] = Legend;
      }
    }
    
    /// <summary>
    /// Create a new object to be displayed on the timeline
    /// datetimes must be in local time
    /// </summary>
    /// <param name="obj">identifier to find the legend</param>
    /// <param name="description">textual information regarding the object</param>
    /// <param name="legend">legend regarding the object</param>
    protected BarObject(object obj, string description, string legend)
    {
      // Default values
      ToolTipPriority = 0;
      
      Obj = obj;
      Description = description;
      Legend = legend;
      
      Brush = s_distinctBrushes.GetBrush(obj);
      if (!s_legendBrushes.ContainsKey(Brush)) {
        s_legendBrushes[Brush] = Legend;
      }
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Reset the legend, forget all previous colors
    /// </summary>
    static public void ResetLegend()
    {
      s_legendBrushes.Clear();
      s_distinctBrushes.ResetBrushes();
    }
    
    /// <summary>
    /// Get the legend use to draw the segments
    /// </summary>
    /// <returns></returns>
    static public IDictionary<Brush, string> GetLegend()
    {
      return s_legendBrushes;
    }
    
    /// <summary>
    /// Return true if a position is contained in the object
    /// </summary>
    /// <param name="dateTime">horizontal position of the mouse</param>
    /// <param name="posY">vertical position of the mouse</param>
    /// <returns></returns>
    public abstract bool Comprises(DateTime dateTime, int posY);
    
    /// <summary>
    /// Try a merge with another barObject
    /// Return true if success (in that case, the merged barObject might be modified)
    /// </summary>
    /// <param name="otherBarObject"></param>
    /// <returns></returns>
    public abstract bool TryMerge(BarObject otherBarObject);
    
    /// <summary>
    /// Paint the object in the bar
    /// </summary>
    /// <param name="g">Graphics which is painted</param>
    public abstract void DrawObject(Graphics g);
    
    /// <summary>
    /// Display a tooltip on an object
    /// </summary>
    /// <param name="toolTip">tooltip which will be displayed</param>
    /// <returns>true if the tooltip is displayed</returns>
    public abstract bool DisplayToolTip(ToolTip toolTip);
    
    /// <summary>
    /// Find the pixel corresponding to a date
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    protected int CoordToPixel(DateTime date)
    {
      double coef = (double)Parent.Width / (double)(Parent.EndDateTime.Subtract(Parent.StartDateTime).Ticks);
      return (int)((double)(date.Subtract(Parent.StartDateTime).Ticks) * coef);
    }
    #endregion // Methods
  }
}
