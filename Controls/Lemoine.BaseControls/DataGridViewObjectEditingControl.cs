// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.ComponentModel;
using System.Windows.Forms;

using Lemoine.Core.Log;

namespace Lemoine.BaseControls
{
  /// <summary>
  /// IDataGridViewEditingControl to update any kind of object
  /// </summary>
  public class DataGridViewObjectEditingControl: TextBox, IDataGridViewEditingControl
  {
    DataGridView m_dataGridView;
    object m_value;
    object m_initialValue;
    bool m_valueChanged = false;
    int m_rowIndex;

    static readonly ILog log = LogManager.GetLogger(typeof (DataGridViewObjectEditingControl).FullName);

    /// <summary>
    /// Constructor
    /// </summary>
    public DataGridViewObjectEditingControl()
    {
    }

    /// <summary>
    /// Value
    /// </summary>
    public object Value
    {
      get { return m_value; }
      set
      {
        m_value = value;
        m_initialValue = m_value;
      }
    }
    
    /// <summary>
    /// Implements the IDataGridViewEditingControl.EditingControlFormattedValue property.
    /// </summary>
    public object EditingControlFormattedValue
    {
      get { return m_value.ToString (); }
      set
      {
        String newValue = value as String;
        if (newValue != null)
        {
          try {
            this.Value = Parse (this.Text);
          }
          catch (Exception ex) {
            log.ErrorFormat ("EditingControlFormattedValue.set: " +
                             "{0}",
                             ex);
          }
        }
      }
    }

    /// <summary>
    /// Implements the
    /// IDataGridViewEditingControl.GetEditingControlFormattedValue method.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
    {
      return EditingControlFormattedValue;
    }

    /// <summary>
    /// Implements the
    /// IDataGridViewEditingControl.ApplyCellStyleToEditingControl method.
    /// </summary>
    /// <param name="dataGridViewCellStyle"></param>
    public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
    {
      this.Font = dataGridViewCellStyle.Font;
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl.EditingControlRowIndex
    /// property.
    /// </summary>
    public int EditingControlRowIndex
    {
      get { return m_rowIndex; }
      set { m_rowIndex = value; }
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl.EditingControlWantsInputKey
    /// method.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dataGridViewWantsInputKey"></param>
    /// <returns></returns>
    public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
    {
      return true;
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl.PrepareEditingControlForEdit
    /// method.
    /// </summary>
    /// <param name="selectAll"></param>
    public void PrepareEditingControlForEdit(bool selectAll)
    {
      // No preparation needs to be done.
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl
    /// .RepositionEditingControlOnValueChange property.
    /// </summary>
    public bool RepositionEditingControlOnValueChange
    {
      get { return false; }
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl
    /// .EditingControlDataGridView property.
    /// </summary>
    public DataGridView EditingControlDataGridView
    {
      get { return m_dataGridView; }
      set { m_dataGridView = value; }
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl
    /// .EditingControlValueChanged property.
    /// </summary>
    public bool EditingControlValueChanged
    {
      get { return m_valueChanged; }
      set { m_valueChanged = value; }
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl
    /// .EditingPanelCursor property.
    /// </summary>
    public Cursor EditingPanelCursor
    {
      get { return base.Cursor; }
    }
    
    /// <summary>
    /// Override OnTextChanged to update the value in the DataGridView
    /// </summary>
    /// <param name="e"></param>
    protected override void OnTextChanged (EventArgs e)
    {
      // Notify the DataGridView that the contents of the cell// have changed.
      try {
        m_value = Parse (this.Text);
      }
      catch (Exception ex) {
        log.WarnFormat ("OntTextChanged: " +
                        "parse failed with {0} for {1} " +
                        "=> use the initial value {2} instead",
                        ex, this.Text, m_initialValue);
        m_value = m_initialValue;
      }
      m_valueChanged = true;
      this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
    }
    
    /// <summary>
    /// Parse a given text according to the type of already recorded data
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    object Parse (string text)
    {
      if (m_value is int) {
        return int.Parse (text);
      }
      else if (m_value is double) {
        return double.Parse (text);
      }
      else if (m_value is bool) {
        return bool.Parse (text);
      }
      else if (m_value is TimeSpan) {
        return TimeSpan.Parse (text);
      }
      else if (m_value is String) {
        return (String) text;
      }
      else if (m_value is DayOfWeek) {
        return Enum.Parse (typeof (DayOfWeek), text);
      }
      else {
        log.ErrorFormat ("Parse: " +
                         "unsupported type for old={0} new={1}",
                         m_value, text);
        throw new NotSupportedException ();
      }
      
    }
  }
}
