// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of ShiftDialog.
  /// </summary>
  public partial class ShiftDialog : OKCancelDialog, IValueDialog<IShift>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ShiftDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Shift a valid value ?
    /// </summary>
    public bool Nullable {
      get { return ShiftSelection1.Nullable; }
      set { ShiftSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  ShiftSelection1.DisplayedProperty; }
      set { ShiftSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// First selected Shift or null if no Shift is selected
    /// </summary>
    public IShift SelectedValue {
      get
      {
        return this.ShiftSelection1.SelectedShift;
      }
      set {
        this.ShiftSelection1.SelectedShift = value;
      }
    }
    
    /// <summary>
    /// Selected Shifts
    /// </summary>
    public IList<IShift> SelectedValues {
      get
      {
        return this.ShiftSelection1.SelectedShifts;        
      }
      set {
        this.ShiftSelection1.SelectedShifts = value;
      }
    }

    /// <summary>
    /// allow/disallow multi-selection
    /// </summary>
    public bool MultiSelect {
      get { return ShiftSelection1.MultiSelect; }
      set { ShiftSelection1.MultiSelect = value; }
    }
    
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ShiftDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("ShiftDialogTitle");
    }
    #endregion // Constructors

    #region Methods
    #endregion // Methods
    
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
  }
}
