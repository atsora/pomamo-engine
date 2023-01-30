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
  /// Description of MachineCategoryDialog.
  /// </summary>
  public partial class MachineCategoryDialog : OKCancelDialog, IValueDialog<IMachineCategory>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineCategoryDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineCategory a valid value ?
    /// </summary>
    public bool Nullable {
      get { return machineCategorySelection1.Nullable; }
      set { machineCategorySelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineCategorySelection1.DisplayedProperty; }
      set { machineCategorySelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected MachineCategory or null if no MachineCategory is selected
    /// </summary>
    public IMachineCategory SelectedValue {
      get
      {
        return this.machineCategorySelection1.SelectedMachineCategory;
      }
      set {
        this.machineCategorySelection1.SelectedMachineCategory = value;
      }
    }
    
    /// <summary>
    /// Selected MachineCategories or null if no MachineCategories is selected
    /// </summary>
    public IList<IMachineCategory> SelectedValues {
      get
      {
        return this.machineCategorySelection1.SelectedMachineCategories;
      }
      set {
        this.machineCategorySelection1.SelectedMachineCategories = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineCategoryDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MachineCategoryDialogTitle");
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
