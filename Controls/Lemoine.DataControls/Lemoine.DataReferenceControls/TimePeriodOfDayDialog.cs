// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of TimeSpanSelectionDialog.
  /// </summary>
  public partial class TimePeriodOfDayDialog : OKCancelDialog, IValueDialog<TimePeriodOfDay?>
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (TimePeriodOfDayDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return Selected TimePeriodOfDay
    /// </summary>
    public TimePeriodOfDay? SelectedValue{
      get {
        return timePeriodOfDaySelection.SelectedValue;
      }
      set {
        timePeriodOfDaySelection.SelectedValue = value;
      }
    }
    
    /// <summary>
    /// No existing reason to be Implemented
    /// </summary>
    public System.Collections.Generic.IList<TimePeriodOfDay?> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    
    /// <summary>
    /// Is a nullable value valid ?
    /// </summary>
    public bool Nullable {
      get {
        return this.timePeriodOfDaySelection.Nullable;
      }
      set {
        this.timePeriodOfDaySelection.Nullable = value;
      }
    }
    
    /// <summary>
    /// <see cref="Lemoine.DataReferenceControls.GenericSelection{T}">NoSelectionText</see> implementation
    /// </summary>
    public string NoSelectionText {
      get {
        return this.timePeriodOfDaySelection.NoSelectionText;
      }
      set {
        this.timePeriodOfDaySelection.NoSelectionText = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public TimePeriodOfDayDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      this.Text = PulseCatalog.GetString ("TimeSpanSelectionDialogTitle");
      
    }

    #endregion // Constructors

    #region Methods
        
    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      timePeriodOfDaySelection.Cancel ();
      this.DialogResult = DialogResult.Cancel;
    }
    #endregion // Methods
    
    void TimePeriodOfDayDialogLoad(object sender, EventArgs e)
    {
    }
  }
}
