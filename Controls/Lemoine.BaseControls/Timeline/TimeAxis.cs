// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// Description of TimeAxis.
  /// </summary>
  public class TimeAxis : UserControl
  {
    #region Members
    DateTime m_startDateTime;
    DateTime m_endDateTime;
    bool m_initialized = false;
    Graphics m_graphics;
    int m_labelWidth;
    int m_margin = 10;
    IList<DateTime> m_ticks = new List<DateTime>();
    bool m_withSecond = false;
    bool m_withYear = false;
    int m_subDivisions = 0;
    #endregion // Members

    static Brush s_backBrush = new SolidBrush(SystemColors.Control);
    static Brush s_foreBrush = new SolidBrush(Color.Black);
    static Font s_font = new Font("Arial", 8);
    static Font s_fontBold = new Font("Arial Black", 8);
    static Pen s_tickPen = new Pen(Color.Black, 1);
    static Pen s_subTickPen = new Pen(Color.Gray, 1);

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TimeAxis()
    {
      this.Margin = new System.Windows.Forms.Padding(0);
      this.SizeChanged += new System.EventHandler(this.TimeAxisSizeChanged);
      
      Width = 10000; // To make sure the graphics is big enough
      Height = 50;
      m_graphics = CreateGraphics();
      Width = 0;
      Height = 0;
      m_graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
      m_labelWidth = (int)m_graphics.MeasureString("XX/XX/XX", s_fontBold).Width;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Set the period of the axis
    /// </summary>
    /// <param name="startDateTime"></param>
    /// <param name="endDateTime"></param>
    public void SetPeriod(DateTime startDateTime, DateTime endDateTime)
    {
      m_startDateTime = startDateTime;
      m_endDateTime = endDateTime;
      m_initialized = true;
      ComputeTicks();
      Invalidate();
    }
    
    int CoordToPixel(DateTime date)
    {
      double coef = (double)Width / (double)(m_endDateTime.Subtract(m_startDateTime).Ticks);
      return (int)((double)(date.Subtract(m_startDateTime).Ticks) * coef);
    }
    
    void ComputeTicks()
    {
      // Number of labels max
      int maxLabelsNb = Width / (m_labelWidth + m_margin);
      if (maxLabelsNb == 0) {
        maxLabelsNb = 1;
      }

      // Time range per label
      TimeSpan timeRangePerLabel = new TimeSpan(m_endDateTime.Subtract(m_startDateTime).Ticks / maxLabelsNb);

      m_ticks.Clear();
      double nbDays = timeRangePerLabel.TotalDays;
      if (nbDays > 365 * 10) {
        // Every 10 years
        ComputeAnnualTicks(10);
        m_subDivisions = 5;
      } else if (nbDays > 365 * 2) {
        // Every 5 years
        ComputeAnnualTicks(5);
        m_subDivisions = 5;
      } else if (nbDays > 365) {
        // Every 2 years
        ComputeAnnualTicks(2);
        m_subDivisions = 4;
      } else if (nbDays > 6 * 30) {
        // Every year
        ComputeAnnualTicks(1);
        m_subDivisions = 6;
      } else if (nbDays > 4 * 30) {
        // Every 6 months
        ComputeMonthlyTicks(6);
        m_subDivisions = 6;
      } else if (nbDays > 3 * 30) {
        // Every 4 months
        ComputeMonthlyTicks(4);
        m_subDivisions = 4;
      } else if (nbDays > 2 * 30) {
        // Every 3 months
        ComputeMonthlyTicks(3);
        m_subDivisions = 6;
      } else if (nbDays > 30) {
        // Every 2 months
        ComputeMonthlyTicks(2);
        m_subDivisions = 4;
      } else if (nbDays > 14) {
        // Every month
        ComputeMonthlyTicks(1);
        m_subDivisions = 4;
      } else if (nbDays > 7) {
        // Every 2 weeks
        ComputeWeeklyTicks(2);
        m_subDivisions = 7;
      } else if (nbDays > 2) {
        // Every week
        ComputeWeeklyTicks(1);
        m_subDivisions = 7;
      } else if (nbDays > 1) {
        // Every 2 days
        ComputeDailyTicks(2);
        m_subDivisions = 4;
      } else {
        double seconds = timeRangePerLabel.TotalSeconds;
        if (seconds > 60 * 60 * 12) {
          // Every day
          ComputeDailyTicks(1);
          m_subDivisions = 4;
        } else if (seconds > 60 * 60 * 6) {
          // Every 12 hours
          ComputeHourlyTicks(12);
          m_subDivisions = 6;
        } else if (seconds > 60 * 60 * 4) {
          // Every 6 hours
          ComputeHourlyTicks(6);
          m_subDivisions = 6;
        } else if (seconds > 60 * 60 * 3) {
          // Every 4 hours
          ComputeHourlyTicks(4);
          m_subDivisions = 4;
        } else if (seconds > 60 * 60 * 2) {
          // Every 3 hours
          ComputeHourlyTicks(3);
          m_subDivisions = 6;
        } else if (seconds > 60 * 60) {
          // Every 2 hours
          ComputeHourlyTicks(2);
          m_subDivisions = 6;
        } else if (seconds > 60 * 30) {
          // Every hour
          ComputeHourlyTicks(1);
          m_subDivisions = 6;
        } else if (seconds > 60 * 20) {
          // Every 30 minutes
          ComputeMinuteTicks(30);
          m_subDivisions = 6;
        } else if (seconds > 60 * 10) {
          // Every 20 minutes
          ComputeMinuteTicks(20);
          m_subDivisions = 4;
        } else if (seconds > 60 * 5) {
          // Every 10 minutes
          ComputeMinuteTicks(10);
          m_subDivisions = 5;
        } else if (seconds > 60 * 2) {
          // Every 5 minutes
          ComputeMinuteTicks(5);
          m_subDivisions = 5;
        } else if (seconds > 60) {
          // Every 2 minutes
          ComputeMinuteTicks(2);
          m_subDivisions = 6;
        } else if (seconds > 30) {
          // Every minute
          ComputeMinuteTicks(1);
          m_subDivisions = 6;
        } else if (seconds > 20) {
          // Every 30 seconds
          ComputeSecondTicks(30);
          m_subDivisions = 6;
        } else if (seconds > 10) {
          // Every 20 seconds
          ComputeSecondTicks(20);
          m_subDivisions = 4;
        } else if (seconds > 5) {
          // Every 10 seconds
          ComputeSecondTicks(10);
          m_subDivisions = 5;
        } else if (seconds > 2) {
          // Every 5 seconds
          ComputeSecondTicks(5);
          m_subDivisions = 5;
        } else if (seconds > 1) {
          // Every 2 seconds
          ComputeSecondTicks(2);
          m_subDivisions = 4;
        } else {
          // Every second
          ComputeSecondTicks(1);
          m_subDivisions = 5;
        }
      }
      
      // Display year or second?
      m_withYear = false;
      m_withSecond = false;
      if (m_ticks.Count > 0) {
        DateTime firstTick = m_ticks[0];
        foreach (DateTime tick in m_ticks) {
          m_withYear |= (firstTick.Year != tick.Year);
          m_withSecond |= (tick.Second != 0);
        }
      }
    }
    
    void ComputeAnnualTicks(int shift)
    {
      DateTime start = m_startDateTime.Date;
      start = start.AddDays(1 - start.Day).AddMonths(1 - start.Month);
      
      m_ticks.Add(start.AddYears(-shift));
      while (m_ticks[m_ticks.Count - 1] < m_endDateTime) {
        m_ticks.Add(m_ticks[m_ticks.Count - 1].AddYears(shift));
      }
    }
    
    void ComputeMonthlyTicks(int shift)
    {
      DateTime start = m_startDateTime.Date;
      start = start.AddDays(1 - start.Day);
      
      m_ticks.Add(start.AddMonths(-shift));
      while (m_ticks[m_ticks.Count - 1] < m_endDateTime) {
        m_ticks.Add(m_ticks[m_ticks.Count - 1].AddMonths(shift));
      }
    }
    
    void ComputeWeeklyTicks(int shift)
    {
      DateTime start = m_startDateTime.Date;
      int dayOfWeek = (start.DayOfWeek == DayOfWeek.Sunday) ? 6 : (int)start.DayOfWeek - 1;
      start = start.AddDays(-dayOfWeek);
      
      m_ticks.Add(start.AddDays(-7 * shift));
      while (m_ticks[m_ticks.Count - 1] < m_endDateTime) {
        m_ticks.Add(m_ticks[m_ticks.Count - 1].AddDays(7 * shift));
      }
    }
    
    void ComputeDailyTicks(int shift)
    {
      DateTime start = m_startDateTime.Date;
      
      m_ticks.Add(start.AddDays(-shift));
      while (m_ticks[m_ticks.Count - 1] < m_endDateTime) {
        m_ticks.Add(m_ticks[m_ticks.Count - 1].AddDays(shift));
      }
    }
    
    void ComputeHourlyTicks(int shift)
    {
      DateTime start = m_startDateTime.Date;
      
      m_ticks.Add(start.AddHours(-shift));
      while (m_ticks[m_ticks.Count - 1] < m_endDateTime) {
        m_ticks.Add(m_ticks[m_ticks.Count - 1].AddHours(shift));
      }
    }
    
    void ComputeMinuteTicks(int shift)
    {
      DateTime start = m_startDateTime.Date.AddHours(m_startDateTime.Hour);
      
      m_ticks.Add(start.AddMinutes(-shift));
      while (m_ticks[m_ticks.Count - 1] < m_endDateTime) {
        m_ticks.Add(m_ticks[m_ticks.Count - 1].AddMinutes(shift));
      }
    }
    
    void ComputeSecondTicks(int shift)
    {
      DateTime start = m_startDateTime.Date.AddHours(m_startDateTime.Hour).AddMinutes(m_startDateTime.Minute);
      
      m_ticks.Add(start.AddSeconds(-shift));
      while (m_ticks[m_ticks.Count - 1] < m_endDateTime) {
        m_ticks.Add(m_ticks[m_ticks.Count - 1].AddSeconds(shift));
      }
    }
    
    #endregion // Methods
    
    #region Event reactions
    void TimeAxisSizeChanged(object sender, EventArgs e)
    {
      using (new SuspendDrawing(this)) {
        if (m_initialized) {
          ComputeTicks ();
        }

        Invalidate ();
      }
    }
    
    /// <summary>
    /// Redefine paint method
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaint(PaintEventArgs e)
    {
      StringFormat format = new StringFormat();
      format.Alignment = StringAlignment.Center;
      DateTime? previousTick = null;
      foreach (DateTime tick in m_ticks) {
        int posX = CoordToPixel(tick);
        if (m_withYear) {
          m_graphics.DrawString(tick.ToString("d"), s_fontBold, s_foreBrush, posX, -1, format);
        }
        else {
          m_graphics.DrawString(tick.ToString("m"), s_fontBold, s_foreBrush, posX, -1, format);
        }

        if (m_withSecond) {
          m_graphics.DrawString(tick.ToString("T"), s_font, s_foreBrush, posX, Height / 2 - 1, format);
        }
        else {
          m_graphics.DrawString(tick.ToString("t"), s_font, s_foreBrush, posX, Height / 2 - 1, format);
        }

        m_graphics.DrawLine(s_tickPen, posX, Height, posX, Height - 4);
        
        // Subdivisions
        if (previousTick.HasValue && m_subDivisions > 1) {
          long subDiv = tick.Subtract(previousTick.Value).Ticks / m_subDivisions;
          for (int i = 1; i < m_subDivisions; i++) {
            posX = CoordToPixel(previousTick.Value.AddTicks(subDiv * i));
            m_graphics.DrawLine(s_subTickPen, posX, Height, posX, Height - 3);
          }
        }
        previousTick = tick;
      }
    }
    #endregion // Event reactions
  }
}
