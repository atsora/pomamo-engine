// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.Model;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// IDataGridViewEditingControl to open a selection dialog
  /// </summary>
  public class DataGridViewDialogEditingControl<T>: TextBox, IDataGridViewEditingControl
  {
    IValueDialog<T> m_dialog = null;
    DataGridView m_dataGridView;
    T m_selectedValue = default(T);
    bool m_valueChanged = false;
    int m_rowIndex;

    /// <summary>
    /// Constructor
    /// </summary>
    public DataGridViewDialogEditingControl()
    {
      this.Click += this.OnClickHandler;
      this.Enter += this.OnClickHandler;
      this.ReadOnly = true;
    }
    
    /// <summary>
    /// Associated dialog
    /// </summary>
    public IValueDialog<T> Dialog
    {
      get { return m_dialog; }
      set { m_dialog = value; }
    }
    
    /// <summary>
    /// Value that was selected in the control
    /// </summary>
    public T SelectedValue {
      get { return m_selectedValue; }
    }

    /// <summary>
    /// Implements the IDataGridViewEditingControl.EditingControlFormattedValue property.
    /// </summary>
    public object EditingControlFormattedValue
    {
      get { return this.Text; }
      set
      {
        m_selectedValue = (T) value;
        this.Text = GetText (m_selectedValue);
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
      return false;
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
    /// What to do in case the user click on the text box
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected virtual void OnClickHandler(object sender, EventArgs e)
    {
      if (null != m_dialog) {
        if (DialogResult.OK == m_dialog.ShowDialog ()) {
          if (!object.Equals (this.Text, GetText (m_dialog.SelectedValue))) {
            m_selectedValue = m_dialog.SelectedValue;
            m_valueChanged = true;
            this.EditingControlDataGridView.NotifyCurrentCellDirty(true);
            this.Text = GetText (m_selectedValue);
          }
        }
      }
    }
    
    /// <summary>
    /// Get the text from the object
    /// 
    /// If the object is a ISelectionable object, SelectionText property is used, else the ToString() method
    /// </summary>
    /// <param name="v"></param>
    /// <returns></returns>
    protected virtual string GetText (T v)
    {
      if (null == v) {
        return "";
      }
      else if (v is ISelectionable) {
        return ((ISelectionable) v).SelectionText;
      }
      else {
        return v.ToString ();
      }
    }
  }
}
