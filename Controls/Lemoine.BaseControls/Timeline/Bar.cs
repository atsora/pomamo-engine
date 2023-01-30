// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of Bar.
  /// </summary>
  public class Bar : UserControl
  {
    #region Events
    /// <summary>
    /// Event emitted when a segment is double-clicked
    /// The first argument is the segment on which the double click is done
    /// </summary>
    public Action<BarObject> BarObjectDoubleClick;
    
    /// <summary>
    /// Event emitted when the bar is double-clicked
    /// The double click is outside a segment
    /// </summary>
    public Action<DateTime> BarDoubleClick;
    
    /// <summary>
    /// Event emitted when a right click is done on the selected period
    /// The first argument is the control in which the click has been done
    /// The second argument is the position of the click within the control
    /// This will allow you for example to show a context menu at this position
    /// </summary>
    public Action<Control, Point> SelectedPeriodRightClick;
    
    /// <summary>
    /// Event emitted on a mouse wheel
    /// The first argument is the delta (positive if wheeled forward, negative otherwise)
    /// The second argument is the position as a datetime
    /// </summary>
    public Action<int, DateTime> BarWheel;
    
    /// <summary>
    /// Event emitted when the bar is clicked (left button)
    /// The first argument is the position X in pixels
    /// </summary>
    public Action<int> BarMouseDown;
    
    /// <summary>
    /// Event emitted when the mouse moves while the left button is down
    /// The first argument is the position X in pixels
    /// </summary>
    public Action<int> BarMouseMoved;
    #endregion // Events
    
    #region Members
    IBarObjectFactory m_barData;
    Graphics m_graphics;
    IList<BarObject> m_barObjects = new List<BarObject>();
    ToolTip m_tooltip = new ToolTip();
    BarObject m_hoveredBarObject = null;
    DateTime? m_selectedPeriodStart;
    DateTime? m_selectedPeriodEnd;
    bool m_selectedPeriodDefined = false;
    string m_name = "";
    int m_nameLength = 0;
    bool m_leftButtonDown = false;
    #endregion // Members

    static readonly Font TEXT_FONT = new Font("Arial", 8);
    static readonly Brush BACKGROUND_TEXT_BRUSH = new SolidBrush(Color.FromArgb(255, 235, 235, 235));
    static readonly Brush FOREGROUND_TEXT_BRUSH = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
    static readonly Pen BORDER_PEN = new Pen(Color.Gray);
    static readonly Brush BACKGROUND_BRUSH = new SolidBrush(Color.White);
    static readonly Brush SELECTION_BRUSH = new HatchBrush(HatchStyle.OutlinedDiamond,
                                                           Color.FromArgb(255, 170, 170, 170),
                                                           Color.Transparent);
    
    #region Getters / Setters
    /// <summary>
    /// Start date time of the bar
    /// </summary>
    public DateTime StartDateTime { get; private set; }
    
    /// <summary>
    /// End date time of the bar
    /// </summary>
    public DateTime EndDateTime { get; private set; }
    
    /// <summary>
    /// Choose the height of the bar
    /// </summary>
    public int BarHeight {
      get { return this.Height; }
      set {
        this.MaximumSize = new System.Drawing.Size(0, value);
        this.MinimumSize = new System.Drawing.Size(50, value);
        this.Size = new System.Drawing.Size(50, value);
        m_graphics.ResetClip();
      }
    }
    
    /// <summary>
    /// Top margin
    /// </summary>
    public int MarginTop { get; set; }
    
    /// <summary>
    /// Bottom margin
    /// </summary>
    public int MarginBottom { get; set; }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="barData"></param>
    public Bar(string name, IBarObjectFactory barData)
    {
      // Default value
      MarginTop = 8;
      MarginBottom = 1;
      
      this.Margin = new System.Windows.Forms.Padding(0);
      this.MouseLeave += new System.EventHandler(this.BarMouseLeave);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.BarMouseMove);
      this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.BarMouseClick);
      this.MouseDoubleClick += new MouseEventHandler(this.BarMouseDoubleClick);
      this.MouseWheel += new MouseEventHandler(this.BarMouseWheel);
      this.MouseUp += new MouseEventHandler(this.OnMouseUp);
      this.MouseDown += new MouseEventHandler(this.OnMouseDown);
      
      m_tooltip.AutoPopDelay = 30000;
      m_tooltip.IsBalloon = true;
      m_barData = barData;
      Width = 10000; // To make sure the graphics is big enough
      m_graphics = this.CreateGraphics();
      m_graphics.SmoothingMode = SmoothingMode.AntiAlias;
      Width = 0;
      
      m_name = name;
      m_nameLength = (int)m_graphics.MeasureString(m_name, TEXT_FONT).Width;
    }
    #endregion // Constructors

    #region Public methods
    /// <summary>
    /// Draw a specific period
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="endDateTime"></param>
    public void Draw(DateTime startDateTime, DateTime endDateTime)
    {
      IList<BarObject> barObjects = m_barData.CreateBarObjects(startDateTime, endDateTime);
      BarObject previousObject = null;
      m_barObjects.Clear();
      foreach (BarObject barObject in barObjects)
      {
        // Insertion (fusion possible)
        barObject.Parent = this;
        if (previousObject == null) {
          previousObject = barObject;
          m_barObjects.Add(barObject);
        } else if (!previousObject.TryMerge(barObject)) {
          previousObject = barObject;
          m_barObjects.Add(barObject);
        }
      }
      
      StartDateTime = startDateTime;
      EndDateTime = endDateTime;
      this.Invalidate();
    }
    
    /// <summary>
    /// Update selected period
    /// </summary>
    /// <param name="isDisplayed"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void ChangeSelectedPeriod(bool isDisplayed, DateTime? start, DateTime? end)
    {
      m_selectedPeriodStart = start;
      m_selectedPeriodEnd = end;
      m_selectedPeriodDefined = isDisplayed;
      this.Invalidate();
    }
    #endregion // Public methods
    
    #region Private methods
    void DrawBackground()
    {
      m_graphics.FillRectangle(BACKGROUND_BRUSH, 0, MarginTop, Width, Height - MarginBottom - MarginTop - 1);
      m_graphics.DrawLine(BORDER_PEN, 0, MarginTop, Width, MarginTop);
      m_graphics.DrawLine(BORDER_PEN, 0, Height - MarginBottom - 1, Width, Height - MarginBottom - 1);
    }
    
    void DrawObjects()
    {
      if (m_barObjects == null) {
        return;
      }

      foreach (BarObject barObject in m_barObjects) {
        barObject.DrawObject(m_graphics);
      }
    }
    
    void DrawSelection()
    {
      if (m_selectedPeriodDefined) {
        int pos1 = m_selectedPeriodStart.HasValue ? CoordToPixel(m_selectedPeriodStart.Value) : CoordToPixel(StartDateTime);
        int pos2 = m_selectedPeriodEnd.HasValue ? CoordToPixel(m_selectedPeriodEnd.Value) : CoordToPixel(EndDateTime);
        if (pos1 < Width) {
          m_graphics.FillRectangle(SELECTION_BRUSH, pos1, MarginTop - 1, pos2 - pos1, Height - MarginTop - MarginBottom + 1);
        }
      }
    }
    
    void DrawName()
    {
      if (m_name != "") {
        m_graphics.FillRectangle(BACKGROUND_TEXT_BRUSH, -1, 2, m_nameLength - 1, 12);
        m_graphics.DrawRectangle(BORDER_PEN, -1, 2, m_nameLength -1, 12);
        m_graphics.DrawString(m_name, TEXT_FONT, FOREGROUND_TEXT_BRUSH, -1, 1);
      }
    }
    
    int CoordToPixel(DateTime date)
    {
      double coef = (double)Width / (double)(EndDateTime.Subtract(StartDateTime).Ticks);
      return (int)((double)(date.Subtract(StartDateTime).Ticks) * coef);
    }
    
    DateTime PixelToCoord(int posX)
    {
      double coef = (double)(EndDateTime.Subtract(StartDateTime).Ticks) / (double)Width;
      return StartDateTime.Add(new TimeSpan((long)(coef * (double)posX)));
    }
    #endregion // Private methods
    
    #region Event reactions
    /// <summary>
    /// Redefine paint event
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
      DrawBackground();
      DrawObjects();
      DrawSelection();
      DrawName();
    }
    
    void BarMouseMove(object sender, MouseEventArgs e)
    {
      if (m_leftButtonDown) {
        if (BarMouseMoved != null) {
          BarMouseMoved (e.X);
        }
      } else {
        // Find a segment and update the tooltip
        BarObject hoveredObject = GetBarObjectAtLocation(e.Location);
        
        if (hoveredObject != m_hoveredBarObject) {
          if (hoveredObject == null) {
            m_tooltip.Hide(this);
            m_hoveredBarObject = null;
          } else {
            if (hoveredObject.DisplayToolTip(m_tooltip)) {
              m_hoveredBarObject = hoveredObject;
            }
            else {
              m_tooltip.Hide(this);
              m_hoveredBarObject = null;
            }
          }
        }
      }
    }
    
    BarObject GetBarObjectAtLocation(Point location)
    {
      DateTime currentDateTime = PixelToCoord(location.X);
      int posY = location.Y;
      
      // Find the segment with the highest priority
      BarObject hoveredObject = null;
      int maxPriority = -1;
      foreach (BarObject barObject in m_barObjects) {
        if (barObject.Comprises(currentDateTime, location.Y) && barObject.ToolTipPriority >= maxPriority) {
          maxPriority = barObject.ToolTipPriority;
          hoveredObject = barObject;
        }
      }
      
      return hoveredObject;
    }
    
    void BarMouseLeave(object sender, EventArgs e)
    {
      m_tooltip.Hide(this);
      m_hoveredBarObject = null;
    }
    
    void BarMouseClick(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Right && m_selectedPeriodDefined && SelectedPeriodRightClick != null) {
        DateTime currentDateTime = PixelToCoord(e.X);
        if (m_selectedPeriodStart <= currentDateTime &&
            (!m_selectedPeriodEnd.HasValue || m_selectedPeriodEnd.Value >= currentDateTime)) {
          SelectedPeriodRightClick (this, e.Location);
        }
      }
    }
    
    void BarMouseDoubleClick(object sender, MouseEventArgs e)
    {
      if (e.Button != MouseButtons.Left) {
        return;
      }

      BarObject barObject = GetBarObjectAtLocation(e.Location);
      if (barObject != null) {
        if (BarObjectDoubleClick != null) {
          BarObjectDoubleClick (barObject);
        }
      } else {
        if (BarDoubleClick != null) {
          BarDoubleClick (PixelToCoord(e.X));
        }
      }
    }
    
    void BarMouseWheel(object sender, MouseEventArgs e)
    {
      if (BarWheel != null) {
        BarWheel (e.Delta, PixelToCoord(e.X));
      }
    }
    
    void OnMouseUp(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left) {
        m_leftButtonDown = false;
      }

      this.Cursor = Cursors.Arrow;
    }
    
    void OnMouseDown(object sender, MouseEventArgs e)
    {
      if (e.Button == MouseButtons.Left) {
        m_leftButtonDown = true;
        if (BarMouseDown != null) {
          BarMouseDown (e.X);
        }

        this.Cursor = Cursors.SizeWE;
      }
    }
    #endregion // Event reactions
  }
}
