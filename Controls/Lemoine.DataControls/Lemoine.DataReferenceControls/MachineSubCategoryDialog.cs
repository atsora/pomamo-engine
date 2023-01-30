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
  /// Description of MachineSubCategoryDialog.
  /// </summary>
  public partial class MachineSubCategoryDialog : OKCancelDialog, IValueDialog<IMachineSubCategory>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MachineSubCategoryDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null MachineSubCategory a valid value ?
    /// </summary>
    public bool Nullable {
      get { return machineSubCategorySelection1.Nullable; }
      set { machineSubCategorySelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  machineSubCategorySelection1.DisplayedProperty; }
      set { machineSubCategorySelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected MachineSubCategory or null if no MachineSubCategory is selected
    /// </summary>
    public IMachineSubCategory SelectedValue {
      get
      {
        return this.machineSubCategorySelection1.SelectedMachineSubCategory;
      }
      set {
        this.machineSubCategorySelection1.SelectedMachineSubCategory = value;
      }
    }
    
    /// <summary>
    /// Selected MachineSubCategory or null if no MachineSubCategory is selected
    /// </summary>
    public IList<IMachineSubCategory> SelectedValues {
      get
      {
        return this.machineSubCategorySelection1.SelectedMachineSubCategories;
      }
      set {
        this.machineSubCategorySelection1.SelectedMachineSubCategories = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineSubCategoryDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("MachineSubCategoryDialogTitle");
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
