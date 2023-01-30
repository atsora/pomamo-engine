// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of TimePeriodOfDaySelection.
  /// </summary>
  public partial class TimePeriodOfDaySelection : UserControl
  {
    #region Members
    string m_noTi18n = "TimePeriodOfDayNull";
    bool m_validate = true;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger (typeof (TimePeriodOfDaySelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is null a valid value ?
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue (false), Description ("Is a null date/time valid ?")]
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

    /// <summary>
    /// I18N Key for No Selection text
    /// </summary>
    [Category ("Configuration"), Browsable (true), DefaultValue ("DateTimeNull"), Description ("I18N for No date/time")]
    public string NoSelectionText
    {
      get {
        return this.m_noTi18n;
      }
      set {
        this.m_noTi18n = value;
      }
    }

    /// <summary>
    /// Get/Set the TimePeriodOfDay
    /// </summary>
    public TimePeriodOfDay? SelectedValue
    {
      get {
        if (nullCheckBox.Checked) {
          return null;
        }
        else {
          TimePeriodOfDay v =
            new TimePeriodOfDay (Truncate (this.beginDateTimePicker.Value, TimeSpan.FromSeconds (1)).TimeOfDay,
                                Truncate (this.endDateTimePicker.Value, TimeSpan.FromSeconds (1)).TimeOfDay);
          return v;
        }
      }
      set {
        if (!value.HasValue) {
          nullCheckBox.Checked = true;
        }
        else {
          this.SetValue (value);
        }
      }
    }
    #endregion // Getters / Setters

    #region Events
    /// <summary>
    /// Value changed
    /// </summary>
    [Category ("Behavior"), Description ("Raised after a value modification")]
    public event EventHandler AfterChanged;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TimePeriodOfDaySelection ()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent ();

      this.Nullable = false;

      errorProvider1.SetIconAlignment (endDateTimePicker, ErrorIconAlignment.MiddleRight);
      errorProvider1.SetIconPadding (endDateTimePicker, 0);
    }
    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Cancel the control: do not validate it
    /// </summary>
    public void Cancel ()
    {
      m_validate = false;
    }

    /// <summary>
    /// Raise the AfterChanged event
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnAfterChanged (EventArgs e)
    {
      if (null != AfterChanged) {
        AfterChanged (this, e);
      }
    }
    void TimePeriodOfDaySelectionLoad (object sender, EventArgs e)
    {
      beginDateTimePicker.Value = DateTime.Now.Date;
      endDateTimePicker.Value = DateTime.Now.Date;
      nullCheckBox.Text = PulseCatalog.GetString (m_noTi18n);
    }

    /// <summary>
    /// Set the selectedValue for ease user modification
    /// </summary>
    /// <param name="o"></param>
    private void SetValue (object o)
    {
      TimePeriodOfDay timePeriodOfDay = (TimePeriodOfDay)o;
      this.beginDateTimePicker.Value =
        new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                     timePeriodOfDay.Begin.Hours, timePeriodOfDay.Begin.Minutes,
                     timePeriodOfDay.Begin.Seconds);
      this.endDateTimePicker.Value =
        new DateTime (DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day,
                     timePeriodOfDay.End.Hours, timePeriodOfDay.End.Minutes,
                     timePeriodOfDay.End.Seconds);
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

    void BeginDateTimePickerValueChanged (object sender, EventArgs e)
    {
      OnAfterChanged (new EventArgs ());
    }

    void EndDateTimePickerValueChanged (object sender, EventArgs e)
    {
      OnAfterChanged (new EventArgs ());
    }

    void NullCheckBoxCheckedChanged (object sender, EventArgs e)
    {
      beginDateTimePicker.Enabled = endDateTimePicker.Enabled = !nullCheckBox.Checked;

      OnAfterChanged (new EventArgs ());
    }
    #endregion // Methods

    void TimePeriodOfDaySelectionValidating (object sender, CancelEventArgs e)
    {
      if (m_validate
          && !nullCheckBox.Checked
          && (0 != endDateTimePicker.Value.TimeOfDay.Ticks)
          && (endDateTimePicker.Value.TimeOfDay.TotalSeconds <= beginDateTimePicker.Value.TimeOfDay.TotalSeconds)) {
        e.Cancel = true;
        // TODO: errorProvider does not work
        errorProvider1.SetError (endDateTimePicker, PulseCatalog.GetString ("InvalidTimePeriodOfDay"));
        MessageBox.Show (PulseCatalog.GetString ("InvalidTimePeriodOfDay"), PulseCatalog.GetString ("InvalidValue"),
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    void TimePeriodOfDaySelectionValidated (object sender, EventArgs e)
    {
      errorProvider1.SetError (endDateTimePicker, "");
    }

  }
}
