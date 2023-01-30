// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Windows.Forms;
using Lemoine.I18N;
using Lemoine.Core.Log;
using Lemoine.ModelDAO.Interfaces;

namespace Lem_MachineStateTemplateGUI
{
  /// <summary>
  /// Description of MainForm.
  /// </summary>
  public partial class MainForm : Form
  {
    static readonly ILog log = LogManager.GetLogger(typeof (MainForm).FullName);
    
    /// <summary>
    /// Main entry
    /// </summary>
    public MainForm()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
    }

    private void MainForm_FormClosed (object sender, FormClosedEventArgs e)
    {
      Application.Exit ();
    }
  }
}
