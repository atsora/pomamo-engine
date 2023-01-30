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
  /// Description of UnitDialog.
  /// </summary>
  public partial class UnitDialog : OKCancelDialog, IValueDialog<IUnit>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (UnitDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Unit a valid value ?
    /// </summary>
    public bool Nullable {
      get { return unitSelection1.Nullable; }
      set { unitSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  unitSelection1.DisplayedProperty; }
      set { unitSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected Unit or null if no Unit is selected
    /// </summary>
    public IUnit SelectedValue {
      get
      {
        return this.unitSelection1.SelectedUnit;
      }
      set {
        this.unitSelection1.SelectedUnit = value;
      }
    }
    
    /// <summary>
    /// Selected Units or null if no Unit is selected
    /// </summary>
    public IList<IUnit> SelectedValues {
      get
      {
        return this.unitSelection1.SelectedUnits;
      }
      set {
        this.unitSelection1.SelectedUnits = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public UnitDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("UnitDialogTitle");
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
