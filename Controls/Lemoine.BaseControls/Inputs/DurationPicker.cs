// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using Lemoine.Core.Log;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// NumericUpDown for editing a timespan with a DDD.HH:MM:SS.zzz format
  /// DDD. is displayed only if HH > 23
  /// .zzz is displayed only if its value is > 0
  /// </summary>
  public partial class DurationPicker : NumericUpDown
  {
    #region Enums
    private enum SelectionType
    {
      Day,
      Hour,
      Minute,
      Second,
      Millisecond,
      Invalid
    };
    #endregion Enums

    #region Members
    TextBox m_textBox = null; // convenient handle
    SelectionType m_currentSelectionType = SelectionType.Invalid;
    bool m_startEditBlockFlag = true;
    ToolTip m_toolTip = new ToolTip ();
    bool m_withMs = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (DurationPicker).FullName);

    #region Getters / Setters
    /// <summary>
    /// Value as timespan
    /// </summary>
    public TimeSpan TimeSpanValue
    {
      get { return TimeSpan.FromMilliseconds ((double)this.Value); }
      set {
        // Check the maximum
        var newValue = value;
        if (newValue.TotalMilliseconds > (double)this.Maximum) {
          newValue = TimeSpan.FromMilliseconds ((double)this.Maximum);
        }
        else if (newValue.TotalMilliseconds < (double)this.Minimum) {
          newValue = TimeSpan.FromMilliseconds ((double)this.Minimum);
        }

        this.Value = (decimal)newValue.TotalMilliseconds;
        this.OnValueChanged (EventArgs.Empty);
        UpdateEditText ();
      }
    }

    /// <summary>
    /// Same than TimeSpanValue
    /// </summary>
    public TimeSpan Duration
    {
      get { return TimeSpanValue; }
      set { TimeSpanValue = value; }
    }

    /// <summary>
    /// Allow the use of milliseconds, true by default
    /// </summary>
    public bool WithMs
    {
      get { return m_withMs; }
      set
      {
        m_withMs = value;
        m_toolTip.SetToolTip (this, value ?
          "Format: {DDD}d {HH}:{mm}:{ss} {zzz}ms\n'{DDD}' and '{zzz}' are optional" :
          "Format: {DDD}d {HH}:{mm}:{ss}\n'{DDD}' is optional");
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DurationPicker () : base ()
    {
      // Default values
      Maximum = 86399999999m; // in ms: 999 days, 23h 59m 59s 999ms (999 * 24 * 60 * 60 * 1000 - 1)
      Minimum = 0m;
      WithMs = true; // Tooltip also initialized

      // Handle to the editable text
      m_textBox = this.Controls[1] as TextBox;
      
      // Acceleration of the modification when holding a button
      this.Accelerations.Add (new NumericUpDownAcceleration (2, 10));
      this.Accelerations.Add (new NumericUpDownAcceleration (4, 60));
      this.Accelerations.Add (new NumericUpDownAcceleration (12, 3600));

      // Events
      this.KeyPress += new KeyPressEventHandler (this.OnKeyPress);
      this.KeyDown += new KeyEventHandler (this.OnKeyDown);
      this.MouseDown += new MouseEventHandler (this.OnMouseDown);
      this.MouseUp += new MouseEventHandler (this.OnMouseUp);
    }

    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Format content of textbox
    /// </summary>
    protected override void UpdateEditText ()
    {
      var currentTimeSpan = TimeSpanValue;
      if (currentTimeSpan.Milliseconds == 0 && m_currentSelectionType != SelectionType.Millisecond || !WithMs) {
        if (currentTimeSpan.Days == 0 && m_currentSelectionType != SelectionType.Day) {
          // No ms, no days
          this.Text = string.Format ("{0}:{1:00}:{2:00}",
            currentTimeSpan.Hours,
            currentTimeSpan.Minutes,
            currentTimeSpan.Seconds);
        } else {
          // No ms but days
          this.Text = string.Format ("{0}d {1:00}:{2:00}:{3:00}",
            currentTimeSpan.Days,
            currentTimeSpan.Hours,
            currentTimeSpan.Minutes,
            currentTimeSpan.Seconds);
        }
      } else {
        if (currentTimeSpan.Days == 0 && m_currentSelectionType != SelectionType.Day) {
          // No days but ms
          this.Text = string.Format ("{0}:{1:00}:{2:00} {3:000}ms",
            currentTimeSpan.Hours,
            currentTimeSpan.Minutes,
            currentTimeSpan.Seconds,
            currentTimeSpan.Milliseconds);
        } else {
          // Both ms and days
          this.Text = string.Format ("{0}d {1:00}:{2:00}:{3:00} {4:000}ms",
            currentTimeSpan.Days,
            currentTimeSpan.Hours,
            currentTimeSpan.Minutes,
            currentTimeSpan.Seconds,
            currentTimeSpan.Milliseconds);
        }
      }

      // Restore the selection
      if (m_textBox != null) {
        m_textBox.SelectionStart = GetStart (m_currentSelectionType);
        m_textBox.SelectionLength = GetLength (m_currentSelectionType);
      }
    }

    /// <summary>
    /// Update value on down arrow
    /// </summary>
    public override void DownButton ()
    {
      // Compute the next value
      Decimal currentValue = this.Value; // milliseconds
      Decimal nextValue = currentValue;
      switch (m_currentSelectionType) {
        case SelectionType.Day:
          nextValue = Decimal.Subtract (currentValue, new Decimal (24 * 3600 * 1000));
          break;
        case SelectionType.Hour:
          nextValue = Decimal.Subtract (currentValue, new Decimal (3600 * 1000));
          break;
        case SelectionType.Minute:
          nextValue = Decimal.Subtract (currentValue, new Decimal (60 * 1000));
          break;
        case SelectionType.Second:
          nextValue = Decimal.Subtract (currentValue, new Decimal (1000));
          break;
        case SelectionType.Millisecond:
          nextValue = Decimal.Subtract (currentValue, new Decimal (1));
          break;
        case SelectionType.Invalid:
          // Do nothing
          break;
      }

      if (nextValue.CompareTo (currentValue) != 0) {
        TimeSpanValue = TimeSpan.FromMilliseconds ((double)nextValue);
      }
    }

    /// <summary>
    /// Update value on up arrow
    /// </summary>
    public override void UpButton ()
    {
      // Compute the next value
      Decimal currentValue = this.Value; // milliseconds
      Decimal nextValue = currentValue;
      switch (m_currentSelectionType) {
        case SelectionType.Day:
          nextValue = Decimal.Add (currentValue, new Decimal (24 * 3600 * 1000));
          break;
        case SelectionType.Hour:
          nextValue = Decimal.Add (currentValue, new Decimal (3600 * 1000));
          break;
        case SelectionType.Minute:
          nextValue = Decimal.Add (currentValue, new Decimal (60 * 1000));
          break;
        case SelectionType.Second:
          nextValue = Decimal.Add (currentValue, new Decimal (1000));
          break;
        case SelectionType.Millisecond:
          nextValue = Decimal.Add (currentValue, new Decimal (1));
          break;
        case SelectionType.Invalid:
          // Do nothing
          break;
      }

      if (nextValue.CompareTo (currentValue) != 0) {
        TimeSpanValue = TimeSpan.FromMilliseconds ((double)nextValue);
      }
    }

    int GetStart (SelectionType selection)
    {
      // Number of characters for the days
      int dayLength = 0;
      var currentTimeSpanValue = TimeSpanValue;
      if (currentTimeSpanValue.Days > 99) {
        dayLength = 3;
      }
      else if (currentTimeSpanValue.Days > 9) {
        dayLength = 2;
      }
      else if (currentTimeSpanValue.Days > 0 || m_currentSelectionType == SelectionType.Day) {
        dayLength = 1;
      }

      // Number of characters for the hours
      int hourLength = (currentTimeSpanValue.Hours > 9 || dayLength != 0) ? 2 : 1;

      int start = 0;
      switch (selection) {
        case SelectionType.Day:
          start = 0;
          break;
        case SelectionType.Hour:
          start = dayLength != 0 ? dayLength + 2 : 0;
          break;
        case SelectionType.Minute:
          start = (dayLength != 0 ? dayLength + 2 : 0) + hourLength + 1;
          break;
        case SelectionType.Second:
          start = (dayLength != 0 ? dayLength + 2 : 0) + hourLength + 4;
          break;
        case SelectionType.Millisecond:
          start = (dayLength != 0 ? dayLength + 2 : 0) + hourLength + 7;
          break;
        case SelectionType.Invalid:
          start = 0;
          break;
      }

      return start;
    }

    int GetLength (SelectionType selection)
    {
      int length = 0;

      var currentTimeSpanValue = TimeSpanValue;
      switch (selection) {
        case SelectionType.Day:
          if (currentTimeSpanValue.Days > 99) {
          length = 3;
        }
        else if (currentTimeSpanValue.Days > 9) {
          length = 2;
        }
        else if (currentTimeSpanValue.Days > 0) {
          length = 1;
        }
        else {
          length = 0;
        }

        break;
        case SelectionType.Hour:
          length = (currentTimeSpanValue.Hours > 9 || currentTimeSpanValue.Days > 0) ? 2 : 1;
          break;
        case SelectionType.Minute:
          length = 2;
          break;
        case SelectionType.Second:
          length = 2;
          break;
        case SelectionType.Millisecond:
          length = 3;
          break;
        case SelectionType.Invalid:
          length = 0;
          break;
      }

      return length;
    }
    #endregion // Methods

    #region Event reactions
    void OnMouseDown (object sender, MouseEventArgs e)
    {
      int selectionStart = this.m_textBox.SelectionStart;
      Control childAtMouse = GetChildAtPoint (new Point (e.X, e.Y));

      if (childAtMouse == m_textBox) {
        switch (e.Button) {
          case MouseButtons.Left:
            // Update the selection type
            if (selectionStart < 0) {
            m_currentSelectionType = SelectionType.Invalid;
          }
          else if (selectionStart < GetStart (SelectionType.Hour)) {
            m_currentSelectionType = SelectionType.Day;
          }
          else if (selectionStart < GetStart (SelectionType.Minute)) {
            m_currentSelectionType = SelectionType.Hour;
          }
          else if (selectionStart < GetStart (SelectionType.Second)) {
            m_currentSelectionType = SelectionType.Minute;
          }
          else if (selectionStart < GetStart (SelectionType.Millisecond) || !WithMs) {
            m_currentSelectionType = SelectionType.Second;
          }
          else {
            m_currentSelectionType = SelectionType.Millisecond;
          }

          // Refresh the display (days may have disappeared)
          // The selection is restored
          UpdateEditText ();
            break;
          default:
            break;
        }
      }

      if (m_textBox.SelectionLength > 3) {
        m_textBox.SelectionStart = GetStart (SelectionType.Second);
        m_textBox.SelectionLength = GetLength (SelectionType.Second);
      }

      // Can start a new edit for the current block
      m_startEditBlockFlag = true;
    }

    void OnMouseUp (object sender, MouseEventArgs e)
    {
      // Same than down
      OnMouseDown (sender, e);
    }

    void OnKeyDown (object sender, KeyEventArgs e)
    {
      // get rid of del / back keys
      if ((e.KeyCode == Keys.Back) || (e.KeyCode == Keys.Delete)) {
        e.SuppressKeyPress = true;
        e.Handled = true;
      } else if (e.KeyCode == Keys.Left) {
        // Select the block on the left
        switch (m_currentSelectionType) {
          case SelectionType.Day:
            m_currentSelectionType = SelectionType.Day; // The same
            break;
          case SelectionType.Hour:
            m_currentSelectionType = SelectionType.Day;
            break;
          case SelectionType.Minute:
            m_currentSelectionType = SelectionType.Hour;
            break;
          case SelectionType.Second:
            m_currentSelectionType = SelectionType.Minute;
            break;
          case SelectionType.Millisecond:
            m_currentSelectionType = SelectionType.Second;
            break;
          case SelectionType.Invalid:
            m_currentSelectionType = SelectionType.Day; // Go to the left
            break;
        }
        m_startEditBlockFlag = true;
        e.SuppressKeyPress = true;
        e.Handled = true;
        UpdateEditText ();
      } else if (e.KeyCode == Keys.Right) {
        // Select the block on the right
        switch (m_currentSelectionType) {
          case SelectionType.Day:
            m_currentSelectionType = SelectionType.Hour;
            break;
          case SelectionType.Hour:
            m_currentSelectionType = SelectionType.Minute;
            break;
          case SelectionType.Minute:
            m_currentSelectionType = SelectionType.Second;
            break;
          case SelectionType.Second:
            if (WithMs) {
            m_currentSelectionType = SelectionType.Millisecond;
          }

          break;
          case SelectionType.Millisecond:
            m_currentSelectionType = SelectionType.Millisecond; // The same
            break;
          default:
            m_currentSelectionType = (TimeSpanValue.Milliseconds == 0 || !WithMs) ? // Go to the right
              SelectionType.Second : SelectionType.Millisecond;
            break;
        }
        m_startEditBlockFlag = true;
        e.SuppressKeyPress = true;
        e.Handled = true;
        UpdateEditText ();
      }
    }

    void OnKeyPress (object sender, KeyPressEventArgs e)
    {
      // Everything is handled
      e.Handled = true;

      // Key that is pressed, return if not between '0' and '9'
      char c = e.KeyChar;
      if (c < '0' || c > '9') {
        return;
      }

      // Current timespan
      TimeSpan currentTimeSpan = TimeSpanValue;

      // Compute the new timespan
      TimeSpan newTimeSpan = currentTimeSpan;
      int newValue = 0;
      switch (m_currentSelectionType) {
        case SelectionType.Day:
          newValue = m_startEditBlockFlag ?
            c - '0' :
            currentTimeSpan.Days * 10 + c - '0';
          newTimeSpan = new TimeSpan (
            newValue, // update number of days
            currentTimeSpan.Hours,
            currentTimeSpan.Minutes,
            currentTimeSpan.Seconds,
            currentTimeSpan.Milliseconds);
          break;
        case SelectionType.Hour:
          newValue = m_startEditBlockFlag ?
            c - '0' :
            currentTimeSpan.Hours * 10 + c - '0';
          if (newValue > 23) {
          return;
        }

        newTimeSpan = new TimeSpan (
            currentTimeSpan.Days,
            newValue, // update number of hours
            currentTimeSpan.Minutes,
            currentTimeSpan.Seconds,
            currentTimeSpan.Milliseconds);
          break;
        case SelectionType.Minute:
          newValue = m_startEditBlockFlag ?
            c - '0' :
            currentTimeSpan.Minutes * 10 + c - '0';
          if (newValue > 59) {
          return;
        }

        newTimeSpan = new TimeSpan (
            currentTimeSpan.Days,
            currentTimeSpan.Hours,
            newValue, // update number of minutes
            currentTimeSpan.Seconds,
            currentTimeSpan.Milliseconds);
          break;
        case SelectionType.Second:
          newValue = m_startEditBlockFlag ?
            c - '0' :
            currentTimeSpan.Seconds * 10 + c - '0';
          if (newValue > 59) {
          return;
        }

        newTimeSpan = new TimeSpan (
            currentTimeSpan.Days,
            currentTimeSpan.Hours,
            currentTimeSpan.Minutes,
            newValue, // update number of seconds
            currentTimeSpan.Milliseconds);
          break;
        case SelectionType.Millisecond:
          newValue = m_startEditBlockFlag ?
            c - '0' :
            currentTimeSpan.Milliseconds * 10 + c - '0';
          if (newValue > 999) {
          return;
        }

        newTimeSpan = new TimeSpan (
            currentTimeSpan.Days,
            currentTimeSpan.Hours,
            currentTimeSpan.Minutes,
            currentTimeSpan.Seconds,
            newValue); // update number of milliseconds
          break;
        case SelectionType.Invalid:
          // Nothing
          return;
      }
      m_startEditBlockFlag = false;

      // The new value must be in the allowed range
      if (newTimeSpan.TotalMilliseconds > (double)this.Maximum) {
        return;
      }

      // Update the value
      TimeSpanValue = newTimeSpan;
    }
    #endregion Event reactions
  }
}
