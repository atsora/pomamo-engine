// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;
using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Core.Log;

namespace Lemoine.ConfigControls
{
  /// <summary>
  /// Description of MachineStateTemplateWarningDialog.
  /// </summary>
  public partial class MachineStateTemplateWarningDialog : OKCancelMessageDialog
  {
    #region Members
    #endregion // Members

    static readonly ILog log = LogManager.GetLogger(typeof (MachineStateTemplateWarningDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Set the Title of the Dialog
    /// </summary>
    public String Title {
      get{ return this.Title; }
      set{ this.SetDialogTitle(value);}
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public MachineStateTemplateWarningDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
    }

    #endregion // Constructors

    #region Methods
    /// <summary>
    /// Used to set a Prefix on the Dialog\'s Title
    /// </summary>
    /// <param name="newDialogTitle"></param>
    private void SetDialogTitle(String newDialogTitle){
      this.Text = PulseCatalog.GetString("MachineStateTemplateDataIntegrityDialogWarningTitlePrefix")+newDialogTitle;
    }
    #endregion // Methods
  }
}
