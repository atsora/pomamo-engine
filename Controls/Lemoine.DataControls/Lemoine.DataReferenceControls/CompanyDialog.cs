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
  /// Description of CompanyDialog.
  /// </summary>
  public partial class CompanyDialog : OKCancelDialog, IValueDialog<ICompany>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CompanyDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Company a valid value ?
    /// </summary>
    public bool Nullable {
      get { return companySelection1.Nullable; }
      set { companySelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  companySelection1.DisplayedProperty; }
      set { companySelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected Company or null if no Company is selected
    /// 
    /// Setable selection
    /// </summary>
    public ICompany SelectedValue {
      get
      {
        return this.companySelection1.SelectedCompany;
      }
      set {
        this.companySelection1.SelectedCompany = value;
      }
    }
    
    /// <summary>
    /// Selected Companies or null if no Companies is selected
    /// 
    /// Setable selection
    /// </summary>
    public IList<ICompany> SelectedValues {
      get {
        return this.companySelection1.SelectedCompanies;
      }
      set {
        this.companySelection1.SelectedCompanies = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CompanyDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("CompanyDialogTitle");
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
