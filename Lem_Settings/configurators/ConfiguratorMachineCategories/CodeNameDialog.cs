// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.Core.Log;

namespace ConfiguratorMachineCategories
{
  /// <summary>
  /// Description of CodeNameDialog.
  /// </summary>
  public partial class CodeNameDialog : Form
  {
    static readonly ILog log = LogManager.GetLogger(typeof (CodeNameDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Name
    /// </summary>
    public string ElementName {
      get { return textName.Text; }
      set { textName.Text = value; }
    }
    
    /// <summary>
    /// Code
    /// </summary>
    public string ElementCode {
      get { return textCode.Text; }
      set { textCode.Text = value; }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public CodeNameDialog() : base()
    {
      this.StartPosition = FormStartPosition.CenterParent;
      if (null != Form.ActiveForm) {
        this.Icon = Form.ActiveForm.Icon;
      }
      InitializeComponent();
    }
    #endregion // Constructors
    
    #region Event reactions
    void ButtonOkClick(object sender, EventArgs e)
    {
      if (textName.Text == "") {
        MessageBoxCentered.Show(this, "The name cannot be empty", "Warning",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }
      else {
        DialogResult = DialogResult.OK;
        Close();
      }
    }
    #endregion // Event reactions
  }
}
