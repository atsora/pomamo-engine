// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Windows.Forms;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// DataGridViewCell for all the ISelectionable objects
  /// </summary>
  public class DataGridViewSelectionableCell<T>: DataGridViewTextBoxCell
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DataGridViewSelectionableCell<T>).FullName);

    IValueDialog<T> m_dialog = null;
    
    /// <summary>
    /// Constructor
    /// </summary>
    public DataGridViewSelectionableCell ()
      : base ()
    {
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dialog">Associated dialog</param>
    public DataGridViewSelectionableCell (IValueDialog<T> dialog)
      : base ()
    {
      m_dialog = dialog;
    }
    
    static readonly Type DEFAULT_VALUE_TYPE = typeof (T);

    /// <summary>
    /// ValueType
    /// </summary>
    public override Type ValueType
    {
      get
      {
        Type valueType = base.ValueType;
        if (null != valueType) {
          return valueType;
        }
        return DEFAULT_VALUE_TYPE;
      }
    }
    
    /// <summary>
    /// Method to implement to use a custom DataGridViewEditingControl
    /// </summary>
    /// <param name="value"></param>
    /// <param name="rowIndex"></param>
    /// <param name="cellStyle"></param>
    /// <param name="valueTypeConverter"></param>
    /// <param name="formattedValueTypeConverter"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override object GetFormattedValue(object value,
                                                int rowIndex,
                                                ref DataGridViewCellStyle cellStyle,
                                                TypeConverter valueTypeConverter,
                                                TypeConverter formattedValueTypeConverter,
                                                DataGridViewDataErrorContexts context)
    {
      ISelectionable selectionable = value as ISelectionable;
      if (null != selectionable) {
        return base.GetFormattedValue (selectionable.SelectionText,
                                       rowIndex,
                                       ref cellStyle,
                                       valueTypeConverter,
                                       formattedValueTypeConverter,
                                       context);
      }
      else {
        return base.GetFormattedValue (value,
                                       rowIndex,
                                       ref cellStyle,
                                       valueTypeConverter,
                                       formattedValueTypeConverter,
                                       context);
      }
    }
    
    /// <summary>
    /// Method to implement to use a custom DataGridViewEditingControl
    /// </summary>
    /// <param name="rowIndex"></param>
    /// <param name="initialFormattedValue"></param>
    /// <param name="dataGridViewCellStyle"></param>
    public override void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
    {
      base.InitializeEditingControl (rowIndex, initialFormattedValue, dataGridViewCellStyle);
      DataGridViewDialogEditingControl<T> ctl =
        DataGridView.EditingControl as DataGridViewDialogEditingControl<T>;
      ctl.Dialog = m_dialog;
      if(this.DataGridView.Rows[rowIndex].Cells[this.ColumnIndex].Value != null){
        if(this.DataGridView.Rows[rowIndex].Cells[this.ColumnIndex].Value is IList &&
           this.DataGridView.Rows[rowIndex].Cells[this.ColumnIndex].Value.GetType().IsGenericType){
          ctl.Dialog.SelectedValues = (System.Collections.Generic.List<T>)this.DataGridView.Rows[rowIndex].Cells[this.ColumnIndex].Value;
        }
        else {
          ctl.Dialog.SelectedValue = (T)this.DataGridView.Rows[rowIndex].Cells[this.ColumnIndex].Value;
        }
      }
        
    }

    /// <summary>
    /// Method to implement to use a custom DataGridViewEditingControl
    /// </summary>
    public override Type EditType {
      get { return typeof (DataGridViewDialogEditingControl<T>); }
    }
    
    /// <summary>
    /// Method to implement to use a custom DataGridViewEditingControl
    /// </summary>
    public override object DefaultNewRowValue
    {
      get { return null; }
    }
    
    /// <summary>
    /// Method to implement to use a custom DataGridViewEditingControl
    /// </summary>
    /// <param name="formattedValue"></param>
    /// <param name="cellStyle"></param>
    /// <param name="formattedValueTypeConverter"></param>
    /// <param name="valueTypeConverter"></param>
    /// <returns></returns>
    public override object ParseFormattedValue (object formattedValue, DataGridViewCellStyle cellStyle,
                                                System.ComponentModel.TypeConverter formattedValueTypeConverter,
                                                System.ComponentModel.TypeConverter valueTypeConverter)
    {
      DataGridViewDialogEditingControl<T> ctl =
        DataGridView.EditingControl as DataGridViewDialogEditingControl<T>;
      return ctl.SelectedValue;
    }
    
    /// <summary>
    /// Override the Clone method
    /// </summary>
    /// <returns></returns>
    public override object Clone ()
    {
      DataGridViewSelectionableCell<T> clone = base.Clone () as DataGridViewSelectionableCell<T>;
      clone.m_dialog = m_dialog;
      return clone;
    }
  }
}
