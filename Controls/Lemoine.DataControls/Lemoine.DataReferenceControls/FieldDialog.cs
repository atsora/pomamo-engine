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
  /// Description of FieldDialog.
  /// </summary>
  public partial class FieldDialog : OKCancelDialog, IValueDialog<IField>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FieldDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Field a valid value ?
    /// </summary>
    public bool Nullable {
      get { return fieldSelection1.Nullable; }
      set { fieldSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  fieldSelection1.DisplayedProperty; }
      set { fieldSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected Field or null if no Field is selected
    /// </summary>
    public IField SelectedValue {
      get
      {
        return this.fieldSelection1.SelectedField;
      }
      set {
        this.fieldSelection1.SelectedField = value;
      }
    }
    
    /// <summary>
    /// Selected Fields or null if no Field is selected
    /// </summary>
    public IList<IField> SelectedValues {
      get
      {
        return this.fieldSelection1.SelectedFields;
      }
      set {
        this.fieldSelection1.SelectedFields = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FieldDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("FieldDialogTitle");
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
