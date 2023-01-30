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
  /// Description of RoleDialog.
  /// </summary>
  public partial class RoleDialog : OKCancelDialog, IValueDialog<IRole>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (RoleDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Role a valid value ?
    /// </summary>
    public bool Nullable {
      get { return RoleSelection1.Nullable; }
      set { RoleSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  RoleSelection1.DisplayedProperty; }
      set { RoleSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// First selected Role or null if no Role is selected
    /// </summary>
    public IRole SelectedValue {
      get
      {
        return this.RoleSelection1.SelectedRole;
      }
      set {
        this.RoleSelection1.SelectedRole = value;
      }
    }
    
    /// <summary>
    /// Selected Roles
    /// </summary>
    public IList<IRole> SelectedValues {
      get
      {
        return this.RoleSelection1.SelectedRoles;        
      }
      set {
        this.RoleSelection1.SelectedRoles = value;
      }
    }

    /// <summary>
    /// allow/disallow multi-selection
    /// </summary>
    public bool MultiSelect {
      get { return RoleSelection1.MultiSelect; }
      set { RoleSelection1.MultiSelect = value; }
    }
    
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public RoleDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("RoleDialogTitle");
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
