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
  /// Description of CadModelDialog.
  /// </summary>
  public partial class CadModelDialog : OKCancelDialog, IValueDialog<ICadModel>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CadModelDialog).FullName);

    #region members

    #endregion
    
    #region Getters / Setters
    /// <summary>
    /// Is a null CadModel a valid value ?
    /// </summary>
    public bool Nullable {
      get { return cadModelSelection1.Nullable; }
      set { cadModelSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  cadModelSelection1.DisplayedProperty; }
      set { cadModelSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected CadModel or null if no CadModel is selected
    /// </summary>
    public ICadModel SelectedValue {
      get
      {
        return cadModelSelection1.SelectedCadModel;
      }
      set {
        cadModelSelection1.SelectedCadModel = value;
      }
    }
    
    /// <summary>
    /// Selected CadModels or null
    /// </summary>
    public IList<ICadModel> SelectedValues {
      get {
        return cadModelSelection1.SelectedCadModels;
      }
      set {
        cadModelSelection1.SelectedCadModels = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CadModelDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString ("CadModelDialogTitle");
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
