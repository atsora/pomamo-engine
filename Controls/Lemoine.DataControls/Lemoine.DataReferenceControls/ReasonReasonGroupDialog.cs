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
  /// Description of ReasonReasonGroupDialog.
  /// </summary>
  public partial class ReasonReasonGroupDialog : OKCancelDialog, IValueDialog<IReason>
  {
    #region Members
    bool m_canSelectReasonGroup = false;
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (ReasonReasonGroupDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null reason or reason group a valid value ?
    /// </summary>
    public bool Nullable {
      get { return reasonReasonGroupSelection1.Nullable; }
      set { reasonReasonGroupSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Can a reason group be selected ?
    /// </summary>
    public bool CanSelectReasonGroup {
      get { return m_canSelectReasonGroup; }
      set
      {
        m_canSelectReasonGroup = value;
        if (m_canSelectReasonGroup) {
          this.Text = PulseCatalog.GetString ("ReasonReasonGroupDialogTitleWithReasonGroup");
        }
        else {
          this.Text = PulseCatalog.GetString ("ReasonReasonGroupDialogTitle");
        }
      }
    }
    
    /// <summary>
    /// Property to use to display a reason
    /// 
    /// Default is Display
    /// </summary>
    public string ReasonDisplayedProperty {
      get { return  reasonReasonGroupSelection1.ReasonDisplayedProperty; }
      set { reasonReasonGroupSelection1.ReasonDisplayedProperty = value; }
    }
    
    /// <summary>
    /// Property to use to display a reason group
    /// 
    /// Default is Display
    /// </summary>
    public string ReasonGroupDisplayedProperty {
      get { return  reasonReasonGroupSelection1.ReasonGroupDisplayedProperty; }
      set { reasonReasonGroupSelection1.ReasonGroupDisplayedProperty = value; }
    }    
    
    /// <summary>
    /// Selected reason or reason group
    /// or null if no reason / reason group is selected
    /// 
    /// Setable selection
    /// </summary>
    public object SelectedReasonReasonGroup {
      get
      {
        return reasonReasonGroupSelection1.SelectedReasonReasonGroup;
      }
      set {
        reasonReasonGroupSelection1.SelectedReasonReasonGroup = value;
      }
    }
    
    /// <summary>
    /// Not Implemented
    /// </summary>
    public System.Collections.Generic.IList<IReason> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }
    
    /// <summary>
    /// Selected reason only (null if a reason group id selected)
    /// </summary>
    public IReason SelectedValue {
      get { return SelectedReasonReasonGroup as IReason; }
      set { SelectedReasonReasonGroup = value;}
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ReasonReasonGroupDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("ReasonReasonGroupDialogTitle");
    }
    #endregion // Constructors

    void OkButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void CancelButtonClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    
    void ReasonReasonGroupSelection1AfterSelect(object sender, EventArgs e)
    {
      if (!this.CanSelectReasonGroup
          && (null != reasonReasonGroupSelection1.SelectedReasonReasonGroup)
          && (reasonReasonGroupSelection1.SelectedReasonReasonGroup is IReasonGroup)) {
        okButton.Enabled = false;
      }
      else {
        okButton.Enabled = true;
      }
    }
  }
}
