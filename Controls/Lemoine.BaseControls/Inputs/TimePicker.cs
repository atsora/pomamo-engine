// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Forms;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// DateTimePicker showing only the time
  /// 
  /// Note: the milliseconds are not supported by Forms.DateTimePicker.
  /// If needed, an additional control must be used for that
  /// </summary>
  public class TimePicker : System.Windows.Forms.DateTimePicker
  {
    #region Members
    bool m_withSeconds = true;
    #endregion // Members

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TimePicker () : base ()
    {
      base.ShowUpDown = true;
      base.Format = DateTimePickerFormat.Custom;
      WithSeconds = true;
      UpdateCustomFormat ();
      this.MouseWheel += OnMouseWheel;
    }
    #endregion // Constructors

    #region Getters / Setters
    /// <summary>
    /// Override of "ShowUpDown", that does nothing
    /// </summary>
    [Browsable (false), EditorBrowsable (EditorBrowsableState.Never), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    new public bool ShowUpDown { get; set; }

    /// <summary>
    /// Override of "Format", that does nothing
    /// </summary>
    [Browsable (false), EditorBrowsable (EditorBrowsableState.Never), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    new public DateTimePickerFormat Format { get; set; }

    /// <summary>
    /// Override of "CustomFormat", that does nothing
    /// </summary>
    [Browsable (false), EditorBrowsable (EditorBrowsableState.Never), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
    new public string CustomFormat { get; set; }

    /// <summary>
    /// True will show the seconds
    /// </summary>
    [DefaultValue (true)]
    [Description ("Seconds can be shown or hidden.")]
    public bool WithSeconds
    {
      get { return m_withSeconds; }
      set
      {
        m_withSeconds = value;
        UpdateCustomFormat ();
      }
    }

    void UpdateCustomFormat ()
    {
      base.CustomFormat = m_withSeconds ?
        CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern :
        CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
    }

    /// <summary>
    /// Set the time
    /// </summary>
    public TimeSpan Time
    {
      get { return this.Value.TimeOfDay; }
      set { this.Value = DateTime.Now.Date.Add (value); }
    }
    #endregion // Getters / Setters

    #region Event reactions
    void OnMouseWheel (object sender, MouseEventArgs e)
    {
      SendKeys.Send (e.Delta > 0 ? "{Up}" : "{Down}");
    }
    #endregion // Event reactions
  }
}
