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
  /// Description of CncAcquisitionDialog.
  /// </summary>
  public partial class CncAcquisitionDialog : OKCancelDialog, IValueDialog<ICncAcquisition>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CncAcquisitionDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null CncAcquisition a valid value ?
    /// </summary>
    public bool Nullable {
      get { return cncAcquisitionSelection1.Nullable; }
      set { cncAcquisitionSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  cncAcquisitionSelection1.DisplayedProperty; }
      set { cncAcquisitionSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected CncAcquisition or null if no CncAcquisition is selected
    /// </summary>
    public ICncAcquisition SelectedValue {
      get
      {
        return this.cncAcquisitionSelection1.SelectedCncAcquisition;
      }
      set {
        this.cncAcquisitionSelection1.SelectedCncAcquisition = value;
      }
    }
    
    /// <summary>
    /// Selected CncAcquisitions or null no CncAcquisition is selected
    /// </summary>
    public IList<ICncAcquisition> SelectedValues {
      get {
        return this.cncAcquisitionSelection1.SelectedCncAcquisitions;
      }
      set {
        this.cncAcquisitionSelection1.SelectedCncAcquisitions = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CncAcquisitionDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("CncAcquisitionDialogTitle");
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
