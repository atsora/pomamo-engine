// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of TimelinesWidget.
  /// </summary>
  public partial class TimelinesWidget : UserControl
  {
    #region Events
    /// <summary>
    /// Event emitted when period changes
    /// </summary>
    public Action<DateTime, DateTime> PeriodChanged;
    
    /// <summary>
    /// Event emitted when the selected period changes
    /// </summary>
    public Action<DateTime?, DateTime?> SelectedPeriodChanged;
    #endregion // Events
    
    #region Members
    IList<Bar> m_bars = new List<Bar>();
    bool m_dateInitialized = false;
    bool m_noBorders = false;
    #endregion // Members
    
    #region Getters / Setters
    /// <summary>
    /// Left limit of the timelines
    /// </summary>
    public DateTime StartDateTime { get; private set; }
    
    /// <summary>
    /// Right limit of the timelines
    /// </summary>
    public DateTime EndDateTime { get; private set; }
    
    /// <summary>
    /// Left limit of the selected period
    /// </summary>
    public DateTime? SelectedPeriodStart { get; private set; }
    
    /// <summary>
    /// Right limit of the selected period (can be null if infinite)
    /// </summary>
    public DateTime? SelectedPeriodEnd { get; private set; }
    
    /// <summary>
    /// Height of the bars
    /// </summary>
    public int BarHeight { get; set; }
    
    /// <summary>
    /// Add or remove the borders
    /// </summary>
    public bool NoBorders {
      get { return m_noBorders; }
      set {
        if (value) {
          baseLayout.RowStyles[1].Height = 1;
          baseLayout.RowStyles[3].Height = 1;
          horizontalBar1.Show();
          horizontalBar2.Show();
        } else {
          baseLayout.RowStyles[1].Height = 0;
          baseLayout.RowStyles[3].Height = 0;
          horizontalBar1.Hide();
          horizontalBar2.Hide();
        }
        m_noBorders = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TimelinesWidget()
    {
      BarHeight = 32;
      InitializeComponent();
      verticalScrollLayout.ScrollbarVisibilityChanged += UpdateTimeAxisRange;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Clear all timelines
    /// </summary>
    public void Clear()
    {
      m_bars.Clear();
      verticalScrollLayout.Clear();
    }
    
    /// <summary>
    /// Add a timeline
    /// </summary>
    /// <param name="name"></param>
    /// <param name="barData"></param>
    public void AddTimeline(string name, IBarObjectFactory barData)
    {
      Bar bar = new Bar(name, barData);
      if (m_noBorders) {
        bar.MarginBottom = 0;
        bar.MarginTop = 0;
      }
      bar.BarObjectDoubleClick += OnBarObjectDoubleClick;
      bar.BarDoubleClick += OnBarDoubleClick;
      bar.BarWheel += OnMouseWheel;
      bar.BarMouseDown += OnMouseDown;
      bar.BarMouseMoved += OnMouseMove;
      bar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
      bar.BarHeight = BarHeight;
      m_bars.Add(bar);
      verticalScrollLayout.AddControl(bar);
    }
    
    /// <summary>
    /// Set the period
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="endDateTime"></param>
    public void SetPeriod(DateTime startDateTime, DateTime endDateTime)
    {
      // Period min: 10 seconds, max: 20 years
      long period = endDateTime.Subtract(startDateTime).Ticks;
      long maxPeriod = (new TimeSpan(365 * 20, 0, 0, 0)).Ticks;
      long minPeriod = (new TimeSpan(0, 0, 10)).Ticks;
      if (period > maxPeriod) {
        TimeSpan offset = new TimeSpan((period - maxPeriod) / 2);
        startDateTime = startDateTime.Add(offset);
        endDateTime = endDateTime.Subtract(offset);
      } else if (period < minPeriod) {
        TimeSpan offset = new TimeSpan((minPeriod - period) / 2);
        startDateTime = startDateTime.Subtract(offset);
        endDateTime = endDateTime.Add(offset);
      }
      
      // Date min: 01/01/1980, max: 01/01/2200
      DateTime minDateTime = new DateTime(1980, 01, 01);
      DateTime maxDateTime = new DateTime(2200, 01, 01);
      if (startDateTime < minDateTime) {
        endDateTime = minDateTime.Add(endDateTime.Subtract(startDateTime));
        startDateTime = minDateTime;
      } else if (endDateTime > maxDateTime) {
        minDateTime = maxDateTime.Add(startDateTime.Subtract(endDateTime));
        endDateTime = maxDateTime;
      }
      
      StartDateTime = startDateTime;
      EndDateTime = endDateTime;
      m_dateInitialized = true;
      buttonChangePeriod.Text = startDateTime.ToString("d") + " " + startDateTime.ToString("T") +
        " - " + endDateTime.ToString("d") + " " + endDateTime.ToString("T");
      UpdateTimeAxisRange(verticalScrollLayout.ScrollbarVisible);
      Draw();
      if (PeriodChanged != null) {
        PeriodChanged (StartDateTime, EndDateTime);
      }
    }
    
    /// <summary>
    /// Set the selected period
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SetSelectedPeriod(DateTime? start, DateTime? end)
    {
      SetSelectedPeriod(true, start, end);
    }
    
    /// <summary>
    /// Draw the timelines
    /// </summary>
    public void Draw()
    {
      foreach (Bar bar in m_bars) {
        bar.Draw(StartDateTime, EndDateTime);
      }
    }
    
    void SetSelectedPeriod(bool isDisplayed, DateTime? start, DateTime? end)
    {
      SelectedPeriodStart = start;
      SelectedPeriodEnd = end;
      foreach (Bar bar in m_bars) {
        bar.ChangeSelectedPeriod(isDisplayed, start, end);
      }

      if (SelectedPeriodChanged != null) {
        SelectedPeriodChanged (start, end);
      }
    }
    
    void UpdateTimeAxisRange(bool withScrollbar)
    {
      if (!m_dateInitialized) {
        return;
      }

      DateTime endDateTime;
      if (withScrollbar) {
        double coef = (double)(verticalScrollLayout.Width) /
          (double)(verticalScrollLayout.Width - verticalScrollLayout.ScrollBarWidth);
        endDateTime = new DateTime((long)((double)EndDateTime.Subtract(StartDateTime).Ticks * coef) +
                                   StartDateTime.Ticks);
      } else {
        endDateTime = EndDateTime;
      }

      timeAxis.SetPeriod(StartDateTime, endDateTime);
    }
    #endregion // Methods
    
    #region Event reactions
    void ButtonLeftClick(object sender, EventArgs e)
    {
      long offset = -EndDateTime.Subtract(StartDateTime).Ticks / 2;
      SetPeriod(StartDateTime.AddTicks(offset), EndDateTime.AddTicks(offset));
    }
    
    void ButtonChangePeriodClick(object sender, EventArgs e)
    {
      ChangePeriodDialog dialog = new ChangePeriodDialog(StartDateTime, EndDateTime);
      if (dialog.ShowDialog() == DialogResult.OK) {
        SetPeriod (dialog.StartDateTime, dialog.EndDateTime);
      }

      dialog.Dispose();
    }
    
    void ButtonRightClick(object sender, EventArgs e)
    {
      long offset = EndDateTime.Subtract(StartDateTime).Ticks / 2;
      SetPeriod(StartDateTime.AddTicks(offset), EndDateTime.AddTicks(offset));
    }
    
    void OnBarObjectDoubleClick(BarObject barObject)
    {
      BarSegment segment = barObject as BarSegment;
      if (segment != null) {
        SetSelectedPeriod (true, segment.Start, segment.End);
      }
    }
    
    void OnBarDoubleClick(DateTime dateTime)
    {
      // Find a selection around dateTime
      TimeSpan offset = new TimeSpan(EndDateTime.Subtract(StartDateTime).Ticks / 10);
      DateTime start = dateTime.Subtract(offset);
      DateTime end = dateTime.Add(offset);
      
      // Round the limits
      int limit = 10;
      if (offset.TotalMilliseconds > limit) {
        start = start.AddMilliseconds(-start.Millisecond);
        end = end.AddMilliseconds(-end.Millisecond);
      }
      if (offset.TotalSeconds > limit) {
        start = start.AddSeconds(-start.Second);
        end = end.AddSeconds(-end.Second);
      }
      if (offset.TotalMinutes > 5) {
        start = start.AddMinutes(-start.Minute);
        end = end.AddMinutes(-end.Minute);
      }
      if (offset.TotalHours > 5) {
        start = start.AddHours(-start.Hour);
        end = end.AddHours(-end.Hour);
      }
      if (offset.TotalDays > 5) {
        start = start.AddDays(-start.Day);
        end = start.AddDays(-end.Day);
      }
      
      SetSelectedPeriod(true, start, end);
    }
    
    void OnMouseWheel(int delta, DateTime dateTime)
    {
      if ((ModifierKeys & Keys.Control) == Keys.Control)
      {
        // Offset to allocate
        long offset;
        long fullLength = EndDateTime.Subtract(StartDateTime).Ticks;
        if (delta > 0) {
          offset = -fullLength / 2;
        }
        else {
          offset = fullLength;
        }

        // Relative position of the mouse
        double pos = (double)dateTime.Subtract(StartDateTime).Ticks / (double)fullLength;
        if (pos < 0) {
          pos = 0;
        }
        else if (pos > 1) {
          pos = 1;
        }

        SetPeriod (StartDateTime.AddTicks((long)(-pos * offset)),
                  EndDateTime.AddTicks((long)((1.0 - pos) * offset)));
      }
    }
    
    int m_initPosX = 0;
    DateTime m_initFirstDateTime;
    DateTime m_initEndDateTime;
    void OnMouseDown(int posX)
    {
      m_initPosX = posX;
      m_initFirstDateTime = StartDateTime;
      m_initEndDateTime = EndDateTime;
    }
    
    void OnMouseMove(int posX)
    {
      // Offset
      long ticks = (m_initPosX - posX) * EndDateTime.Subtract(StartDateTime).Ticks / this.Width;
      SetPeriod(m_initFirstDateTime.AddTicks(ticks), m_initEndDateTime.AddTicks(ticks));
    }
    
    void ButtonZoomOutClick(object sender, EventArgs e)
    {
      long offset = EndDateTime.Subtract(StartDateTime).Ticks / 2;
      SetPeriod(StartDateTime.AddTicks(-offset), EndDateTime.AddTicks(offset));
    }
    
    void ButtonZoomInClick(object sender, EventArgs e)
    {
      long offset = EndDateTime.Subtract(StartDateTime).Ticks / 4;
      SetPeriod(StartDateTime.AddTicks(offset), EndDateTime.AddTicks(-offset));
    }
    #endregion // Event reactions
  }
}
