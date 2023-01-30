// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// TimeSpanDialog used to input for TimeSpan data type.
  /// </summary>
  /// TODO: null return
  public partial class TimeSpanDialog : OKCancelDialog, IValueDialog<TimeSpan?>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (TimeSpanDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null TimeSpan a valid value ?
    /// </summary>
    public bool Nullable {
      get { return timeSpanSelection.Nullable; }
      set { timeSpanSelection.Nullable = value; }
    }
    
    /// <summary>
    /// Selected TimeSpan or null if no TimeSpan is selected
    /// </summary>
    public TimeSpan? SelectedValue { 
      get
      {
        return timeSpanSelection.SelectedTimeSpan;
      }
      set {
        timeSpanSelection.SelectedTimeSpan = value;
      }
    }
    
    /// <summary>
    /// No reason to be Implemented
    /// </summary>
    public System.Collections.Generic.IList<TimeSpan?> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TimeSpanDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      this.Text = PulseCatalog.GetString ("TimeSpanDialogTitle");
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
