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
  /// Description of ComputerDialog.
  /// </summary>
  public partial class ComputerDialog : OKCancelDialog, IValueDialog<IComputer>
  {
    static readonly ILog log = LogManager.GetLogger(typeof (ComputerDialog).FullName);

    #region Getters / Setters
    /// <summary>
    /// Is a null Computer a valid value ?
    /// </summary>
    public bool Nullable {
      get { return computerSelection1.Nullable; }
      set { computerSelection1.Nullable = value; }
    }
    
    /// <summary>
    /// Property that is displayed
    /// </summary>
    public string DisplayedProperty {
      get { return  computerSelection1.DisplayedProperty; }
      set { computerSelection1.DisplayedProperty = value; }
    }
    
    /// <summary>
    /// Selected Computer or null if no Computer is selected
    /// 
    /// Setable selection
    /// </summary>
    public IComputer SelectedValue {
      get
      {
        return this.computerSelection1.SelectedComputer;
      }
      set {
        this.computerSelection1.SelectedComputer = value;
      }
    }
    
    /// <summary>
    /// Selected Computers or null if no Computer is selected
    /// 
    /// Setable selection
    /// </summary>
    public IList<IComputer> SelectedValues {
      get {
        return this.computerSelection1.SelectedComputers;
      }
      set {
        this.computerSelection1.SelectedComputers = value;
      }
    }
    #endregion // Getters / Setters

    #region Constructors
    /// <summary>
    /// Description of the constructor
    /// </summary>
    public ComputerDialog()
    {
      //
      // The InitializeComponent() call is required for Windows Forms designer support.
      //
      InitializeComponent();
      
      //
      // Add constructor code after the InitializeComponent() call.
      //
      this.Text = PulseCatalog.GetString ("ComputerDialogTitle");
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
