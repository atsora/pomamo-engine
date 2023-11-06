// Copyright (C) 2009-2023 Lemoine Automation Technologies
//
// SPDX-License-Identifier: Apache-2.0

using System;
using System.Drawing;
using System.Windows.Forms;

using Lemoine.BaseControls;
using Lemoine.I18N;
using Lemoine.Model;
using Lemoine.Core.Log;

namespace Lemoine.DataReferenceControls
{
  /// <summary>
  /// Description of FileRepositoryDialog.
  /// </summary>
  public partial class FileRepositoryDialog : OKCancelDialog, IValueDialog<string>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (FileRepositoryDialog).FullName);

    /// <summary>
    /// FileRepository namespace
    /// </summary>
    public string NSpace {
      get { return fileSelection1.NSpace; }
      set { fileSelection1.NSpace = value; }
    }
    
    /// <summary>
    /// FileRepository path
    /// </summary>
    public string Path {
      get { return fileSelection1.Path; }
      set { fileSelection1.Path = value; }
    }

    /// <summary>
    /// Is a null File a valid value ?
    /// </summary>
    public bool Nullable {
      get { return fileSelection1.Nullable; }
      set { fileSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Selected File or null if no File is selected
    /// </summary>
    public string SelectedValue {
      get
      {
        if (fileSelection1.SelectedFiles.Count < 1) {
          return null;
        }
        else {
          return fileSelection1.SelectedFiles [0];
        }
      }
      set {;}
    }
    
    /// <summary>
    /// No reason to be Implemented
    /// </summary>
    public System.Collections.Generic.IList<string> SelectedValues {
      get {
        throw new NotImplementedException();
      }
      set {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    /// Description of the constructor
    /// </summary>
    public FileRepositoryDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("FileRepositoryDialogTitle");
    }

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
