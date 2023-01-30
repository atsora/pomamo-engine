// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Control to select a date/time
  /// </summary>
  public partial class DateTimeSelection : UserControl
  {
    #region Members
    string m_noTi18n = "DateTimeNull";
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (DateTimeSelection).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is null a valid value ?
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue(false), Description("Is a null date/time valid ?")]
    public bool Nullable {
      get { return nullCheckBox.Visible; }
      set
      {
        if (value) {
          tableLayoutPanel1.RowStyles [1].Height = 30;
          nullCheckBox.Visible = true;
        }
        else {
          tableLayoutPanel1.RowStyles [1].Height = 0;
          nullCheckBox.Visible = false;
        }
      }
    }
    
    /// <summary>
    /// I18N Key for No Selection text
    /// </summary>
    [Category("Configuration"), Browsable(true), DefaultValue("DateTimeNull"), Description("I18N for No date/time")]
    public string NoSelectionText {
      get {
        return this.m_noTi18n;
      }
      set {
        this.m_noTi18n = value;
      }
    }
    
    /// <summary>
    /// Local date/time
    /// </summary>
    public DateTime? LocalDateTime {
      get
      {
        if (nullCheckBox.Checked) {
          return null;
        }
        else {
          return DateTime.SpecifyKind (monthCalendar.SelectionStart.Add (timePicker.Value.TimeOfDay),
                                       DateTimeKind.Local);
        }
      }
      set {
        if (!value.HasValue) {
          nullCheckBox.Checked = true;
        }
        else {
          nullCheckBox.Checked = false;
          DateTime local = value.Value;
          if (DateTimeKind.Unspecified == local.Kind) {
            local = DateTime.SpecifyKind (local, DateTimeKind.Local);
          }
          local = local.ToLocalTime ();
          monthCalendar.SelectionStart = monthCalendar.SelectionEnd = local.Date;
          timePicker.Value = local;
        }
      }
    }
    
    /// <summary>
    /// UTC date/time
    /// </summary>
    public DateTime? UtcDateTime {
      get
      {
        if (!this.LocalDateTime.HasValue) {
          return null;
        }
        else {
          return this.LocalDateTime.Value.ToUniversalTime ();
        }
      }
      set
      {
        if (!value.HasValue) {
          this.LocalDateTime = null;
        }
        else {
          Debug.Assert (DateTimeKind.Utc == value.Value.Kind);
          DateTime utc = value.Value;
          if (DateTimeKind.Unspecified == utc.Kind) {
            utc = DateTime.SpecifyKind (utc, DateTimeKind.Utc);
          }
          utc = utc.ToUniversalTime ();
          this.LocalDateTime = utc.ToLocalTime ();
        }
      }
    }
    #endregion // Getters / Setters
    
    #region Events
    /// <summary>
    /// Selection changed
    /// </summary>
    [Category("Behavior"), Description("Raised after a selection")]
    public event EventHandler AfterSelect;
    #endregion // Events

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DateTimeSelection()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
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
    #endregion // Methods
    
    void DateTimeSelectionLoad(object sender, EventArgs e)
    {
      this.LocalDateTime = DateTime.Now;
      nullCheckBox.Text = PulseCatalog.GetString (m_noTi18n);
    }
    
    void NullCheckBoxCheckedChanged(object sender, EventArgs e)
    {
      monthCalendar.Enabled = timePicker.Enabled = !nullCheckBox.Checked;
      
      OnAfterSelect (new EventArgs ());
    }
    
    void ListBoxSelectedIndexChanged(object sender, EventArgs e)
    {
      OnAfterSelect (new EventArgs ());
    }
  }
}
