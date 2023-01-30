// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of EventCncValueInputDialog.
  /// </summary>
  public partial class EventCncValueInputDialog : OKCancelDialog
  {
    static readonly ILog log = LogManager.GetLogger(typeof (EventCncValueInputDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Return Name of EventCncValue
    /// </summary>
    public string SelectedName {
      get
      {
        return eventCncValueConfigInput.SelectedName;
      }
    }
    
    /// <summary>
    /// Return Message of EventCncValue
    /// </summary>
    public string SelectedMessage {
      get {
        return eventCncValueConfigInput.SelectedMessage;
      }
    }
    
    /// <summary>
    /// Return Condition of EventCncValue
    /// </summary>
    public string SelectedCondition {
      get {
        return eventCncValueConfigInput.SelectedCondition;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public EventCncValueInputDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();

      this.Text = PulseCatalog.GetString ("EventCncValueInputDialogTitle");
      
      this.okButton.Enabled = false;
      this.eventCncValueConfigInput.OkButton = this.okButton;
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
