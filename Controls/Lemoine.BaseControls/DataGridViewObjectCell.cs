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
  /// DataGridViewCell for generic objects
  /// </summary>
  public class DataGridViewObjectCell: DataGridViewTextBoxCell
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DataGridViewObjectCell).FullName);
    
    Type m_valueType = typeof (object);
    
    /// <summary>
    /// Constructor
    /// </summary>
    public DataGridViewObjectCell ()
      : base ()
    {
    }

    /// <summary>
    /// ValueType
    /// </summary>
    public override Type ValueType
    {
      get
      {
        Type valueType = base.ValueType;
        if (valueType != null)
        {
          return valueType;
        }
        return m_valueType;
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
      // Note: this.Value returns an exception rowIndex out of bount
      //       the following line is just a work around
      object v = this.DataGridView.CurrentCell.Value;
      base.InitializeEditingControl (rowIndex, initialFormattedValue, dataGridViewCellStyle);
      DataGridViewObjectEditingControl ctl =
        DataGridView.EditingControl as DataGridViewObjectEditingControl;
      ctl.Value = v;
      m_valueType = v.GetType ();
    }
    
    /// <summary>
    /// Method to implement to use a custom DataGridViewEditingControl
    /// </summary>
    public override Type EditType {
      get { return typeof (DataGridViewObjectEditingControl); }
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
      DataGridViewObjectEditingControl ctl =
        DataGridView.EditingControl as DataGridViewObjectEditingControl;
      return ctl.Value;
    }

    /// <summary>
    /// Override the Clone method
    /// </summary>
    /// <returns></returns>
    public override object Clone ()
    {
      DataGridViewObjectCell clone = base.Clone () as DataGridViewObjectCell;
      return clone;
    }
  }
}
