// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a truncated (s) TimeSpan
  /// </summary>
  public partial class TimeSpanSelection : UserControl
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (TimeSpanSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return Selected TimeSpan
    /// </summary>
    public TimeSpan? SelectedTimeSpan
    {
      get {
        if (this.nullCheckBox.Checked) {
          return null;
        }
        return Truncate (this.dateTimePicker.Value, TimeSpan.FromSeconds (1)).TimeOfDay;
      }
      set {
        this.SetValue (value.Value);
      }
    }

    /// <summary>
    /// Is a null TimeSpan a valid value ?
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue (false), Description ("Is a null MachineObservation valid ?")]
    public bool Nullable
    {
      get { return nullCheckBox.Visible; }
      set {
        if (value) {
          tableLayoutPanel1.RowStyles[1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles[1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }

    #endregion // Getters / Setters

    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category ("Behavior"), Description ("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TimeSpanSelection ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      nullCheckBox.Text = PulseCatalog.GetString ("NullTimeSpan");

      this.Nullable = false;
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Raise the AfterSelect event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAfterSelect (EventArgs e)
    {
      if (null != AfterSelect) {
        AfterSelect (this, e);
      }
    }

    /// <summary>
    /// Set the selectedValue for ease user modification
    /// </summary>
    /// <param name="o"></param>
    private void SetValue (object o)
    {
      TimeSpan timeSpan = (TimeSpan)o;
      this.dateTimePicker.Value =
        new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    /// <summary>
    /// Remove a part of the time 
    /// </summary>
    /// <param name="dateTime"></param>
    /// <param name="timeSpan"></param>
    /// <returns>DateTime</returns>
    private DateTime Truncate (DateTime dateTime, TimeSpan timeSpan)
    {
      if (timeSpan == TimeSpan.Zero) {
        return dateTime;
      }

      return dateTime.AddTicks (-(dateTime.Ticks % timeSpan.Ticks));
    }
    #endregion // Methods

    void TimeSpanSelectionLoad (object sender, EventArgs e)
    {
    }

    void NullCheckBoxCheckedChanged (object sender, EventArgs e)
    {
      if (nullCheckBox.Checked) {
        dateTimePicker.Enabled = false;
      }
      else {
        dateTimePicker.Enabled = true;
      }

      OnAfterSelect (new EventArgs ());
    }

    void ListBoxSelectedIndexChanged (object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
