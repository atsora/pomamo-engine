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
  /// Description of DepartmentDialog.
  /// </summary>
  public partial class DepartmentDialog : OKCancelDialog, IValueDialog<IDepartment>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (DepartmentDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Department a valid value ?
    /// </summary>
    public bool Nullable {
      get { return departmentSelection1.Nullable; }
      set { departmentSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  departmentSelection1.DisplayedProperty; }
      set { departmentSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected Department or null if no Department is selected
    /// </summary>
    public IDepartment SelectedValue {
      get
      {
        return this.departmentSelection1.SelectedDepartment;
      }
      set {
        this.departmentSelection1.SelectedDepartment = value;
      }
    }
    
    /// <summary>
    /// Selected Departments or null if no Department is selected
    /// </summary>
    public IList<IDepartment> SelectedValues {
      get {
        return this.departmentSelection1.SelectedDepartments;
      }
      set {
        this.departmentSelection1.SelectedDepartments = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public DepartmentDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("DepartmentDialogTitle");
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
