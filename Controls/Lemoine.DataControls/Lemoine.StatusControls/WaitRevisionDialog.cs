// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Linq;
using System.Windows.Forms;
using Lemoine.Model;

namespace Lemoine.StatusControls
{
  /// <summary>
  /// Description of WaitDialog.
  /// </summary>
  public partial class WaitRevisionDialog : Form
  {
    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public WaitRevisionDialog(IRevision revision)
    {
      this.StartPosition = FormStartPosition.CenterParent;
      if (Form.ActiveForm != null) {
        this.Icon = Form.ActiveForm.Icon;
      }

      InitializeComponent ();
      progressBar.Finished += OnFinished;
      progressBar.TimeOut += OnTimeOut;
      progressBar.AddModifications(revision.GlobalModifications.Cast<IModification>());
      progressBar.AddModifications(revision.MachineModifications.Cast<IModification>());
      progressBar.AnalysisProgressLoad();
    }
    #endregion // Constructors
    
    #region Getters / Setters
    /// <summary>
    /// Timeout duration in seconds
    /// By default 0: no timeout
    /// </summary>
    public int TimeOutDuration {
      get { return progressBar.TimeOutDuration; }
      set { progressBar.TimeOutDuration = value; }
    }
    #endregion // Getters / Setters
    
    #region Event reactions
    void OnTimeOut()
    {
      this.DialogResult = DialogResult.Cancel;
    }
    
    void OnFinished()
    {
      this.DialogResult = DialogResult.OK;
    }
    
    void ButtonCancelClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.Cancel;
    }
    #endregion // Event reactions
  }
}
